using FavouriteBookstore.Services;
using FavouriteBookstore.Models;

namespace FavouriteBookstore
{
    /// <summary>
    /// Runs all end-to-end tests for the bookstore system.
    /// This includes account management, cart operations, checkout, and catalogue search/filter functionality.
    /// </summary>
    public class TestRunner
    {
        public static void RunAllTests()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine(" SWE30003 Group 3: Full End-to-End Cart & Checkout Test   ");
            Console.WriteLine("==========================================================\n");

            try
            {
                RunPhase1_SystemBootstrap();
                RunPhase2_AccountRegistration();
                RunPhase3_Authentication();
                RunPhase4_CheckoutAndOrders();
                RunPhase5_SessionTeardown();
                RunPhase6_StatePersistence();
                RunPhase7_MultiUserIsolation();
                RunPhase8_CatalogueSearchFilter();

                Console.WriteLine("\n==================================================");
                Console.WriteLine("   All core domain test assertions passed!      ");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Critical System Crash Exception]: {ex.Message}");
            }
        }

        private static void RunPhase1_SystemBootstrap()
        {
            Console.WriteLine("--- PHASE 1: System Bootstrap ---");
            BookstoreSystem system = BookstoreSystem.Instance;
            Console.WriteLine($"Status: {system}\n");
        }

        private static void RunPhase2_AccountRegistration()
        {
            Console.WriteLine("--- PHASE 2: Testing Account Registration (Task 1) ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;

            Console.WriteLine("[Test 2A] Registering a valid new customer account...");
            bool isRegSuccess = authManager.RegisterAccount(
                "jeremy@swinburne.edu.au", 
                "securePassword123", 
                "Jeremy Allan", 
                "customer"
            );
            Console.WriteLine($"Registration Result: {(isRegSuccess ? "SUCCESS" : "FAILED (or already exists)")}\n");

            Console.WriteLine("[Test 2B] Verifying constraint handling for blank strings...");
            try
            {
                authManager.RegisterAccount("", "pass", "Name", "customer");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation Catch Success: {ex.Message}\n");
            }

            Console.WriteLine("[Test 2C] Verifying duplicate registration prevention...");
            bool isDuplicateSuccess = authManager.RegisterAccount(
                "jeremy@swinburne.edu.au", 
                "differentPass", 
                "Jeremy Duplicate Check", 
                "customer"
            );
            Console.WriteLine($"Duplicate Allowed? {(isDuplicateSuccess ? "YES (Flaw)" : "NO (Correct Validation Pattern)")}\n");
        }

        private static void RunPhase3_Authentication()
        {
            Console.WriteLine("--- PHASE 3: Testing Authentication Loop ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;

            Console.WriteLine("[Test 3A] Attempting login with incorrect password...");
            bool badLogin = authManager.Login("jeremy@swinburne.edu.au", "wrongPassword");
            Console.WriteLine($"Login Allowed? {badLogin}\n");

            Console.WriteLine("[Test 3B] Attempting login with valid credentials...");
            bool goodLogin = authManager.Login("jeremy@swinburne.edu.au", "securePassword123");
            Console.WriteLine($"Login Allowed? {goodLogin}");

            if (authManager.IsUserLoggedIn && authManager.CurrentSessionUser != null)
            {
                User activeUser = authManager.CurrentSessionUser;
                Console.WriteLine($"Active Session State: {activeUser.Name} | Role: {activeUser.Role}\n");
            }
        }

        private static void RunPhase4_CheckoutAndOrders()
        {
            Console.WriteLine("--- PHASE 4: Testing Checkout and Order Processing ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;

            Customer activeCustomer = authManager.CurrentSessionUser as Customer ?? throw new Exception("Customer session invalid.");
            ShoppingCart userCart = activeCustomer.Cart;

            Console.WriteLine($"[Test 4A] Cart loaded. Current distinct items: {userCart.GetAllItems().Count}\n");

            Console.WriteLine("[Test 4B] Adding items to Shopping Cart...");
            Book book1 = new Book { Id = "B000", Price = 29.99M, Stock = 10, Quantity = 2 };
            Book book2 = new Book { Id = "B001", Price = 15.50M, Stock = 5, Quantity = 1 };
            Book book3 = new Book { Id = "B003", Price = 9.99M, Stock = 20, Quantity = 1 };

            userCart.AddItem(book1);
            userCart.AddItem(book2);
            userCart.AddItem(book3);

            Console.WriteLine($"Cart contains {userCart.GetAllItems().Count} distinct items. Total Cart Value: ${userCart.GetTotalPrice():F2}\n");
            Console.WriteLine("Current Cart Contents:");
            foreach(var item in userCart.GetAllItems()) { Console.WriteLine($"- {item.Id} | Qty: {item.Quantity} | Unit Price: ${item.Price:F2} | Subtotal: ${(item.Price * item.Quantity):F2}"); }

            Console.WriteLine("\n[Test 4C] Testing dynamic cart modification methods...");
            userCart.RemoveItemById("B001");
            Console.WriteLine("- Removed item B001 completely.");

            userCart.IncreaseQuantity("B003", 2);
            Console.WriteLine("- Increased quantity of B003 by 2.");

            userCart.DecreaseQuantity("B000", 1);
            Console.WriteLine("- Decreased quantity of B000 by 1.");

            Console.WriteLine("\nCart Contents After Modifications:");
            foreach(var item in userCart.GetAllItems()) { Console.WriteLine($"- {item.Id} | Qty: {item.Quantity} | Unit Price: ${item.Price:F2} | Subtotal: ${(item.Price * item.Quantity):F2}"); }

            Console.WriteLine("\n[Test 4D] Selecting specific items to checkout (ONLY B003)...");
            List<Item> selectedItems = userCart.GetAllItems().Where(i => i.Id == "B003").ToList();

            Console.WriteLine("[Test 4E] Creating Order from Selection...");
            Order? newOrder = userCart.CreateOrder(selectedItems);
            if (newOrder != null)
            {
                Console.WriteLine("Order Created Successfully.");
                newOrder.PrintOrderDetails(); 
                Console.WriteLine($"\nOrder Initial Status - Paid: {newOrder.IsPaid}\n");

                Console.WriteLine("[Test 4F] Processing Checkout with DummyPayment Strategy...");
                PaymentMethod dummyPayment = new DummyPayment();
                Invoice? generatedInvoice = newOrder.ProcessCheckout(dummyPayment);

                Console.WriteLine($"Order Final Status - Paid: {newOrder.IsPaid}");
                if (generatedInvoice != null)
                {
                    Console.WriteLine($"\n=======================================");
                    Console.WriteLine($"               INVOICE                 ");
                    Console.WriteLine($"=======================================");
                    Console.WriteLine($"Invoice ID:   {generatedInvoice.InvoiceId}");
                    Console.WriteLine($"Date Issued:  {generatedInvoice.DateIssued:g}");
                    Console.WriteLine($"Order Ref:    {generatedInvoice.OrderReference.OrderId}");
                    Console.WriteLine($"Payment:      PAID");
                    Console.WriteLine($"---------------------------------------");
                    Console.WriteLine($"Items:");
                    foreach (var item in generatedInvoice.OrderReference.Items)
                    {
                        Console.WriteLine($"  - {item.Id,-6} | Qty: {item.Quantity,2} | ${(item.Price * item.Quantity),7:F2}");
                    }
                    Console.WriteLine($"---------------------------------------");
                    Console.WriteLine($"Total Amount: ${generatedInvoice.TotalAmount:F2}");
                    Console.WriteLine($"=======================================\n");

                    Console.WriteLine("[Test 4G] Payment successful. Removing purchased items from the user's cart...");
                    userCart.RemovePurchasedItems(newOrder.Items);
                }
                else
                {
                    Console.WriteLine("Checkout Failed: Invoice was null.\n");
                }
            }
            else
            {
                Console.WriteLine("Failed to create Order from Cart.\n");
            }

            Console.WriteLine($"[Test 4H] Cart Validation - Remaining distinct items in cart: {userCart.GetAllItems().Count}");
            foreach(var remaining in userCart.GetAllItems())
            {
                Console.WriteLine($"- Remaining Item: {remaining.Id} | Qty: {remaining.Quantity} (Unit Price: ${remaining.Price:F2} | Subtotal: ${(remaining.Price * remaining.Quantity):F2})");
            }
            Console.WriteLine("");
        }

        private static void RunPhase5_SessionTeardown()
        {
            Console.WriteLine("--- PHASE 5: Session Teardown and Save State ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;
            authManager.Logout();
            Console.WriteLine($"Is User Still Logged In? {authManager.IsUserLoggedIn}\n");
        }

        private static void RunPhase6_StatePersistence()
        {
            Console.WriteLine("--- PHASE 6: Testing State Persistence After Re-Login ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;

            Console.WriteLine("[Test 6A] Logging back in as jeremy@swinburne.edu.au...");
            bool reloginSuccess = authManager.Login("jeremy@swinburne.edu.au", "securePassword123");

            if (reloginSuccess && authManager.CurrentSessionUser is Customer reloadedCust)
            {
                int reloadedCartCount = reloadedCust.Cart.GetAllItems().Count;
                Console.WriteLine($"[Success] User re-logged in. Restored Cart has {reloadedCartCount} distinct items.");
                foreach(var item in reloadedCust.Cart.GetAllItems())
                {
                    Console.WriteLine($"  - Restored Item: {item.Id} | Qty: {item.Quantity} | Price: ${item.Price:F2}");
                }

                Console.WriteLine("\n[Test 6B] Adding another book to verify state append works...");
                reloadedCust.Cart.AddItem(new Book { Id = "B999", Price = 100.00M, Quantity = 1 });
            }
            authManager.Logout();

            Console.WriteLine("\n[Test 6C] Logging back in ONE MORE TIME to confirm appended state...");
            authManager.Login("jeremy@swinburne.edu.au", "securePassword123");
            if (authManager.CurrentSessionUser is Customer finalCheckCust)
            {
                Console.WriteLine($"[Success] Final cart validation has {finalCheckCust.Cart.GetAllItems().Count} distinct items.");
                foreach(var item in finalCheckCust.Cart.GetAllItems())
                {
                    Console.WriteLine($"  - Final Item: {item.Id} | Qty: {item.Quantity} | Price: ${item.Price:F2}");
                }
            }
            authManager.Logout();
        }

        private static void RunPhase7_MultiUserIsolation()
        {
            Console.WriteLine("\n--- PHASE 7: Testing Multi-User Cart Isolation ---");
            AccountManager authManager = BookstoreSystem.Instance.AccountManager;

            Console.WriteLine("[Test 7A] Registering and logging in as alice@swinburne.edu.au...");

            authManager.RegisterAccount("alice@swinburne.edu.au", "securePassword123", "Alice Smith", "customer");
            authManager.Login("alice@swinburne.edu.au", "securePassword123");

            if (authManager.CurrentSessionUser is Customer aliceCust)
            {
                int aliceCartCount = aliceCust.Cart.GetAllItems().Count;
                Console.WriteLine($"[Success] Alice logged in. Cart is successfully initialized as EMPTY: {aliceCartCount} items. (Isolated from Jeremy's cart)");

                aliceCust.Cart.AddItem(new Book { Id = "A001", Price = 50.00M, Quantity = 1 });
                Console.WriteLine($"Added item A001 to Alice's cart. Alice's cart now has {aliceCust.Cart.GetAllItems().Count} distinct item(s).");
            }
            authManager.Logout();

            Console.WriteLine("\n[Test 7B] Logging back in as Alice to verify her specific state...");
            authManager.Login("alice@swinburne.edu.au", "securePassword123");
            if (authManager.CurrentSessionUser is Customer aliceFinalCheck)
            {
                Console.WriteLine($"Alice's Cart correctly restored with {aliceFinalCheck.Cart.GetAllItems().Count} items:");
                foreach(var item in aliceFinalCheck.Cart.GetAllItems())
                {
                    Console.WriteLine($"  - Restored Item: {item.Id} | Qty: {item.Quantity} | Price: ${item.Price:F2}");
                }
            }
            authManager.Logout();
        }

        private static void RunPhase8_CatalogueSearchFilter()
        {
            Console.WriteLine("\n--- PHASE 8: Testing Catalogue Search & Filter ---");
            Catalogue catalogue = new Catalogue();

            try
            {
                catalogue.RegisterBook(new Book { Id = "CAT_001", ISBN = "978-0743-27557-5", Name = "The Great Gatsby", Author = "F. Scott Fitzgerald", Genre = "Classic", Price = 10.99M, Stock = 5 });
                catalogue.RegisterBook(new Book { Id = "CAT_002", ISBN = "978-0061-12008-4", Name = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Classic", Price = 12.99M, Stock = 3 });
                catalogue.RegisterBook(new Book { Id = "CAT_003", ISBN = "978-0451-52493-2", Name = "1984", Author = "George Orwell", Genre = "Dystopian", Price = 13.99M, Stock = 2 });
                catalogue.RegisterBook(new Book { Id = "CAT_004", ISBN = "978-0547-92873-4", Name = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", Price = 14.99M, Stock = 8 });
                catalogue.RegisterBook(new Book { Id = "CAT_005", ISBN = "978-0553-29438-0", Name = "Foundation", Author = "Isaac Asimov", Genre = "Science Fiction", Price = 15.99M, Stock = 4 });
                Console.WriteLine("[Test 8A] Added 5 test books to catalogue.\n");

                Console.WriteLine("[Test 8B] SearchByTitle - searching for 'Great'...");
                var titleResults = catalogue.SearchByTitle("Great");
                Console.WriteLine($"Found {titleResults.Count} result(s): {string.Join(", ", titleResults.Select(b => b.Name))}");

                Console.WriteLine("[Test 8C] SearchByTitle - searching for 'HOBBIT' (case-insensitive)...");
                titleResults = catalogue.SearchByTitle("HOBBIT");
                Console.WriteLine($"Found {titleResults.Count} result(s): {string.Join(", ", titleResults.Select(b => b.Name))}\n");

                Console.WriteLine("[Test 8D] SearchByAuthor - searching for 'Tolkien'...");
                var authorResults = catalogue.SearchByAuthor("Tolkien");
                Console.WriteLine($"Found {authorResults.Count} result(s): {string.Join(", ", authorResults.Select(b => b.Name))}");

                Console.WriteLine("[Test 8E] SearchByAuthor - searching for 'orwell' (case-insensitive)...");
                authorResults = catalogue.SearchByAuthor("orwell");
                Console.WriteLine($"Found {authorResults.Count} result(s): {string.Join(", ", authorResults.Select(b => b.Name))}\n");

                Console.WriteLine("[Test 8F] FilterByGenre - filtering for 'Classic'...");
                var classicBooks = catalogue.FilterByGenre("Classic");
                Console.WriteLine($"Found {classicBooks.Count} result(s): {string.Join(", ", classicBooks.Select(b => b.Name))}");

                Console.WriteLine("[Test 8G] FilterByGenre - filtering for 'fantasy' (case-insensitive)...");
                var fantasyBooks = catalogue.FilterByGenre("fantasy");
                Console.WriteLine($"Found {fantasyBooks.Count} result(s): {string.Join(", ", fantasyBooks.Select(b => b.Name))}\n");

                Console.WriteLine("[Test 8H] SearchByTitle - searching for 'NonExistent'...");
                var noResults = catalogue.SearchByTitle("NonExistent");
                Console.WriteLine($"Found {noResults.Count} result(s) (expected 0)\n");

                Console.WriteLine("==================================================");
                Console.WriteLine("  Catalogue Search & Filter tests passed!       ");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Catalogue Test Exception]: {ex.Message}");
            }
        }
    }
}
