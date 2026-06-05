using System.Text.Json;
using FavouriteBookstore.Models;

namespace FavouriteBookstore.Data
{
    public class DatabaseConnector
    {
        private static DatabaseConnector? _instance;
        private static readonly object _lock = new object();
        
        // Path.Combine automatically maps appropriate cross-platform directory slashes
        private readonly string _usersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "data", "users.json");
        private readonly string _booksFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "data", "books.json");

        private DatabaseConnector()
        {
            InitializeStorageFiles();
        }

        public static DatabaseConnector GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnector();
                    }
                }
            }
            return _instance;
        }

        private void InitializeStorageFiles()
        {
            try
            {
                // Check if files do not exist OR are completely empty (0 bytes) 
                if (!File.Exists(_usersFilePath) || new FileInfo(_usersFilePath).Length == 0)
                {
                    File.WriteAllText(_usersFilePath, "[]");
                    Console.WriteLine("[Infrastructure] Initialized empty users array token.");
                }
                
                if (!File.Exists(_booksFilePath) || new FileInfo(_booksFilePath).Length == 0)
                {
                    File.WriteAllText(_booksFilePath, "[]");
                    Console.WriteLine("[Infrastructure] Initialized empty books array token.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error Handling] Storage file initialization failed: {ex.Message}");
            }
        }

        public User? GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            try
            {
                string jsonString = File.ReadAllText(_usersFilePath);
                var users = string.IsNullOrWhiteSpace(jsonString) 
                    ? new List<User>() 
                    : JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
                
                // Find and return the matching user entity
                return users.Find(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                // Graceful degradation / recovery mechanism as dictated by requirements
                return null;
            }
        }

        public bool SaveUser(User user)
        {
            if (user == null) return false;

            lock (_lock) // Ensure file access consistency across concurrent execution paths 
            {
                try
                {
                    string jsonString = File.ReadAllText(_usersFilePath);

                    var users = string.IsNullOrWhiteSpace(jsonString) 
                        ? new List<User>() 
                        : JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();

                    // Check if user already exists
                    int existingIndex = users.FindIndex(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
                    
                    if (existingIndex >= 0)
                    {
                        // Update existing user
                        users[existingIndex] = user;
                    }
                    else
                    {
                        // Add new user
                        users.Add(user);
                    }

                    string updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_usersFilePath, updatedJson);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error Handling] Failed to persist user: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Book> GetAllBooks()
        {
            try
            {
                string jsonString = File.ReadAllText(_booksFilePath);

                return string.IsNullOrWhiteSpace(jsonString)
                    ? new List<Book>()
                    : JsonSerializer.Deserialize<List<Book>>(
                        jsonString,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    ) ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error Handling] Failed to load books: {ex.Message}");
                return new List<Book>();
            }
        }
    }
}