namespace LeaveManagement.Models
{
    public class LeaveRequest
    {
        public int LeaveId { get; set; }       
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime DateRequested { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; }
        public string Remarks { get; set; }
    }

    public class LeaveRequestDto
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public string Department { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
        public DateTime DateRequested { get; set; }
    }

    public class UpdateLeaveStatusRequest
    {
        public string Status { get; set; }
    }

    public class LeaveStatusCountDto
    {
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class DashboardDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string ManagerName { get; set; }
        public int TotalApproved { get; set; }
        public int TotalPending { get; set; }
        public int TotalRejected { get; set; }
    }
}
