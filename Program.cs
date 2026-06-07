using FavouriteBookstore;
using FavouriteBookstore.Services;
using Microsoft.AspNetCore.DataProtection;

ResetDemoData();

// Run all integration tests before starting the app
TestRunner.RunAllTests();

Console.WriteLine("\n==================================================");
Console.WriteLine("   Starting Web Application...                     ");
Console.WriteLine("==================================================\n");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();

builder.Services.AddControllersWithViews();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5500", "http://localhost:5501", "http://localhost:3000", "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "FavouriteBookstore.Auth";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

// Get the single shared bookstore system.
BookstoreSystem bookstoreSystem = BookstoreSystem.Instance;

// Load every book from Infrastructure/data/books.json.
bookstoreSystem.LoadBooks();

// Manually choose which books appear in the website catalogue.
bookstoreSystem.RegisterBookToCatalogue("B01");
bookstoreSystem.RegisterBookToCatalogue("B02");
bookstoreSystem.RegisterBookToCatalogue("B03");
bookstoreSystem.RegisterBookToCatalogue("B04");
bookstoreSystem.RegisterBookToCatalogue("B05");
bookstoreSystem.RegisterBookToCatalogue("B06");
bookstoreSystem.RegisterBookToCatalogue("B07");
bookstoreSystem.RegisterBookToCatalogue("B08");
bookstoreSystem.RegisterBookToCatalogue("B09");
bookstoreSystem.RegisterBookToCatalogue("B10");
bookstoreSystem.RegisterBookToCatalogue("B11");
bookstoreSystem.RegisterBookToCatalogue("B12");

// Register this prepared bookstore system with ASP.NET.
builder.Services.AddSingleton(bookstoreSystem);

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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void ResetDemoData()
{
    string dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Infrastructure", "data");
    dataPath = Path.GetFullPath(dataPath);

    ResetDataFile(dataPath, "books.seed.json", "books.json");
    ResetDataFile(dataPath, "users.seed.json", "users.json");
}

static void ResetDataFile(string dataPath, string seedFileName, string activeFileName)
{
    string seedPath = Path.Combine(dataPath, seedFileName);
    string activePath = Path.Combine(dataPath, activeFileName);

    if (!File.Exists(seedPath))
    {
        return;
    }

    File.Copy(seedPath, activePath, overwrite: true);
}
