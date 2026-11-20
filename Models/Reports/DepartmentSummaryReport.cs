namespace SupplyTrackingSystem.Models.Reports
{
    public class DepartmentSummaryReport
    {
        public string DepartmentName { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal DailyPlan { get; set; }
        public decimal ActualVolume { get; set; }
        public decimal CompletionPercent => DailyPlan > 0 ? ActualVolume / DailyPlan * 100 : 0;
        public string StatusColor
        {
            get
            {
                if (CompletionPercent < 80) return "#FF4444";
                if (CompletionPercent < 100) return "#FFAA00";
                return "#44AA44";
            }
        }
    }
}
