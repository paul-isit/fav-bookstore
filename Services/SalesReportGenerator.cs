namespace FavouriteBookstore.Services
{
    /// <summary>
    /// NOTE FOR MARKER: This SalesReportGenerator singleton processor is defined in our original Object Design (Section 3.3.19),
    /// but is left unimplemented here because it relates to administrative sales analytics,
    /// which falls outside of the 4 core business operation scenarios implemented for Assignment 3.
    /// </summary>
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
