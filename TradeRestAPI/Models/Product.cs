using System.Text.Json.Serialization;

namespace TradeRestAPI.Models
{
    public class Product
    {
        public int Article { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DepartmentCode { get; set; }

        public decimal PriceThousandRub => Math.Round(Price / 1000m, 3);

        [JsonIgnore]
        public virtual Department? DepartmentNavigation { get; set; }

        [JsonIgnore]
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
