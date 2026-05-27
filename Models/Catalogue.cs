namespace FavouriteBookstore.Models
{
    public class Catalogue
    {
        private List<Book> books = new();

        public void AddBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }
            if (books.Any(b => b.Id == book.Id || b.ISBN == book.ISBN))
            {
                throw new InvalidOperationException("This book already exists in the catalogue.");
            }
            
            books.Add(book);
        }

        public Book? FindBookById(string id)
        {
            foreach (Book book in books)
            {
                if (book.Id == id)
                {
                    return book;
                }
            }
            return null;
        }

        public List<Book> SearchByTitle(string searchText)
        {
            List<Book> queryResults = new List<Book>();
            
            //TODO:

            return queryResults;
        }
        
        public List<Book> SearchByAuthor(string searchText)
        {
            List<Book> queryResults = new List<Book>();
            
            //TODO:

            return queryResults;
        }
        
        public List<Book> FilterByGenre(string genre)
        {
            List<Book> filterResults = new List<Book>();
            
            //TODO:

            return filterResults;
        }

        public List<Book> GetAvailableBooks()
        {
            List<Book> availableBooks = new List<Book>();

            foreach (Book book in books)
            {
                if (book.HasEnoughStock(1))
                {
                    availableBooks.Add(book);
                }
            }
            return availableBooks;
        }
    }
}
