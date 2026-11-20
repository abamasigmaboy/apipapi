namespace SupplyTrackingSystem.Models.Reports
{
    public class ProductRatingReport
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal SharePercent { get; set; }
    }
}
