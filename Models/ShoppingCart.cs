namespace FavouriteBookstore.Models
{
    public class ShoppingCart
    {
        private List<Item> cartItems = new List<Item>();

        public void AddItem(Item item)
        {
            cartItems.Add(item);
        }
    }
}
