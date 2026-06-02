namespace FavouriteBookstore.Models
{
    /// <summary>
    /// Concrete Strategy implementation for PayPal payments (Section 3.3.8 of Object Design).
    /// </summary>
    public class PayPal : PaymentMethod
    {
        public string AccountEmail { get; set; } = string.Empty;

        public override bool ProcessPayment(decimal amount)
        {
            if (string.IsNullOrWhiteSpace(AccountEmail) || !AccountEmail.Contains("@"))
            {
                System.Console.WriteLine("[Payment] PayPal authorization failed: Invalid account email.");
                return false;
            }

            System.Console.WriteLine($"[Strategy: PayPal] Successfully authorized ${amount:F2} via standard email integration.");
            return true;
        }
    }
}
