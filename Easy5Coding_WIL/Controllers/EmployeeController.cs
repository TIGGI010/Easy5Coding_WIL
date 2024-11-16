using Easy5Coding_WIL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Easy5Coding_WIL.Controllers
{
    [Authorize(Roles = "Employee, HRManager, Manager")]
    public class EmployeeController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(FirebaseService firebaseService, ILogger<EmployeeController> logger)
        {
            _firebaseService = firebaseService;  // Simply assign the injected service
            _logger = logger;  // Assign the injected logger
        }

        // GET: Employee/Index
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.Identity.Name;  // Get logged-in user's UID
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Unauthorized access attempt to Employee/Index.");
                return Unauthorized("User is not logged in.");
            }

            var currentUserRole = User.IsInRole("Employee") ? "Employee" :
                                  (User.IsInRole("HRManager") ? "HRManager" : "Manager");

            try
            {
                if (currentUserRole == "Employee")
                {
                    var employee = await _firebaseService.GetUserByIdAsync(currentUserId);
                    if (employee == null || employee.EmployeeDetails == null)
                    {
                        _logger.LogWarning($"Employee details not found for user ID {currentUserId}.");
                        return NotFound();
                    }

                    _logger.LogInformation($"Fetched details for employee: {employee.EmployeeDetails.FirstName} {employee.EmployeeDetails.LastName}");
                    return View(new[] { employee.EmployeeDetails });
                }
                else
                {
                    var users = await _firebaseService.GetUsersAsync();
                    var employeeDetailsList = users
                        .Where(u => u.EmployeeDetails != null)
                        .Select(u => u.EmployeeDetails)
                        .ToList();

                    _logger.LogInformation($"Fetched {employeeDetailsList.Count} employees for role {currentUserRole}.");
                    return View(employeeDetailsList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee list.");
                ModelState.AddModelError("", "Unable to retrieve employees at this time.");
                return View(Enumerable.Empty<EmployeeDetails>());
            }
        }

        // GET: Employee/Create
        [Authorize(Roles = "HRManager, Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Create(EmployeeDetails employee)
        {
            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                employee.EmployeeID = Guid.NewGuid().ToString(); // Assign unique ID
                await _firebaseService.AddEmployeeAsync(employee);

                _logger.LogInformation($"Employee {employee.FirstName} {employee.LastName} created successfully.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new employee.");
                ModelState.AddModelError("", "Error adding employee. Please try again.");
                return View(employee);
            }
        }

        // GET: Employee/Edit/5
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Employee ID is required.");

            try
            {
                var employee = await _firebaseService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {id} not found.");
                    return NotFound();
                }

                _logger.LogInformation($"Editing employee: {employee.FirstName} {employee.LastName}");
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching employee with ID {id}.");
                return NotFound();
            }
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [Authorize(Roles = "HRManager, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EmployeeDetails employee)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Employee ID is required.");

            if (id != employee.EmployeeID)
                return BadRequest("Employee ID mismatch.");

            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                await _firebaseService.UpdateEmployeeAsync(id, employee);
                _logger.LogInformation($"Employee {employee.FirstName} {employee.LastName} updated successfully.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee with ID {id}.");
                ModelState.AddModelError("", "Error updating employee. Please try again.");
                return View(employee);
            }
        }

        // DELETE: Employee/DeleteEmployee
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteEmployee(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return BadRequest("Employee ID is required.");

            try
            {
                await _firebaseService.DeleteEmployeeAsync(employeeId);
                _logger.LogInformation($"Employee with ID {employeeId} deleted successfully.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting employee with ID {employeeId}.");
                return RedirectToAction("Error");
            }
        }

        // GET: Employee/MyDetails
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyDetails()
        {
            var userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not logged in.");

            try
            {
                var employee = await _firebaseService.GetUserByIdAsync(userId);
                if (employee == null || employee.EmployeeDetails == null)
                {
                    _logger.LogWarning($"Details not found for employee with ID {userId}.");
                    return NotFound();
                }

                _logger.LogInformation($"Viewing details for employee: {employee.EmployeeDetails.FirstName} {employee.EmployeeDetails.LastName}");
                return View(employee.EmployeeDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching details for user {userId}.");
                return NotFound();
            }
        }
        public async Task<IActionResult> Dashboard()
        {
            // Retrieve logged-in user's ID
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated

            // Fetch employee data from Firebase
            var employee = await _firebaseService.GetUserByIdAsync(userId);
            if (employee == null)
                return NotFound(); // Show 404 if employee data not found

            return View(employee); // Pass the employee data to the view
        }

    }
}
