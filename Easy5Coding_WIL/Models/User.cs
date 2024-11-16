using System;
using System.Collections.Generic;

namespace Easy5Coding_WIL.Models
{
    public class User
    {
        public string Uid { get; set; } // Firebase UID
        public string Email { get; set; }
        public string Password { get; set; } // Use hashed password
        public string Role { get; set; } // "Manager", "Employee", etc.
        public EmployeeDetails EmployeeDetails { get; set; }
        public List<LeaveRequest> LeaveRequests { get; set; }
        public Payroll Payroll { get; set; }

        public User()
        {
            LeaveRequests = new List<LeaveRequest>(); // Initialize the list to prevent null reference
        }
    }
}
