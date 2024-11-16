using System;

namespace Easy5Coding_WIL.Models
{
    public class Payroll
    {
        public decimal Bonuses { get; set; }
        public decimal Deductions { get; set; }
        public string EmployeeID { get; set; }
        public decimal NetPay { get; set; }
        public string Notes { get; set; }
        public DateTime PayDate { get; set; }
        public string PayPeriod { get; set; } // Monthly, Weekly, etc.
        public string PayrollId { get; set; }
        public decimal Salary { get; set; }

        // Constructor to initialize default values if needed
        public Payroll()
        {
            PayrollId = string.Empty;
            PayPeriod = string.Empty;
            Notes = string.Empty;
        }
    }
}
