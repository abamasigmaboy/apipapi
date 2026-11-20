using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TradeRestAPI.Models;
using TradeRestAPI.Models.Data;
using TradeRestAPI.Models.Services;

namespace TradeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly ILogger<DepartmentController> logger;
        private readonly TradeDbContext db;
        private readonly DepartmentsService service;
        private readonly IMemoryCache memoryCache;
        private const string CacheKey = "Departments_Cache";

        public DepartmentController(TradeDbContext _db, ILogger<DepartmentController> _logger, IMemoryCache _memoryCache, DepartmentsService _service)
        {
            db = _db;
            logger = _logger;
            memoryCache = _memoryCache;
            service = _service;
        }

        [HttpGet]
        [Produces(typeof(Department[]))]
        public async Task<ActionResult<IEnumerable<Department>>> GetPage([FromQuery] int? page)
        {
            string pKey = $"{CacheKey}_page_{page ?? 1}";
            if (!memoryCache.TryGetValue(pKey, out IEnumerable<Department>? cached))
            {
                cached = await service.GetPage(page);
                var entryOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10),
                    Size = cached.Count()
                };
                memoryCache.Set(pKey, cached, entryOptions);
            }

            var stats = memoryCache.GetCurrentStatistics();
            logger.LogInformation($"Memory cache. Hits: {stats?.TotalHits}. Size: {stats?.CurrentEstimatedSize}.");
            return Ok(cached ?? Enumerable.Empty<Department>());
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Department>>> GetAll()
        {
            var res = await service.GetAll();
            return Ok(res);
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Department>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Отдел не найден" }) : Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Department>> Create([FromBody] Department dept)
        {
            if (service.Create(dept))
                return CreatedAtAction(nameof(GetById), new { id = dept.DepartmentCode }, dept);
            return BadRequest(new { message = "Не удалось создать отдел" });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Department>> Update(int id, [FromBody] Department dept)
        {
            if (dept.DepartmentCode != id) return BadRequest(new { message = "Несовпадение идентификатора" });
            if (service.Update(id, dept))
                return Ok(dept);
            return NotFound(new { message = "Отдел не найден" });
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (service.Delete(id))
                return NoContent();
            return NotFound(new { message = "Отдел не найден" });
        }
    }
}
