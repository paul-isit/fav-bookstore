namespace FavouriteBookstore.Models
{
    public class Guest : User, IShopper
    {
        private ShoppingCart shoppingCart;
        public ShoppingCart GetCart()
        {
            return shoppingCart;
        }
    }
}
