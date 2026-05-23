using System;

namespace FavouriteBookstore.Models
{
    public class Guest : User
    {
        // Guests utilize a temporary, single-session cart
        public ShoppingCart Cart { get; private set; }

        public Guest() : base()
        {
            Email = $"Guest_{Guid.NewGuid().ToString().Substring(0, 8)}@favouritebooks.com";
            Name = "Guest Shopper";
            Role = "Guest";
            Cart = new ShoppingCart();
        }

        public Guest(string temporaryEmail)
        {
            Email = temporaryEmail;
            Name = "Guest Shopper";
            Role = "Guest";
            Cart = new ShoppingCart();
        }
    }
}
