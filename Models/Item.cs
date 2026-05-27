using System.Text.Json.Serialization;

namespace FavouriteBookstore.Models
{
    // Make sure JSON mapping supports Book so items in the cart are deserialized properly
    [JsonDerivedType(typeof(Book), typeDiscriminator: "Book")]
    public abstract class Item
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Stock { get; set; }
        
        // Quantity for cart management
        public int Quantity { get; set; } = 1;

        public bool IsAvailable => Stock > 0;

        protected Item() { }

        public bool HasEnoughStock(int quantity)
        {
            return quantity > 0 && Stock >= quantity;
        }

        public void ReduceStock(int quantity)
        {
            if (!HasEnoughStock(quantity))
            {
                throw new InvalidOperationException("Not enough stock available.");
            }

            Stock -= quantity;
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            Stock += quantity;
        }
    }
}