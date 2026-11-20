using Microsoft.EntityFrameworkCore;
using TradeRestAPI.Models.Abstractions;
using TradeRestAPI.Models.Data;

namespace TradeRestAPI.Models.Services
{
    public class SalesService : AbstractionService, ICommonService<Sale, int>
    {
        private int pageSize = 15;
        private readonly TradeDbContext db;

        public SalesService(TradeDbContext _db)
        {
            db = _db;
        }

        public bool Create(Sale model)
        {
            return DoAction(() =>
            {
                var product = db.Products.FirstOrDefault(p => p.Article == model.Article);
                if (product != null && model.DepartmentCode == 0)
                {
                    model.DepartmentCode = product.DepartmentCode;
                }
                db.Sales.Add(model);
                db.SaveChanges();
            });
        }

        public bool Delete(int id)
        {
            return DoAction(() =>
            {
                var sale = db.Sales.FirstOrDefault(p => p.SaleId == id);
                if (sale != null)
                {
                    db.Sales.Remove(sale);
                    db.SaveChanges();
                }
            });
        }

        public async Task<Sale?> Get(int id)
        {
            var sale = await db.Sales
                .Include(s => s.ProductNavigation)
                .Include(s => s.DepartmentNavigation)
                .FirstOrDefaultAsync(p => p.SaleId == id);

            if (sale != null && sale.ProductNavigation != null)
            {
                sale.TotalAmountRub = sale.Quantity * sale.ProductNavigation.Price;
            }
            return sale;
        }

        public async Task<IEnumerable<Sale>> GetAll()
        {
            var list = await db.Sales
                .Include(s => s.ProductNavigation)
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.SaleId)
                .ToListAsync();

            foreach (var sale in list)
            {
                if (sale.ProductNavigation != null)
                {
                    sale.TotalAmountRub = sale.Quantity * sale.ProductNavigation.Price;
                }
            }
            return list;
        }

        public bool Update(int id, Sale model)
        {
            return DoAction(() =>
            {
                var res = db.Sales.FirstOrDefault(p => p.SaleId == id);
                if (res != null)
                {
                    res.Article = model.Article;
                    res.SaleDate = model.SaleDate;
                    res.Quantity = model.Quantity;
                    res.DepartmentCode = model.DepartmentCode;
                    db.Sales.Update(res);
                    db.SaveChanges();
                }
            });
        }

        public async Task<IEnumerable<Sale>> GetPage(int? page)
        {
            int p = page.GetValueOrDefault(1);
            if (p < 1) p = 1;
            var list = await db.Sales
                .Include(s => s.ProductNavigation)
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.SaleId)
                .Skip((p - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var sale in list)
            {
                if (sale.ProductNavigation != null)
                {
                    sale.TotalAmountRub = sale.Quantity * sale.ProductNavigation.Price;
                }
            }
            return list;
        }

        public async Task<IEnumerable<Sale>> Filter(DateOnly? from, DateOnly? to, int? departmentCode)
        {
            var query = db.Sales.Include(s => s.ProductNavigation).AsQueryable();

            if (from.HasValue)
                query = query.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue)
                query = query.Where(s => s.SaleDate <= to.Value);
            if (departmentCode.HasValue && departmentCode.Value > 0)
                query = query.Where(s => s.DepartmentCode == departmentCode.Value);

            var list = await query
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.SaleId)
                .ToListAsync();

            foreach (var sale in list)
            {
                if (sale.ProductNavigation != null)
                {
                    sale.TotalAmountRub = sale.Quantity * sale.ProductNavigation.Price;
                }
            }
            return list;
        }
    }
}
