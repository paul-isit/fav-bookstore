using System.Text.Json;
using FavouriteBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using FavouriteBookstore.Services;

namespace FavouriteBookstore.Controllers
{
    [ApiController]
    [Route("api")]
    public class StoreApiController : ControllerBase
    {
        private readonly string _booksPath;
        private readonly string _usersPath;
        private static readonly object FileLock = new object();
        private readonly BookstoreSystem _bookstoreSystem;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public StoreApiController(IWebHostEnvironment environment, BookstoreSystem bookstoreSystem)
        {
            string dataPath = Path.Combine(environment.ContentRootPath, "Infrastructure", "data");
            _booksPath = Path.Combine(dataPath, "books.json");
            _usersPath = Path.Combine(dataPath, "users.json");

            // Use the main bookstore system for catalogue logic.
            _bookstoreSystem = bookstoreSystem;
        }

        [HttpGet("books")]
        public ActionResult<List<BookDto>> GetBooks()
        {
            // Only return books that were registered into the Catalogue.
            // This hides reserve books from the website.
            List<BookDto> catalogueBooks = _bookstoreSystem
                .GetCatalogueBooks()
                .Select(book => new BookDto
                {
                    Id = book.Id,
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
        public IActionResult Signup(SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Name, email and password are required." });
            }

            lock (FileLock)
            {
                List<WebsiteUserRecord> users = ReadUsersUnsafe();
                if (users.Any(user => user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict(new { message = "An account already exists for this email address." });
                }

                if (users.Any(user => user.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict(new { message = "That username is already taken." });
                }

                WebsiteUserRecord newUser = new WebsiteUserRecord(request.Name.Trim(), request.Email.Trim().ToLowerInvariant(), request.Password, "Customer");
                users.Add(newUser);
                WriteUsersUnsafe(users);

                return Ok(new UserDto(newUser.Name, newUser.Email, newUser.Role));
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            lock (FileLock)
            {
                WebsiteUserRecord? user = ReadUsersUnsafe().FirstOrDefault(existing => existing.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
                if (user == null || user.PasswordHash != request.Password)
                {
                    return Unauthorized(new { message = "Email or password is incorrect." });
                }

                return Ok(new UserDto(user.Name, user.Email, user.Role));
            }
        }

        [HttpPost("guest")]
        public IActionResult GuestLogin()
        {
            Guest guest = new Guest();
            return Ok(new UserDto(guest.Name, guest.Email, guest.Role));
        }

        [HttpPost("checkout")]
        public IActionResult Checkout(CheckoutRequest request)
        {
            if (request.Items.Count == 0)
            {
                return BadRequest(new { message = "Cart is empty." });
            }

            if (request.Address == null ||
                string.IsNullOrWhiteSpace(request.Address.Street) ||
                string.IsNullOrWhiteSpace(request.Address.Suburb) ||
                string.IsNullOrWhiteSpace(request.Address.State) ||
                string.IsNullOrWhiteSpace(request.Address.Postcode))
            {
                return BadRequest(new { message = "Shipping address is required before checkout." });
            }

            if (request.Payment == null || string.IsNullOrWhiteSpace(request.Payment.Method))
            {
                return BadRequest(new { message = "Payment method is required before checkout." });
            }

            lock (FileLock)
            {
                List<BookDto> books = ReadBooksUnsafe();

                foreach (CheckoutItem item in request.Items)
                {
                    BookDto? book = books.FirstOrDefault(existing => existing.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
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
                        return BadRequest(new { message = $"Not enough stock for {book.Title}. Only {book.Stock} left." });
                    }
                }

                decimal total = request.Items.Sum(item =>
                {
                    BookDto book = books.First(existing => existing.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
                    return book.Price * item.Quantity;
                });

                foreach (CheckoutItem item in request.Items)
                {
                    BookDto book = books.First(existing => existing.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
                    book.Stock -= item.Quantity;
                }

                WriteBooksUnsafe(books);

                CheckoutInvoice invoice = new CheckoutInvoice(
                    $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    DateTime.UtcNow,
                    request.Email,
                    request.Address,
                    request.Payment,
                    request.Items.Select(item =>
                    {
                        BookDto book = books.First(existing => existing.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
                        return new InvoiceLine(book.Id, book.Title, item.Quantity, book.Price, book.Price * item.Quantity);
                    }).ToList(),
                    total
                );

                return Ok(new CheckoutResponse("Order confirmed.", total, books, invoice));
            }
        }

        private List<BookDto> ReadBooks()
        {
            lock (FileLock)
            {
                return ReadBooksUnsafe();
            }
        }

        private List<BookDto> ReadBooksUnsafe()
        {
            if (!System.IO.File.Exists(_booksPath)) return new List<BookDto>();
            string json = System.IO.File.ReadAllText(_booksPath);
            return string.IsNullOrWhiteSpace(json)
                ? new List<BookDto>()
                : JsonSerializer.Deserialize<List<BookDto>>(json, JsonOptions) ?? new List<BookDto>();
        }

        private void WriteBooksUnsafe(List<BookDto> books)
        {
            System.IO.File.WriteAllText(_booksPath, JsonSerializer.Serialize(books, JsonOptions));
        }

        private List<WebsiteUserRecord> ReadUsersUnsafe()
        {
            if (!System.IO.File.Exists(_usersPath)) return new List<WebsiteUserRecord>();
            string json = System.IO.File.ReadAllText(_usersPath);
            return string.IsNullOrWhiteSpace(json)
                ? new List<WebsiteUserRecord>()
                : JsonSerializer.Deserialize<List<WebsiteUserRecord>>(json, JsonOptions) ?? new List<WebsiteUserRecord>();
        }

        private void WriteUsersUnsafe(List<WebsiteUserRecord> users)
        {
            System.IO.File.WriteAllText(_usersPath, JsonSerializer.Serialize(users, JsonOptions));
        }
    }

    public class BookDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
    }

    public record SignupRequest(string Name, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record WebsiteUserRecord(string Name, string Email, string PasswordHash, string Role);
    public record UserDto(string Name, string Email, string Role);
    public record CheckoutItem(string Id, int Quantity);
    public record CheckoutRequest(string? Email, List<CheckoutItem> Items);
    public record CheckoutResponse(string Message, decimal Total, List<BookDto> Books);
    //public record CheckoutAddress(string Street, string Suburb, string State, string Postcode);
    //public record CheckoutPayment(string Method);
    //public record CheckoutRequest(string? Email, List<CheckoutItem> Items, CheckoutAddress? Address, CheckoutPayment? Payment);
    //public record InvoiceLine(string Id, string Title, int Quantity, double UnitPrice, double LineTotal);
    //public record CheckoutInvoice(string InvoiceNumber, DateTime IssuedAt, string? Email, CheckoutAddress Address, CheckoutPayment Payment, List<InvoiceLine> Items, double Total);
    //public record CheckoutResponse(string Message, double Total, List<BookDto> Books, CheckoutInvoice Invoice);
}
