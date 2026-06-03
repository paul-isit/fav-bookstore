namespace FavouriteBookstore.Models
{
    public class Catalogue
    {
        private List<Book> availableBooks = new();

        public void RegisterBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }
            if (availableBooks.Any(b => b.Id == book.Id))    // -- OLD IF STATEMENT -- if (availableBooks.Any(b => b.Id == book.Id || b.ISBN == book.ISBN))
            {
                throw new InvalidOperationException("This book already exists in the catalogue.");
            }

            availableBooks.Add(book);
        }

        public Book? FindBookById(string id)
        {
            foreach (Book book in availableBooks)
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

            foreach (Book book in availableBooks)
            {
                if (book.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    queryResults.Add(book);
                }
            }

            return queryResults;
        }
        
        public List<Book> SearchByAuthor(string searchText)
        {
            List<Book> queryResults = new List<Book>();

            foreach (Book book in availableBooks)
            {
                if (book.Author.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    queryResults.Add(book);
                }
            }

            return queryResults;
        }
        
        public List<Book> FilterByGenre(string genre)
        {
            List<Book> filterResults = new List<Book>();

            foreach (Book book in availableBooks)
            {
                if (book.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                {
                    filterResults.Add(book);
                }
            }

            return filterResults;
        }

        public List<Book> GetAvailableBooks()
        {
            List<Book> availableBooks = new List<Book>();

            foreach (Book book in this.availableBooks)
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
