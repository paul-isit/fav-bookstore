namespace FavouriteBookstore.Models
{
    public class Book : Item
    {
        public string ISBN { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty;
        //TODO: Implement Cover Image & display in catalogue and item UI
        public int PublicationYear { get; set; }
        
        //The title of the book is just the Name string variable from Item.cs

        public BookStatus Status { get; set; } = BookStatus.Available;
        
        public enum BookStatus
        {
            Available,
            OutOfStock,
            Discontinued,
            Archived
        }
    }
}
