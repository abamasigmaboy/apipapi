using System.Text.Json.Serialization;

namespace TradeRestAPI.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int Article { get; set; }
        public DateOnly SaleDate { get; set; }
        public int Quantity { get; set; }
        public int DepartmentCode { get; set; }

        public decimal TotalAmountRub { get; set; }

        public decimal TotalAmountThousandRub => Math.Round(TotalAmountRub / 1000m, 3);

        [JsonIgnore]
        public virtual Product? ProductNavigation { get; set; }

        [JsonIgnore]
        public virtual Department? DepartmentNavigation { get; set; }
    }
}
