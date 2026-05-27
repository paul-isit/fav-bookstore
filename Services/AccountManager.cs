using FavouriteBookstore.Models;
using FavouriteBookstore.Data;

namespace FavouriteBookstore.Services
{
    public class AccountManager
    {
        private readonly DatabaseConnector _db;
        
        // Tracks the currently authenticated active session independently of HTTP context
        public User? CurrentSessionUser { get; private set; }
        public bool IsUserLoggedIn => CurrentSessionUser != null;

        public AccountManager(DatabaseConnector db)
        {
            _db = db;
            CurrentSessionUser = null; 
        }

        public bool RegisterAccount(string email, string password, string name, string roleContext)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Registration attributes cannot be blank to satisfy data consistency constraints.");
            }
            
            // Prevent duplicate registration explicitly here before trying to save
            if (_db.GetUserByEmail(email) != null)
            {
                return false;
            }

            // Utilize your Factory Method inside the User class to instantiate profiles dynamically
            // Note: Adjust parameters to match your exact User.CreateUser() method signature
            User? newUser = User.CreateUser(email, password, name, roleContext);
            
            if (newUser == null) return false;

            // Save the state inside your persistence layer
            return _db.SaveUser(newUser);
        }

        public bool Login(string email, string password)
        {
            // Query persistent storage to fetch user state by email identity
            User? user = _db.GetUserByEmail(email);

            if (user != null && user.VerifyPassword(password))
            {
                CurrentSessionUser = user;
                Console.WriteLine($"[Session] User {email} logged in successfully.");
                return true;
            }

            Console.WriteLine("[Session] Authentication failure: invalid credentials.");
            return false;
        }

        public void Logout()
        {
            if (CurrentSessionUser != null)
            {
                // Sync session user state back to DB right before logout
                _db.SaveUser(CurrentSessionUser);
                
                Console.WriteLine($"[Session] User {CurrentSessionUser.Email} logged out and state saved.");
                CurrentSessionUser = null;
            }
        }
    }
}