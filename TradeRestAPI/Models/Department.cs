using System.Text.Json.Serialization;

namespace TradeRestAPI.Models
{
    public class Department
    {
        public int DepartmentCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal DailyPlan { get; set; }

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        [JsonIgnore]
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
