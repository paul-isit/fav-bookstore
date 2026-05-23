namespace FavouriteBookstore.Models
{
    public class Invoice
    {
        public string InvoiceId { get; private set; }
        public DateTime DateIssued { get; private set; }
        public double TotalAmount { get; private set; }
        public Order OrderReference { get; private set; }

        public Invoice(Order order, double totalAmount)
        {
            InvoiceId = Guid.NewGuid().ToString();
            DateIssued = DateTime.Now;
            OrderReference = order;
            TotalAmount = totalAmount;
        }
    }
}
