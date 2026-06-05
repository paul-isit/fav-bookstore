using FavouriteBookstore.Services;
using Microsoft.Extensions.FileProviders;

namespace FavouriteBookstore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ResetDemoData();

            // Run all integration tests
            TestRunner.RunAllTests();

            // Build and run the web application
            Console.WriteLine("\n==================================================");
            Console.WriteLine("   Starting Web Application...                     ");
            Console.WriteLine("==================================================\n");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
            builder.Services.AddSingleton(BookstoreSystem.Instance);

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            string websitePath = Path.Combine(app.Environment.ContentRootPath, "website");
            if (Directory.Exists(websitePath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(websitePath),
                    RequestPath = "/website"
                });
            }

            app.UseRouting();
            app.UseCors();

            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/website/index.html"));
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ResetDemoData()
        {
            string dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Infrastructure", "data");
            dataPath = Path.GetFullPath(dataPath);

            ResetDataFile(dataPath, "books.seed.json", "books.json");
            ResetDataFile(dataPath, "users.seed.json", "users.json");
        }

        private static void ResetDataFile(string dataPath, string seedFileName, string activeFileName)
        {
            string seedPath = Path.Combine(dataPath, seedFileName);
            string activePath = Path.Combine(dataPath, activeFileName);

            if (!File.Exists(seedPath))
            {
                return;
            }

            File.Copy(seedPath, activePath, overwrite: true);
        }
    }
}
