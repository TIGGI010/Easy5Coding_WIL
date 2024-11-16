using System;

namespace Easy5Coding_WIL.Models
{
    public class EmployeeDetails
    {
        public string Address { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } // Add DateOfBirth field
        public string Department { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public string PositionLevel { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; } // Nullable to handle cases where not applicable
        public int LeaveBalance { get; set; }
        public bool IsActive { get; set; }
        

        // Constructor to initialize default values if needed
        public EmployeeDetails()
        {
            Address = string.Empty;
            ContactEmail = string.Empty;
            ContactPhone = string.Empty;
            Department = string.Empty;
            EmergencyContactName = string.Empty;
            EmergencyContactPhone = string.Empty;
            EmployeeID = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            JobRole = string.Empty;
            PositionLevel = string.Empty;
            Role = string.Empty;
            Salary = 0.0m;
            LeaveBalance = 0;
            IsActive = true;
        }
    }
}
