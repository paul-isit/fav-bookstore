using System;
using FavouriteBookstore;
using FavouriteBookstore.Services;
using Microsoft.Extensions.FileProviders;

// Run all integration tests before starting the app (keeps previous branch behavior)
TestRunner.RunAllTests();

Console.WriteLine("\n==================================================");
Console.WriteLine("   Starting Web Application...                     ");
Console.WriteLine("==================================================\n");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
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
