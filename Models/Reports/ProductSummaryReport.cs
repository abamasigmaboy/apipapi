namespace SupplyTrackingSystem.Models.Reports
{
    public class ProductSummaryReport
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgDailyQuantity { get; set; }
    }
}
