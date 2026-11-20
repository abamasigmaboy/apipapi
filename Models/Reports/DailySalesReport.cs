using System;

namespace SupplyTrackingSystem.Models.Reports
{
    public class DailySalesReport
    {
        public DateTime Date { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgCheck => SalesCount > 0 ? TotalRevenue / SalesCount : 0;
        public string TopProductName { get; set; } = string.Empty;
        public string DateDisplay => Date.ToString("dd.MM.yyyy");
    }
}
