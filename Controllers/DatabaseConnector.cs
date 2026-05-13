namespace FavouriteBookstore.Controllers
{
    public class DatabaseConnector
    {
        private static DatabaseConnector? instance;

        private DatabaseConnector() { }

        public static DatabaseConnector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DatabaseConnector();
                }

                return instance;
            }
        }
    }
}
