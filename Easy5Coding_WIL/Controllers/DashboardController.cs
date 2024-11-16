using Microsoft.AspNetCore.Mvc;
using Easy5Coding_WIL.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Easy5Coding_WIL.Controllers
{
    public class DashboardController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(FirebaseService firebaseService, ILogger<DashboardController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Dashboard()
        {
            // Get logged-in user's ID
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID is null. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Fetch user data
                var employee = await _firebaseService.GetUserByIdAsync(userId);

                if (employee == null)
                {
                    _logger.LogWarning("No data found for user ID: {UserId}. Showing empty dashboard.", userId);

                    // Return an empty User object for the view
                    return View(new User
                    {
                        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                        EmployeeDetails = new EmployeeDetails
                        {
                            FirstName = "No Data",
                            LastName = "Available"
                        }
                    });
                }

                // Return fetched data
                _logger.LogInformation("User data loaded for user ID: {UserId}.", userId);
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data for user ID: {UserId}", userId);
                return View(new User()); // Return empty User object on failure
            }
        }
    }
}
