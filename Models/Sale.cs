using System;

namespace SupplyTrackingSystem.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int Article { get; set; }
        public DateTime SaleDate { get; set; }
        public int Quantity { get; set; }
        public int DepartmentCode { get; set; }

        public decimal TotalAmount => Quantity * Price;
        public decimal Price { get; set; }

        public string SaleDateDisplay => SaleDate.ToString("dd.MM.yyyy");
    }
}
