namespace TradeRestAPI.Models.Reports
{
    public class DepartmentSummaryReportDto
    {
        public int DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal DailyPlan { get; set; }
        public decimal ActualVolume { get; set; }
        public decimal ActualVolumeThousandRub => Math.Round(ActualVolume / 1000m, 3);
        public double CompletionPercent => DailyPlan > 0 ? (double)(ActualVolume / (DailyPlan * 10m)) * 100.0 : 0.0;
    }

    public class ProductRatingReportDto
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRevenueThousandRub => Math.Round(TotalRevenue / 1000m, 3);
        public decimal SharePercent { get; set; }
    }

    public class DailySalesReportDto
    {
        public DateOnly Date { get; set; }
        public int SalesCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRevenueThousandRub => Math.Round(TotalRevenue / 1000m, 3);
        public string TopProductName { get; set; } = string.Empty;
    }

    public class UnderperformersReportDto
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }
}
