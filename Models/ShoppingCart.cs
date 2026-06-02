using System.Text.Json.Serialization;

namespace FavouriteBookstore.Models
{
    public class ShoppingCart
    {
        [JsonInclude]
        private List<Item> cartItems = new List<Item>();

        public void AddItem(Item item)
        {
            // If the item already exists in the cart, simply increment the quantity.
            var existingItem = cartItems.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cartItems.Add(item);
            }
        }

        public void IncreaseQuantity(string itemId, int amount = 1)
        {
            var existingItem = cartItems.FirstOrDefault(i => i.Id == itemId);
            if (existingItem != null)
            {
                existingItem.Quantity += amount;
            }
            else
            {
                throw new InvalidOperationException($"Cannot increase quantity: Item with ID '{itemId}' does not exist in the cart.");
            }
        }

        public void DecreaseQuantity(string itemId, int amount = 1)
        {
            var existingItem = cartItems.FirstOrDefault(i => i.Id == itemId);
            if (existingItem != null)
            {
                existingItem.Quantity -= amount;
                if (existingItem.Quantity <= 0)
                {
                    cartItems.Remove(existingItem);
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot decrease quantity: Item with ID '{itemId}' does not exist in the cart.");
            }
        }

        public bool RemoveItemById(string itemId)
        {
            var existingItem = cartItems.FirstOrDefault(i => i.Id == itemId);
            if (existingItem != null)
            {
                return cartItems.Remove(existingItem);
            }
            return false;
        }

        public void Clear()
        {
            cartItems.Clear();
        }

        public bool RemoveItem(Item item)
        {
            var existingItem = cartItems.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                return cartItems.Remove(existingItem);
            }
            return false;
        }

        public IReadOnlyList<Item> GetAllItems()
        {
            return cartItems.AsReadOnly();
        }

        public decimal GetTotalPrice()
        {
            return cartItems.Sum(item => item.Price * item.Quantity);
        }

        public Order? CreateOrder(IEnumerable<Item> selectedItems)
        {
            var selectedIds = selectedItems.Select(i => i.Id).ToList();
            var itemsToOrder = cartItems.Where(i => selectedIds.Contains(i.Id)).ToList();
            
            if (itemsToOrder.Count == 0)
            {
                return null;
            }

            decimal orderTotal = itemsToOrder.Sum(item => item.Price * item.Quantity);
            return new Order(itemsToOrder, orderTotal);
        }

        public void RemovePurchasedItems(IEnumerable<Item> purchasedItems)
        {
            var purchasedIds = purchasedItems.Select(i => i.Id).ToList();
            cartItems.RemoveAll(i => purchasedIds.Contains(i.Id));
        }
    }
}
