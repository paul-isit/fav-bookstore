namespace FavouriteBookstore.Models
{
    /// <summary>
    /// Data-holder class for Customer/Guest Shipping Address as defined in Section 3.3.10 of the Object Design.
    /// Includes validation support to satisfy the input validation requirement of Scenario 4.
    /// </summary>
    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string Suburb { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Street) &&
                   !string.IsNullOrWhiteSpace(Suburb) &&
                   !string.IsNullOrWhiteSpace(State) &&
                   !string.IsNullOrWhiteSpace(Postcode);
        }

        public override string ToString()
        {
            return $"{Street}, {Suburb}, {State} {Postcode}";
        }
    }
}
