namespace SupplyTrackingSystem.Models
{
    public class Department
    {
        public int DepartmentCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal DailyPlan { get; set; }

        public override string ToString() => $"{Name} ({Manager})";
    }
}
