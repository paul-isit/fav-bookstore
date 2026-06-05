using System;
using FavouriteBookstore;
using FavouriteBookstore.Services;
using Microsoft.AspNetCore.DataProtection;

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
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
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