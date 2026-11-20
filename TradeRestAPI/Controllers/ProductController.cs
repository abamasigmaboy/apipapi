using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TradeRestAPI.Models;
using TradeRestAPI.Models.Data;
using TradeRestAPI.Models.Services;

namespace TradeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> logger;
        private readonly TradeDbContext db;
        private readonly ProductsService service;
        private readonly IMemoryCache memoryCache;
        private const string CacheKey = "Products_Cache";

        public ProductController(TradeDbContext _db, ILogger<ProductController> _logger, IMemoryCache _memoryCache, ProductsService _service)
        {
            db = _db;
            logger = _logger;
            memoryCache = _memoryCache;
            service = _service;
        }

        [HttpGet]
        [Produces(typeof(Product[]))]
        public async Task<ActionResult<IEnumerable<Product>>> GetPage([FromQuery] int? page)
        {
            string pKey = $"{CacheKey}_page_{page ?? 1}";
            if (!memoryCache.TryGetValue(pKey, out IEnumerable<Product>? cached))
            {
                cached = await service.GetPage(page);
                var entryOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10),
                    Size = cached.Count()
                };
                memoryCache.Set(pKey, cached, entryOptions);
            }

            return Ok(cached ?? Enumerable.Empty<Product>());
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var res = await service.GetAll();
            return Ok(res);
        }

        [HttpGet("by-department/{departmentCode:int}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetByDepartment(int departmentCode)
        {
            var res = await service.GetByDepartment(departmentCode);
            return Ok(res);
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Товар не найден" }) : Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (service.Create(product))
                return CreatedAtAction(nameof(GetById), new { id = product.Article }, product);
            return BadRequest(new { message = "Не удалось добавить товар" });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
        {
            if (product.Article != id) return BadRequest(new { message = "Несовпадение артикула" });
            if (service.Update(id, product))
                return Ok(product);
            return NotFound(new { message = "Товар не найден" });
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (service.Delete(id))
                return NoContent();
            return NotFound(new { message = "Товар не найден" });
        }
    }
}
