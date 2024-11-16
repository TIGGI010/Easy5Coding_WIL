using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Easy5Coding_WIL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Easy5Coding_WIL.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(FirebaseService firebaseService, ILogger<PayrollController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        // GET: Payroll
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var payrolls = await _firebaseService.GetPayrollsAsync();
                return View(payrolls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payroll records.");
                ModelState.AddModelError("", "Unable to load payroll records.");
                return View();
            }
        }

        // GET: Payroll/Create
        [Authorize(Roles = "HRManager, Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Payroll/Create
        [HttpPost]
        [Authorize(Roles = "HRManager, Manager")]
        public async Task<IActionResult> Create(Payroll payroll)
        {
            if (!ModelState.IsValid) return View(payroll);

            try
            {
                var employee = await _firebaseService.GetEmployeeByIdAsync(payroll.EmployeeID);
                if (employee == null)
                {
                    ModelState.AddModelError("", "Invalid Employee ID");
                    return View(payroll);
                }

                await _firebaseService.AddPayrollAsync(payroll);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payroll record.");
                ModelState.AddModelError("", "Error creating payroll record. Please try again.");
                return View(payroll);
            }
        }

        // GET: Payroll/Details/5
        // GET: Payroll/Details/5
        [Authorize(Roles = "Employee, HRManager, Manager")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var payroll = await _firebaseService.GetPayrollByIdAsync(id);
                if (payroll == null) return NotFound();

                // Restrict employees to only view their own payroll record
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (User.IsInRole("Employee") && payroll.EmployeeID != userId)
                {
                    return Forbid();
                }

                return View(payroll);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching payroll details for ID: {id}");
                return NotFound();
            }
        }

    }
}
