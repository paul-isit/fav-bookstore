using System;
using System.Text.Json.Serialization; // Required for polymorphic JSON mapping

namespace FavouriteBookstore.Models
{
    // Tell .NET 9 how to map abstract type profiles cleanly within text arrays
    [JsonDerivedType(typeof(Customer), typeDiscriminator: "Customer")]
    [JsonDerivedType(typeof(Admin), typeDiscriminator: "Admin")]
    [JsonDerivedType(typeof(Guest), typeDiscriminator: "Guest")]
    
    public abstract class User
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 

        protected User() { }

        // The Factory Method pattern to dynamically instantiate profiles
        public static User? CreateUser(string email, string password, string name, string roleContext)
        {
            string hashedPassword = string.IsNullOrWhiteSpace(password) ? "" : BCrypt.Net.BCrypt.HashPassword(password);
            
            return roleContext.ToLower() switch
            {
                "admin" => new Admin(email, hashedPassword, name),
                "customer" => new Customer(email, hashedPassword, name),
                "guest" => new Guest(email),
                _ => null
            };
        }

        public bool VerifyPassword(string inputPassword)
        {
            if (PasswordHash.StartsWith("$2"))
            {
                try {
                    return BCrypt.Net.BCrypt.Verify(inputPassword, PasswordHash);
                } catch {
                    return false;
                }
            }
            return PasswordHash == inputPassword; // Fallback for old plain-text data
        }
    }
}
