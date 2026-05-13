namespace FavouriteBookstore.Models
{
    public class Customer : User, IShopper
    {
        private ShoppingCart shoppingCart;

        public ShoppingCart GetCart()
        {
            return shoppingCart;
        }
    }
}
