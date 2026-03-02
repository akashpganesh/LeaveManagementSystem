namespace LeaveManagement.Models
{
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int MaxLeavesPerYear { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
