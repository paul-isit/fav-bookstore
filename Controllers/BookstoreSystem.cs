namespace FavouriteBookstore.Controllers
{
    public class BookstoreSystem
    {
        private static BookstoreSystem? instance;

        private BookstoreSystem() { }

        public static BookstoreSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BookstoreSystem();
                }

                return instance;
            }
        }

        public override string ToString()
        {
            return "Favourite Books Online Bookstore System is running";
        }
    }
}
