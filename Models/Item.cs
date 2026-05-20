namespace FavouriteBookstore.Models
{
    public abstract class Item
    {
        // Modern C# auto-properties eliminate unused private field warnings
        public string Id { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }

        protected Item() { }
    }
}
