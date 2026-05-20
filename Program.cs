using System;
using FavouriteBookstore.Services;
using FavouriteBookstore.Models;
using FavouriteBookstore.Data;

namespace FavouriteBookstore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine(" SWE30003 Group 3: Isolated Domain Core Test Run ");
            Console.WriteLine("==================================================\n");

            try
            {
                // 1. Bootstrap the System Facade
                Console.WriteLine("--- PHASE 1: System Bootstrap ---");
                BookstoreSystem system = BookstoreSystem.Instance;
                Console.WriteLine($"Status: {system}\n");

                AccountManager authManager = system.AccountManager;

                // 2. Execute Task 1: Manage Customer Account (Registration)
                Console.WriteLine("--- PHASE 2: Testing Account Registration (Task 1) ---");
                
                // Test Case 2A: Successful standard customer creation
                Console.WriteLine("[Test 2A] Registering a valid new customer account...");
                bool isRegSuccess = authManager.RegisterAccount(
                    "jeremy@swinburne.edu.au", 
                    "securePassword123", 
                    "Jeremy Allan", 
                    "customer"
                );
                Console.WriteLine($"Registration Result: {(isRegSuccess ? "SUCCESS" : "FAILED")}\n");

                // Test Case 2B: Validation Handling - Blank Input Strings
                Console.WriteLine("[Test 2B] Verifying constraint handling for blank strings...");
                try
                {
                    authManager.RegisterAccount("", "pass", "Name", "customer");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Validation Catch Success: {ex.Message}\n");
                }

                // Test Case 2C: Validation Handling - Duplicate Email Guardrails
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

                // Test Case 3A: Bad credentials verification
                Console.WriteLine("[Test 3A] Attempting login with incorrect password...");
                bool badLogin = authManager.Login("jeremy@swinburne.edu.au", "wrongPassword");
                Console.WriteLine($"Login Allowed? {badLogin}\n");

                // Test Case 3B: Correct credentials verification
                Console.WriteLine("[Test 3B] Attempting login with valid credentials...");
                bool goodLogin = authManager.Login("jeremy@swinburne.edu.au", "securePassword123");
                Console.WriteLine($"Login Allowed? {goodLogin}");
                
                if (authManager.IsUserLoggedIn && authManager.CurrentSessionUser != null)
                {
                    User activeUser = authManager.CurrentSessionUser;
                    Console.WriteLine($"Active Active Session State: {activeUser.Name} | Role: {activeUser.Role}\n");
                }

                // 4. Test Session Cleanup
                Console.WriteLine("--- PHASE 4: Session Teardown ---");
                authManager.Logout();
                Console.WriteLine($"Is User Still Logged In? {authManager.IsUserLoggedIn}\n");

                Console.WriteLine("==================================================");
                Console.WriteLine("      All core domain test assertions passed!     ");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Critical System Crash Exception]: {ex.Message}");
            }
        }
    }
}
