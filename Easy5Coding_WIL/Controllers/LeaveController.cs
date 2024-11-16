using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Easy5Coding_WIL.Models;
using Microsoft.AspNetCore.Authorization;

namespace Easy5Coding_WIL.Controllers
{
    // Base level authorization for any access to the LeaveController
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly FirebaseService _firebaseService;

        // Constructor to initialize the Firebase service
        public LeaveController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        // GET: Leave
        // Accessible by HR Managers and Managers to view all leave requests
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Index()
        {
            var leave = await _firebaseService.GetLeaveRequestsAsync();
            return View(leave);
        }

        // GET: Leave/RequestLeave
        // Accessible by Employees to request leave
        [Authorize(Roles = "Employee")]
        public IActionResult RequestLeave()
        {
            return View();
        }

        // POST: Leave/RequestLeave
        // Allows Employees to submit their leave request
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> RequestLeave(LeaveRequest leave)
        {
            if (ModelState.IsValid)
            {
                // Associate the leave request with the logged-in employee
                leave.EmployeeID = User.Identity.Name; // Use Firebase user ID or email to associate leave request
                await _firebaseService.AddLeaveRequestAsync(leave);  // Call Firebase service to save the request
                return RedirectToAction(nameof(MyLeaveRequests)); // Redirect to the page displaying the employee's leave requests
            }
            return View(leave); // Return the same view with errors if the model state is invalid
        }

        // GET: Leave/Details/5
        // Accessible by HR Managers and Managers to view details of a specific leave request
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Details(string id)
        {
            var leave = await _firebaseService.GetLeaveRequestByIdAsync(id);
            if (leave == null)
            {
                return NotFound(); // If no leave request found, return NotFound view
            }
            return View(leave);  // Return the details view for the leave request
        }

        // GET: Leave/MyLeaveRequests
        // Accessible by Employees to view their own leave requests
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyLeaveRequests()
        {
            var userId = User.Identity.Name;  // Use logged-in user's name (email/Firebase ID)
            var leave = await _firebaseService.GetLeaveRequestsByEmployeeIdAsync(userId); // Fetch the leave requests for the employee
            return View(leave);  // Return the list of leave requests
        }

    }
}
