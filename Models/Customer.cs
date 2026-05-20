using System;

namespace FavouriteBookstore.Models
{
    public class Customer : User
    {
        // Registered shoppers maintain a dedicated, persistent cart link
        public ShoppingCart Cart { get; private set; }

        public Customer() : base() 
        {
            Role = "Customer";
            Cart = new ShoppingCart();
        }

        public Customer(string email, string password, string name)
        {
            Email = email;
            PasswordHash = password;
            Name = name;
            Role = "Customer";
            Cart = new ShoppingCart();
        }
    }
}
