using Microsoft.EntityFrameworkCore;
using TradeRestAPI.Models.Abstractions;
using TradeRestAPI.Models.Data;

namespace TradeRestAPI.Models.Services
{
    public class ProductsService : AbstractionService, ICommonService<Product, int>
    {
        private int pageSize = 10;
        private readonly TradeDbContext db;

        public ProductsService(TradeDbContext _db)
        {
            db = _db;
        }

        public bool Create(Product model)
        {
            return DoAction(() =>
            {
                db.Products.Add(model);
                db.SaveChanges();
            });
        }

        public bool Delete(int id)
        {
            return DoAction(() =>
            {
                var product = db.Products.FirstOrDefault(p => p.Article == id);
                if (product != null)
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                }
            });
        }

        public async Task<Product?> Get(int id)
        {
            return await db.Products
                .FirstOrDefaultAsync(p => p.Article == id);
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            return await db.Products
                .OrderBy(p => p.Article)
                .ToListAsync();
        }

        public bool Update(int id, Product model)
        {
            return DoAction(() =>
            {
                var res = db.Products.FirstOrDefault(p => p.Article == id);
                if (res != null)
                {
                    res.Name = model.Name;
                    res.Unit = model.Unit;
                    res.Price = model.Price;
                    res.DepartmentCode = model.DepartmentCode;
                    db.Products.Update(res);
                    db.SaveChanges();
                }
            });
        }

        public async Task<IEnumerable<Product>> GetPage(int? page)
        {
            int p = page.GetValueOrDefault(1);
            if (p < 1) p = 1;
            return await db.Products
                .OrderBy(p => p.Article)
                .Skip((p - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByDepartment(int departmentCode)
        {
            return await db.Products
                .Where(p => p.DepartmentCode == departmentCode)
                .OrderBy(p => p.Article)
                .ToListAsync();
        }
    }
}
