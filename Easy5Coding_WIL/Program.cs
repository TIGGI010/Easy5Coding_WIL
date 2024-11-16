using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Easy5Coding_WIL.Models;
using Easy5Coding_WIL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add configuration file (appsettings.json)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Initialize Firebase if not already initialized
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(builder.Configuration["Firebase:JsonPath"]) // Ensure this path is correct in your appsettings
    });
}

// Register HttpClient for use in FirebaseService
builder.Services.AddHttpClient();
builder.Services.AddLogging(); // Add logging services

// Register FirebaseService as a singleton and pass configuration settings
builder.Services.AddSingleton<FirebaseService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<FirebaseService>>();  // Ensure logger is available
    var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];
    var firebaseDatabaseUrl = builder.Configuration["Firebase:DatabaseUrl"];

    return new FirebaseService(httpClientFactory, firebaseApiKey, firebaseDatabaseUrl, logger);
});

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login page if not authenticated
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect if access denied
        options.Cookie.HttpOnly = true; // Prevent access via JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use cookies over HTTPS when appropriate
        options.Cookie.SameSite = SameSiteMode.Strict; // Prevent CSRF attacks
    });

// Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Environment-specific error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Shows detailed error pages in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Default error page for production
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable Authentication Middleware
app.UseAuthorization();  // Enable Authorization Middleware

// Routing setup for controllers and views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Default route set to Account/Login

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Dashboard}/{id?}",
    defaults: new { controller = "Dashboard", action = "Dashboard" });

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Start the application
app.Run();
