using Easy5Coding_WIL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Easy5Coding_WIL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Home page, accessible to all users, including those not authenticated
        [AllowAnonymous]
        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed.");
            return View();
        }

        // Privacy page, accessible only to users with "Manager" or "HRManager" roles
        [Authorize(Roles = "Manager,HRManager")]
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page accessed by user: {UserName}", User.Identity.Name);
            return View();
        }

        // Custom Access Denied page, shown when a user tries to access a restricted page
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied for user: {UserName}", User.Identity?.Name ?? "Unauthenticated");
            return View();
        }

        // Error page, accessible to all users (public)
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error occurred. RequestId: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
