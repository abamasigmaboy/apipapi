using Microsoft.AspNetCore.Mvc;
using TradeRestAPI.Models;
using TradeRestAPI.Models.Data;
using TradeRestAPI.Models.Services;

namespace TradeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly ILogger<SaleController> logger;
        private readonly TradeDbContext db;
        private readonly SalesService service;

        public SaleController(TradeDbContext _db, ILogger<SaleController> _logger, SalesService _service)
        {
            db = _db;
            logger = _logger;
            service = _service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetPage([FromQuery] int? page)
        {
            var res = await service.GetPage(page);
            return Ok(res);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Sale>>> GetAll()
        {
            var res = await service.GetAll();
            return Ok(res);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Sale>>> Filter(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? departmentCode)
        {
            var res = await service.Filter(from, to, departmentCode);
            return Ok(res);
        }

        [HttpGet("{id:int}")]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
        public async Task<ActionResult<Sale>> GetById(int id)
        {
            var res = await service.Get(id);
            return res == null ? NotFound(new { message = "Запись о продаже не найдена" }) : Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<Sale>> Create([FromBody] Sale sale)
        {
            if (service.Create(sale))
                return CreatedAtAction(nameof(GetById), new { id = sale.SaleId }, sale);
            return BadRequest(new { message = "Не удалось сохранить продажу" });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Sale>> Update(int id, [FromBody] Sale sale)
        {
            if (sale.SaleId != id) return BadRequest(new { message = "Несовпадение идентификатора" });
            if (service.Update(id, sale))
                return Ok(sale);
            return NotFound(new { message = "Запись не найдена" });
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (service.Delete(id))
                return NoContent();
            return NotFound(new { message = "Запись не найдена" });
        }
    }
}
