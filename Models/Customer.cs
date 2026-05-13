namespace FavouriteBookstore.Models
{
    public class Customer : IShopoper, User
    {
        private ShoppingCart shoppingCart;

        public ShoppingCart GetCart()
        {
            return shoppingCart;
        }
    }
}
