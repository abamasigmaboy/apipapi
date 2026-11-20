namespace TradeRestAPI.Models.Reports
{
    /// <summary>
    /// Модель отчёта по Заданию 7: Выручка от продажи товаров за месяц года
    /// </summary>
    public class Task7MonthlyRevenueReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string Title => $"Выручка от продажи товаров за {MonthName} {Year} года";

        public List<DepartmentRevenueGroupDto> Departments { get; set; } = new();

        /// <summary>
        /// Итого по магазину (в тыс. руб.)
        /// </summary>
        public decimal StoreTotalRevenueThousandRub =>
            Math.Round(Departments.Sum(d => d.DepartmentTotalRevenueThousandRub), 3);

        /// <summary>
        /// Итого по магазину (в рублях)
        /// </summary>
        public decimal StoreTotalRevenueRub =>
            Departments.Sum(d => d.DepartmentTotalRevenueRub);

        public int StoreTotalQuantity =>
            Departments.Sum(d => d.DepartmentTotalQuantity);
    }

    /// <summary>
    /// Группа данных по конкретному отделу ("Отдел ...")
    /// </summary>
    public class DepartmentRevenueGroupDto
    {
        public int DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentHeader => $"Отдел: {DepartmentName} (Код: {DepartmentCode})";

        public List<ProductRevenueRowDto> Products { get; set; } = new();

        /// <summary>
        /// Итого по отделу (в тыс. руб.)
        /// </summary>
        public decimal DepartmentTotalRevenueThousandRub =>
            Math.Round(Products.Sum(p => p.RevenueThousandRub), 3);

        /// <summary>
        /// Итого по отделу (в рублях)
        /// </summary>
        public decimal DepartmentTotalRevenueRub =>
            Products.Sum(p => p.RevenueRub);

        public int DepartmentTotalQuantity =>
            Products.Sum(p => p.Quantity);
    }

    /// <summary>
    /// Строка таблицы отчёта:
    /// Наименование товара | Единица измерения | Цена розничная, тыс. руб. | Количество товара | Выручка, тыс. руб.
    /// </summary>
    public class ProductRevenueRowDto
    {
        public int Article { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Цена розничная, тыс. руб.
        /// </summary>
        public decimal PriceThousandRub { get; set; }

        /// <summary>
        /// Количество товара
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Выручка, тыс. руб. = "Цена розничная, тыс. руб." x "Количество товара"
        /// </summary>
        public decimal RevenueThousandRub { get; set; }

        /// <summary>
        /// Цена в рублях (для справки)
        /// </summary>
        public decimal PriceRub { get; set; }

        /// <summary>
        /// Выручка в рублях (для справки)
        /// </summary>
        public decimal RevenueRub { get; set; }
    }
}
