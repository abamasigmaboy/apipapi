namespace SupplyTrackingSystem.Models.Reports
{
    public class UnderperformersReport
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DepartmentCode { get; set; }
    }
}
