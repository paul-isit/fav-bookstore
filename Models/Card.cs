namespace FavouriteBookstore.Models
{
    /// <summary>
    /// Concrete Strategy implementation for Credit/Debit Card payments (Section 3.3.7 of Object Design).
    /// </summary>
    public class Card : PaymentMethod
    {
        public string CardholderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;

        public override bool ProcessPayment(decimal amount)
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < 15 || string.IsNullOrWhiteSpace(CVV))
            {
                System.Console.WriteLine("[Payment] Card authorization failed: Invalid credit card credentials.");
                return false;
            }
            
            System.Console.WriteLine($"[Strategy: Credit Card] Successfully authorized ${amount:F2} for {CardholderName}.");
            return true;
        }
    }
}
