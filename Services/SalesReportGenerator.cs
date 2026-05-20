namespace FavouriteBookstore.Controllers
{
    public class SalesReportGenerator
    {
        private static SalesReportGenerator? instance;

        private SalesReportGenerator() { }

        public static SalesReportGenerator Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new SalesReportGenerator();
                }
                return instance;
            }
        }
    }
}
