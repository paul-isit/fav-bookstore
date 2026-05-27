namespace FavouriteBookstore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Run all integration tests
            TestRunner.RunAllTests();

            // Build and run the web application
            Console.WriteLine("\n==================================================");
            Console.WriteLine("   Starting Web Application...                     ");
            Console.WriteLine("==================================================\n");

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}

