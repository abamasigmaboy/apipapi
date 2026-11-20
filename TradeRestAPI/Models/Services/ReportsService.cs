using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TradeRestAPI.Models.Abstractions;
using TradeRestAPI.Models.Data;
using TradeRestAPI.Models.Reports;

namespace TradeRestAPI.Models.Services
{
    public class ReportsService : AbstractionService
    {
        private readonly TradeDbContext db;
        private static readonly string[] RussianMonthNames = new[]
        {
            "", "январь", "февраль", "март", "апрель", "май", "июнь",
            "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь"
        };

        public ReportsService(TradeDbContext _db)
        {
            db = _db;
        }

        /// <summary>
        /// Реализация отчёта Задания 7:
        /// Выручка от продажи товаров за месяц года
        /// с группировкой по отделам, строками товаров, итогами по отделу и итогами по магазину
        /// </summary>
        public async Task<Task7MonthlyRevenueReportDto> GetTask7MonthlyRevenue(int year, int month)
        {
            var monthName = (month >= 1 && month <= 12) ? RussianMonthNames[month] : month.ToString();

            var report = new Task7MonthlyRevenueReportDto
            {
                Year = year,
                Month = month,
                MonthName = monthName
            };

            // Выбираем продажи за указанный месяц и год
            var salesInMonth = await db.Sales
                .Include(s => s.ProductNavigation)
                .Include(s => s.DepartmentNavigation)
                .Where(s => s.SaleDate.Year == year && s.SaleDate.Month == month)
                .ToListAsync();

            // Получаем все отделы
            var departments = await db.Departments
                .OrderBy(d => d.DepartmentCode)
                .ToListAsync();

            foreach (var dept in departments)
            {
                var deptSales = salesInMonth.Where(s => s.DepartmentCode == dept.DepartmentCode).ToList();
                if (!deptSales.Any())
                    continue;

                var deptGroup = new DepartmentRevenueGroupDto
                {
                    DepartmentCode = dept.DepartmentCode,
                    DepartmentName = dept.Name
                };

                // Группируем по товарам
                var productGroups = deptSales
                    .GroupBy(s => s.Article)
                    .OrderBy(g => g.First().ProductNavigation?.Name ?? g.Key.ToString());

                foreach (var pg in productGroups)
                {
                    var product = pg.First().ProductNavigation;
                    if (product == null) continue;

                    int totalQty = pg.Sum(s => s.Quantity);
                    decimal priceThousand = Math.Round(product.Price / 1000m, 3);
                    decimal revenueThousand = Math.Round(priceThousand * totalQty, 3);
                    decimal revenueRub = product.Price * totalQty;

                    deptGroup.Products.Add(new ProductRevenueRowDto
                    {
                        Article = product.Article,
                        ProductName = product.Name,
                        Unit = product.Unit,
                        PriceRub = product.Price,
                        PriceThousandRub = priceThousand,
                        Quantity = totalQty,
                        RevenueRub = revenueRub,
                        RevenueThousandRub = revenueThousand
                    });
                }

                report.Departments.Add(deptGroup);
            }

            return report;
        }

        public async Task<IEnumerable<DepartmentSummaryReportDto>> GetDepartmentSummary(DateOnly? from = null, DateOnly? to = null)
        {
            var depts = await db.Departments.OrderBy(d => d.DepartmentCode).ToListAsync();
            var salesQuery = db.Sales.Include(s => s.ProductNavigation).AsQueryable();

            if (from.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= to.Value);

            var sales = await salesQuery.ToListAsync();

            return depts.Select(d =>
            {
                var deptSales = sales.Where(s => s.DepartmentCode == d.DepartmentCode);
                var actual = deptSales.Sum(s => s.Quantity * (s.ProductNavigation?.Price ?? 0m));

                return new DepartmentSummaryReportDto
                {
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.Name,
                    Manager = d.Manager,
                    Phone = d.Phone,
                    DailyPlan = d.DailyPlan,
                    ActualVolume = actual
                };
            }).ToList();
        }

        public async Task<IEnumerable<ProductRatingReportDto>> GetProductRating(DateOnly? from = null, DateOnly? to = null)
        {
            var salesQuery = db.Sales.Include(s => s.ProductNavigation).AsQueryable();

            if (from.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= to.Value);

            var sales = await salesQuery.ToListAsync();

            var groups = sales
                .Where(s => s.ProductNavigation != null)
                .GroupBy(s => s.Article)
                .Select(g => new
                {
                    Article = g.Key,
                    ProductName = g.First().ProductNavigation!.Name,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    TotalRevenue = g.Sum(s => s.Quantity * g.First().ProductNavigation!.Price)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            decimal grandTotal = groups.Sum(x => x.TotalRevenue);

            return groups.Select(x => new ProductRatingReportDto
            {
                Article = x.Article,
                ProductName = x.ProductName,
                TotalQuantity = x.TotalQuantity,
                TotalRevenue = x.TotalRevenue,
                SharePercent = grandTotal > 0 ? Math.Round((x.TotalRevenue / grandTotal) * 100m, 2) : 0m
            }).ToList();
        }

        public async Task<IEnumerable<DailySalesReportDto>> GetDailySales(DateOnly? from = null, DateOnly? to = null)
        {
            var salesQuery = db.Sales.Include(s => s.ProductNavigation).AsQueryable();

            if (from.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= to.Value);

            var sales = await salesQuery.ToListAsync();

            return sales
                .GroupBy(s => s.SaleDate)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var topProduct = g
                        .Where(s => s.ProductNavigation != null)
                        .GroupBy(s => s.ProductNavigation!.Name)
                        .OrderByDescending(pg => pg.Sum(s => s.Quantity * s.ProductNavigation!.Price))
                        .FirstOrDefault()?.Key ?? "";

                    return new DailySalesReportDto
                    {
                        Date = g.Key,
                        SalesCount = g.Count(),
                        TotalQuantity = g.Sum(s => s.Quantity),
                        TotalRevenue = g.Sum(s => s.Quantity * (s.ProductNavigation?.Price ?? 0m)),
                        TopProductName = topProduct
                    };
                }).ToList();
        }

        public async Task<IEnumerable<UnderperformersReportDto>> GetUnderperformers(DateOnly? from = null, DateOnly? to = null)
        {
            var salesQuery = db.Sales.AsQueryable();
            if (from.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= to.Value);

            var soldArticles = await salesQuery.Select(s => s.Article).Distinct().ToListAsync();

            var unsoldProducts = await db.Products
                .Include(p => p.DepartmentNavigation)
                .Where(p => !soldArticles.Contains(p.Article))
                .OrderBy(p => p.Article)
                .ToListAsync();

            return unsoldProducts.Select(p => new UnderperformersReportDto
            {
                Article = p.Article,
                ProductName = p.Name,
                Unit = p.Unit,
                Price = p.Price,
                DepartmentCode = p.DepartmentCode,
                DepartmentName = p.DepartmentNavigation?.Name ?? ""
            }).ToList();
        }
    }
}
