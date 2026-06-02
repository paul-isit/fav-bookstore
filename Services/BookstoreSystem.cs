using FavouriteBookstore.Data;
using FavouriteBookstore.Models;
using System;
using System.Text.Json;

namespace FavouriteBookstore.Services
{
    public class BookstoreSystem
    {
        private static BookstoreSystem? _instance;
        private static readonly object _lock = new object();
        private List<Book> books = new List<Book>();    //All books in the system, not just from catalogue.

        // Subsystem managers maintained by the Facade
        public Catalogue Catalogue { get; private set; }
        public AccountManager AccountManager { get; private set; }

        private BookstoreSystem()
        {
            // Coordinate system bootstrap and component initialization
            Console.WriteLine("[Bootstrap] Initializing core bookstore subsystems...");
            
            // Wire up the shared Singleton Database Connector
            DatabaseConnector db = DatabaseConnector.GetInstance();

            // Instantiate domain coordinators
            Catalogue = new Catalogue();
            AccountManager = new AccountManager(db);
            
            Console.WriteLine("[Bootstrap] Subsystems successfully initialized and wired.");
        }

        public static BookstoreSystem Instance
        {
            get
            {
                // Thread-safe singleton double-check lock pattern
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BookstoreSystem();
                        }
                    }
                }
                return _instance;
            }
        }

        public void LoadBooks()
        {
            DatabaseConnector db = DatabaseConnector.GetInstance();

            books = db.GetAllBooks();
        }

        // Returns every book loaded from books.json.
        // This includes books that are NOT shown in the catalogue.
        public List<Book> GetBooks()
        {
            return books;
        }

        // Returns only books that have been registered into the catalogue.
        // This is what the website should display.
        public List<Book> GetCatalogueBooks()
        {
            return Catalogue.GetAvailableBooks();
        }

        // Registers a specific Book object into the catalogue.
        public void RegisterBook(Book book)
        {
            Catalogue.RegisterBook(book);
        }


        // Finds a book from the full books list, then registers it into the catalogue.
        // This is useful now in Program.cs, and later for the admin feature.
        public bool RegisterBookToCatalogue(string bookId)
        {
            foreach (Book book in books)
            {
                if (book.Id.Equals(bookId, StringComparison.OrdinalIgnoreCase))
                {
                    Catalogue.RegisterBook(book);
                    return true;
                }
            }

            // No matching book was found.
            return false;
        }

        public override string ToString()
        {
            return "Favourite Books Online Bookstore System is running core business logic.";
        }
    }
}
