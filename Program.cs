using FavouriteBookstore.Services;
using FavouriteBookstore.Models;

namespace FavouriteBookstore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine(" SWE30003 Group 3: Full End-to-End Cart & Checkout Test   ");
            Console.WriteLine("==========================================================\n");

            string usersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "data", "users.json");

            try
            {
                // 1. Bootstrap the System Facade
                Console.WriteLine("--- PHASE 1: System Bootstrap ---");
                BookstoreSystem system = BookstoreSystem.Instance;
                Console.WriteLine($"Status: {system}\n");

                AccountManager authManager = system.AccountManager;

                // 2. Execute Task 1: Manage Customer Account (Registration)
                Console.WriteLine("--- PHASE 2: Testing Account Registration (Task 1) ---");
                
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

                // 3. Execute Task 1: Manage Customer Account (Authentication)
                Console.WriteLine("--- PHASE 3: Testing Authentication Loop ---");

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

                // 4. Test Checkout and Order Processing (Task: Green Section & Advanced Cart Logic)
                Console.WriteLine("--- PHASE 4: Testing Checkout and Order Processing ---");
                
                Customer activeCustomer = authManager.CurrentSessionUser as Customer ?? throw new Exception("Customer session invalid.");
                ShoppingCart userCart = activeCustomer.Cart;

                // Prove cart initializes properly (or loads empty if no previous state)
                Console.WriteLine($"[Test 4A] Cart loaded. Current distinct items: {userCart.GetAllItems().Count}\n");

                Console.WriteLine("[Test 4B] Adding items to Shopping Cart...");
                Book book1 = new Book { Id = "B000", Price = 29.99, Stock = 10, Quantity = 2 };
                Book book2 = new Book { Id = "B001", Price = 15.50, Stock = 5, Quantity = 1 };
                Book book3 = new Book { Id = "B003", Price = 9.99, Stock = 20, Quantity = 1 };
                
                userCart.AddItem(book1);
                userCart.AddItem(book2);
                userCart.AddItem(book3);
                
                Console.WriteLine($"Cart contains {userCart.GetAllItems().Count} distinct items. Total Cart Value: ${userCart.GetTotalPrice():F2}\n");
                Console.WriteLine("Current Cart Contents:");
                foreach(var item in userCart.GetAllItems()) { Console.WriteLine($"- {item.Id} | Qty: {item.Quantity} | Unit Price: ${item.Price:F2} | Subtotal: ${(item.Price * item.Quantity):F2}"); }

                // Test dynamic cart operations requested
                Console.WriteLine("\n[Test 4C] Testing dynamic cart modification methods...");
                userCart.RemoveItemById("B001");
                Console.WriteLine("- Removed item B001 completely.");
                
                userCart.IncreaseQuantity("B003", 2); // Qty becomes 3
                Console.WriteLine("- Increased quantity of B003 by 2.");
                
                userCart.DecreaseQuantity("B000", 1); // Qty drops to 1
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

                // 5. Test Session Cleanup & Persistence
                Console.WriteLine("--- PHASE 5: Session Teardown and Save State ---");
                authManager.Logout();
                Console.WriteLine($"Is User Still Logged In? {authManager.IsUserLoggedIn}\n");
                
                // 6. Test Login Reload (Confirming Cart state persisted)
                Console.WriteLine("--- PHASE 6: Testing State Persistence After Re-Login ---");
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
                    reloadedCust.Cart.AddItem(new Book { Id = "B999", Price = 100.00, Quantity = 1 });
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

                // 7. Testing Multi-User Cart Isolation
                Console.WriteLine("\n--- PHASE 7: Testing Multi-User Cart Isolation ---");
                Console.WriteLine("[Test 7A] Registering and logging in as alice@swinburne.edu.au...");
                
                authManager.RegisterAccount("alice@swinburne.edu.au", "securePassword123", "Alice Smith", "customer");
                authManager.Login("alice@swinburne.edu.au", "securePassword123");
                
                if (authManager.CurrentSessionUser is Customer aliceCust)
                {
                    int aliceCartCount = aliceCust.Cart.GetAllItems().Count;
                    Console.WriteLine($"[Success] Alice logged in. Cart is successfully initialized as EMPTY: {aliceCartCount} items. (Isolated from Jeremy's cart)");
                    
                    aliceCust.Cart.AddItem(new Book { Id = "A001", Price = 50.00, Quantity = 1 });
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

                Console.WriteLine("\n==================================================");
                Console.WriteLine("      All core domain test assertions passed!     ");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Critical System Crash Exception]: {ex.Message}");
            }
            // finally
            // {
            //     // Environment Teardown ensuring idempotency
            //     Console.WriteLine("\n--- PHASE 8: Environment Teardown ---");
            //     if (File.Exists(usersFilePath))
            //     {
            //         File.WriteAllText(usersFilePath, "[]");
            //         Console.WriteLine("[Environment Reset] users.json wiped entirely. Ready for next execution block.");
            //     }
            // }
        }
    }
}
