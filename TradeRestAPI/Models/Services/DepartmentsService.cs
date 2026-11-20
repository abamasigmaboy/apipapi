using Microsoft.EntityFrameworkCore;
using TradeRestAPI.Models.Abstractions;
using TradeRestAPI.Models.Data;

namespace TradeRestAPI.Models.Services
{
    public class DepartmentsService : AbstractionService, ICommonService<Department, int>
    {
        private int pageSize = 10;
        private readonly TradeDbContext db;

        public DepartmentsService(TradeDbContext _db)
        {
            db = _db;
        }

        public bool Create(Department model)
        {
            return DoAction(() =>
            {
                db.Departments.Add(model);
                db.SaveChanges();
            });
        }

        public bool Delete(int id)
        {
            return DoAction(() =>
            {
                var dep = db.Departments.FirstOrDefault(p => p.DepartmentCode == id);
                if (dep != null)
                {
                    db.Departments.Remove(dep);
                    db.SaveChanges();
                }
            });
        }

        public async Task<Department?> Get(int id)
        {
            return await db.Departments
                .FirstOrDefaultAsync(p => p.DepartmentCode == id);
        }

        public async Task<IEnumerable<Department>> GetAll()
        {
            return await db.Departments
                .OrderBy(d => d.DepartmentCode)
                .ToListAsync();
        }

        public bool Update(int id, Department model)
        {
            return DoAction(() =>
            {
                var res = db.Departments.FirstOrDefault(p => p.DepartmentCode == id);
                if (res != null)
                {
                    res.Name = model.Name;
                    res.Manager = model.Manager;
                    res.Phone = model.Phone;
                    res.DailyPlan = model.DailyPlan;
                    db.Departments.Update(res);
                    db.SaveChanges();
                }
            });
        }

        public async Task<IEnumerable<Department>> GetPage(int? page)
        {
            int p = page.GetValueOrDefault(1);
            if (p < 1) p = 1;
            return await db.Departments
                .OrderBy(p => p.DepartmentCode)
                .Skip((p - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
