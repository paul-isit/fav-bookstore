using System;

namespace FavouriteBookstore.Models
{
    public class Admin : User
    {
        public Admin() : base()
        {
            Role = "Admin";
        }

        public Admin(string email, string password, string name)
        {
            Email = email;
            PasswordHash = password;
            Name = name;
            Role = "Admin";
        }
    }
}