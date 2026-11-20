namespace SupplyTrackingSystem.Models
{
    public class Product
    {
        public int Article { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DepartmentCode { get; set; }

        public string PriceDisplay => $"{Price:N0} ₽";
        public override string ToString() => Name;
    }
}
