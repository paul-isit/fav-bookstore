namespace FavouriteBookstore.Models
{
    public class Order
    {
        public string OrderId { get; private set; }
        public List<Item> Items { get; private set; }
        public double TotalPrice { get; private set; }
        
        public bool IsPaid { get; private set; }

        public Order(List<Item> items, double totalPrice)
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
            Console.WriteLine($"Total Amount: ${TotalPrice:F2}");
            Console.WriteLine("----------------------------------");
        }

        public Invoice? ProcessCheckout(PaymentMethod paymentMethod)
        {
            if (paymentMethod == null) throw new ArgumentNullException(nameof(paymentMethod));

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
