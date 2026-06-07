using System.Text.Json;
using System.Security.Claims;
using FavouriteBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using FavouriteBookstore.Services;
using FavouriteBookstore.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace FavouriteBookstore.Controllers
{
    [ApiController]
    [Route("api")]
    public class StoreApiController : ControllerBase
    {
        private readonly BookstoreSystem _bookstoreSystem;

        public StoreApiController(BookstoreSystem bookstoreSystem)
        {
            _bookstoreSystem = bookstoreSystem;
        }

        [HttpGet("books")]
        public ActionResult<List<BookDto>> GetBooks()
        {
            List<BookDto> catalogueBooks = _bookstoreSystem
                .GetCatalogueBooks()
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    ISBN = book.ISBN,
                    Price = book.Price,
                    Stock = book.Stock,
                    Title = book.Name,
                    Author = book.Author,
                    Genre = book.Genre,
                    Publisher = book.Publisher
                })
                .ToList();

            return Ok(catalogueBooks);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Name, email and password are required." });
            }

            bool success = _bookstoreSystem.AccountManager.RegisterAccount(request.Email.Trim().ToLowerInvariant(), request.Password, request.Name.Trim(), "Customer");
            if (!success)
            {
                return Conflict(new { message = "An account already exists for this email address." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.Email.Trim().ToLowerInvariant()),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Ok(new UserDto(request.Name.Trim(), request.Email.Trim().ToLowerInvariant(), "Customer", new List<string>()));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            bool success = _bookstoreSystem.AccountManager.Login(request.Email.Trim().ToLowerInvariant(), request.Password);
            if (!success)
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }

            var user = _bookstoreSystem.AccountManager.CurrentSessionUser;
            if (user == null) return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Ok(new UserDto(user.Name, user.Email, user.Role, GetCartIdsForUser(user)));
        }

        [HttpPost("guest")]
        public async Task<IActionResult> GuestLogin()
        {
            Guest guest = new Guest();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, guest.Email),
                new Claim(ClaimTypes.Role, guest.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Ok(new UserDto(guest.Name, guest.Email, guest.Role, new List<string>()));
        }

        [Authorize]
        [HttpGet("session")]
        public IActionResult GetSession()
        {
            var email = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer";

            if (string.IsNullOrEmpty(email)) return Unauthorized();

            if (role.Equals("Guest", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new UserDto("Guest Shopper", email, "Guest", new List<string>()));
            }

            var dbUser = DatabaseConnector.GetInstance().GetUserByEmail(email);
            if (dbUser == null) return Unauthorized();

            return Ok(new UserDto(dbUser.Name, dbUser.Email, dbUser.Role, GetCartIdsForUser(dbUser)));
        }

        private List<string> GetCartIdsForUser(User? user)
        {
            if (user is Customer customer)
            {
                return customer.Cart.GetAllItems()
                    .SelectMany(item => Enumerable.Repeat(item.Id, item.Quantity))
                    .ToList();
            }
            return new List<string>();
        }

        [Authorize]
        [HttpPost("cart")]
        public IActionResult SaveCart([FromBody] List<string> bookIds)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = DatabaseConnector.GetInstance().GetUserByEmail(email);
            if (user == null) return Unauthorized();

            if (user is Customer customer)
            {
                customer.Cart.Clear();

                List<Book> systemBooks = _bookstoreSystem.GetBooks();

                foreach (var id in bookIds)
                {
                    var book = systemBooks.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                    if (book != null)
                    {
                        customer.Cart.AddItem(new Book
                        {
                            Id = book.Id,
                            Name = book.Name,
                            Price = book.Price,
                            Stock = book.Stock,
                            ISBN = book.ISBN,
                            Author = book.Author,
                            Publisher = book.Publisher,
                            Genre = book.Genre,
                            PublicationYear = book.PublicationYear,
                            Status = book.Status,
                            Quantity = 1
                        });
                    }
                }

                DatabaseConnector.GetInstance().SaveUser(customer);

                // Sync the modified cart to the AccountManager singleton session user to prevent logout overwriting
                var sessionUser = _bookstoreSystem.AccountManager.CurrentSessionUser;
                if (sessionUser != null && sessionUser.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase) && sessionUser is Customer sessionCustomer)
                {
                    sessionCustomer.Cart.Clear();
                    foreach (var item in customer.Cart.GetAllItems())
                    {
                        sessionCustomer.Cart.AddItem(item);
                    }
                }

                return Ok(new { message = "Cart synchronized successfully." });
            }

            return BadRequest(new { message = "Only registered customers can persist carts." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _bookstoreSystem.AccountManager.Logout();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("checkout")]
        public IActionResult Checkout(CheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { message = "Cart is empty." });
            }

            if (request.Address == null || !new Address { Street = request.Address.Street, Suburb = request.Address.Suburb, State = request.Address.State, Postcode = request.Address.Postcode }.IsValid())
            {
                return BadRequest(new { message = "Shipping address is required before checkout." });
            }

            if (request.Payment == null || string.IsNullOrWhiteSpace(request.Payment.Method))
            {
                return BadRequest(new { message = "Payment method is required before checkout." });
            }

            // If user is a Guest (unauthenticated or in Guest role), require them to provide Name and Email in the form
            bool isGuest = User.Identity?.IsAuthenticated != true || User.IsInRole("Guest");
            if (isGuest)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Guest name is required before checkout." });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Guest email is required before checkout." });
                }
            }

            ShoppingCart cart = new ShoppingCart();
            List<Book> systemBooks = _bookstoreSystem.GetBooks();

            foreach (var item in request.Items)
            {
                Book? book = systemBooks.FirstOrDefault(b => b.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
                if (book == null)
                {
                    return NotFound(new { message = $"Book {item.Id} was not found." });
                }
                if (item.Quantity <= 0)
                {
                    return BadRequest(new { message = "Item quantity must be greater than zero." });
                }
                if (book.Stock < item.Quantity)
                {
                    return BadRequest(new { message = $"Not enough stock for {book.Name}. Only {book.Stock} left." });
                }

                cart.AddItem(new Book { Id = book.Id, Name = book.Name, Price = book.Price, Quantity = item.Quantity });
            }

            Order? order = cart.CreateOrder(cart.GetAllItems());
            if (order == null)
            {
                return BadRequest(new { message = "Failed to create order." });
            }

            order.ShippingAddress = new Address { Street = request.Address.Street, Suburb = request.Address.Suburb, State = request.Address.State, Postcode = request.Address.Postcode };

            PaymentMethod paymentMethod;
            string payerEmail = (isGuest ? request.Email : (User.Identity?.Name ?? request.Email)) ?? "";
            string payerName = (isGuest ? request.Name : (User.Identity?.Name ?? request.Name)) ?? "Guest Shopper";

            if (request.Payment.Method.Contains("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = new PayPal { AccountEmail = payerEmail };
            }
            else
            {
                paymentMethod = new Card { CardNumber = "4111111111111111", CVV = "123", CardholderName = payerName }; // Mock card for demo
            }

            Invoice? invoiceResult;
            try
            {
                invoiceResult = order.ProcessCheckout(paymentMethod);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (invoiceResult == null)
            {
                return BadRequest(new { message = "Payment processing failed." });
            }

            // Deduct stock in system and save
            foreach (var orderedItem in order.Items)
            {
                Book systemBook = systemBooks.First(b => b.Id == orderedItem.Id);
                systemBook.ReduceStock(orderedItem.Quantity);
            }
            
            _bookstoreSystem.SaveBooks();

            var finalBooksDto = _bookstoreSystem.GetCatalogueBooks().Select(book => new BookDto
            {
                Id = book.Id, ISBN = book.ISBN, Price = book.Price, Stock = book.Stock,
                Title = book.Name, Author = book.Author, Genre = book.Genre, Publisher = book.Publisher
            }).ToList();

            var invoiceDto = new CheckoutInvoice(
                invoiceResult.InvoiceId,
                invoiceResult.DateIssued,
                payerName,
                payerEmail,
                request.Address,
                request.Payment,
                order.Items.Select(i => new InvoiceLine(i.Id, i.Name, i.Quantity, i.Price, i.Price * i.Quantity)).ToList(),
                invoiceResult.TotalAmount
            );

            return Ok(new CheckoutResponse("Order confirmed.", invoiceResult.TotalAmount, finalBooksDto, invoiceDto));
        }
    }

    public class BookDto
    {
        public string Id { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
    }

    public record SignupRequest(string Name, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record UserDto(string Name, string Email, string Role, List<string>? Cart = null);
    public record CheckoutItem(string Id, int Quantity);
    public record CheckoutAddress(string Street, string Suburb, string State, string Postcode);
    public record CheckoutPayment(string Method);
    public record CheckoutRequest(string? Name, string? Email, List<CheckoutItem> Items, CheckoutAddress? Address, CheckoutPayment? Payment);
    public record InvoiceLine(string Id, string Title, int Quantity, decimal UnitPrice, decimal LineTotal);
    public record CheckoutInvoice(string InvoiceNumber, DateTime IssuedAt, string? CustomerName, string? Email, CheckoutAddress Address, CheckoutPayment Payment, List<InvoiceLine> Items, decimal Total);
    public record CheckoutResponse(string Message, decimal Total, List<BookDto> Books, CheckoutInvoice? Invoice);
}
