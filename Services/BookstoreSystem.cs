using System;
using FavouriteBookstore.Models;
using FavouriteBookstore.Data;

namespace FavouriteBookstore.Services
{
    public class BookstoreSystem
    {
        private static BookstoreSystem? _instance;
        private static readonly object _lock = new object();

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

        public override string ToString()
        {
            return "Favourite Books Online Bookstore System is running core business logic.";
        }
    }
}
