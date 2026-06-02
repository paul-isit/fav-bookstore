namespace FavouriteBookstore.Models
{
    public class Order
    {
        public string OrderId { get; private set; }
        public List<Item> Items { get; private set; }
        public decimal TotalPrice { get; private set; }
        public bool IsPaid { get; private set; }
        public Address? ShippingAddress { get; set; } // Added ShippingAddress to satisfy Scenario 4 requirements

        public Order(List<Item> items, decimal totalPrice)
        {
            OrderId = Guid.NewGuid().ToString();
            Items = new List<Item>(items);
            TotalPrice = totalPrice;
            IsPaid = false;
        }

        public void PrintOrderDetails()
        {
            Console.WriteLine($"\n--- Order Details ({OrderId}) ---");
            
            // Iterate directly since items are now distinct and possess a Quantity property
            foreach (var item in Items)
            {
                Console.WriteLine($"Item: {item.Id} | Qty: {item.Quantity} | Unit Price: ${item.Price:F2} | Subtotal: ${(item.Price * item.Quantity):F2}");
            }
            if (ShippingAddress != null)
            {
                Console.WriteLine($"Shipping Address: {ShippingAddress}");
            }
            else
            {
                Console.WriteLine("Shipping Address: NOT PROVIDED");
            }
            Console.WriteLine($"Total Amount: ${TotalPrice:F2}");
            Console.WriteLine("----------------------------------");
        }

        public Invoice? ProcessCheckout(PaymentMethod paymentMethod)
        {
            if (paymentMethod == null) throw new ArgumentNullException(nameof(paymentMethod));

            // Validate that we have a shipping address before processing checkout
            if (ShippingAddress == null || !ShippingAddress.IsValid())
            {
                throw new InvalidOperationException("Checkout cannot proceed: A valid shipping address is required.");
            }

            bool paymentSuccess = paymentMethod.ProcessPayment(TotalPrice);

            if (paymentSuccess)
            {
                IsPaid = true;
                return new Invoice(this, TotalPrice);
            }

            return null;
        }
    }
}
