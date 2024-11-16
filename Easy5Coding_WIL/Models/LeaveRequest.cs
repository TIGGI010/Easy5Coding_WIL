using System;

namespace Easy5Coding_WIL.Models
{
    public class LeaveRequest
    {
        public string EmployeeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveRequestId { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Pending, Approved, etc.

        // Constructor to initialize default values if needed
        public LeaveRequest()
        {
            LeaveRequestId = string.Empty;
            LeaveType = string.Empty;
            Reason = string.Empty;
            Status = string.Empty;
        }
    }
}
