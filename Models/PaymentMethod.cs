namespace FavouriteBookstore.Models
{
    public abstract class PaymentMethod
    {
        // Strategy pattern interface
        public abstract bool ProcessPayment(double amount);
    }
    
    // Dummy concrete implementation since child classes are excluded
    public class DummyPayment : PaymentMethod
    {
        public override bool ProcessPayment(double amount)
        {
            System.Console.WriteLine($"[DummyPayment] Successfully processed dummy payment of ${amount:F2}");
            return true;
        }
    }
}
