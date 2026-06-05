using System;
using System.Linq;

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
            if (string.IsNullOrWhiteSpace(Street) ||
                string.IsNullOrWhiteSpace(Suburb) ||
                string.IsNullOrWhiteSpace(State) ||
                string.IsNullOrWhiteSpace(Postcode))
            {
                return false;
            }

            // State must be a valid Australian state or territory abbreviation (case-insensitive)
            string upperState = State.Trim().ToUpper();
            string[] validStates = { "VIC", "NSW", "QLD", "WA", "SA", "TAS", "ACT", "NT" };
            if (!validStates.Contains(upperState))
            {
                return false;
            }

            // Postcode must be a 4-digit numeric string
            string cleanPostcode = Postcode.Trim();
            if (cleanPostcode.Length != 4 || !cleanPostcode.All(char.IsDigit))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Street}, {Suburb}, {State} {Postcode}";
        }
    }
}
