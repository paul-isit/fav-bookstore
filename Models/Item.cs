using System.Text.Json.Serialization;

namespace FavouriteBookstore.Models
{
    // Make sure JSON mapping supports Book so items in the cart are deserialized properly
    [JsonDerivedType(typeof(Book), typeDiscriminator: "Book")]
    public abstract class Item
    {
        // Modern C# auto-properties eliminate unused private field warnings
        public string Id { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        
        // Quantity for cart management
        public int Quantity { get; set; } = 1;

        protected Item() { }
    }
}
