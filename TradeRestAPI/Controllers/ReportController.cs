using Microsoft.AspNetCore.Mvc;
using TradeRestAPI.Models.Reports;
using TradeRestAPI.Models.Services;

namespace TradeRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ReportsService reportService;

        public ReportController(ReportsService _reportService)
        {
            reportService = _reportService;
        }

        /// <summary>
        /// ЗАДАНИЕ 7: Выручка от продажи товаров за месяц года.
        /// Возвращает строки с товарами (наименование, единица измерения, цена розничная тыс. руб.,
        /// количество, выручка тыс. руб.), сгруппированные по отделам, с итогами по каждому отделу
        /// и общим итогом по магазину.
        /// </summary>
        [HttpGet("task7-monthly-revenue")]
        public async Task<ActionResult<Task7MonthlyRevenueReportDto>> GetTask7MonthlyRevenue(
            [FromQuery] int year = 2025,
            [FromQuery] int month = 5)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Номер месяца должен быть от 1 до 12" });

            if (year < 2000 || year > 2100)
                return BadRequest(new { message = "Некорректный год" });

            var result = await reportService.GetTask7MonthlyRevenue(year, month);
            return Ok(result);
        }

        /// <summary>
        /// Сводка по отделам и выполнению плана
        /// </summary>
        [HttpGet("department-summary")]
        public async Task<ActionResult<IEnumerable<DepartmentSummaryReportDto>>> GetDepartmentSummary(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            var result = await reportService.GetDepartmentSummary(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Рейтинг товаров по выручке
        /// </summary>
        [HttpGet("product-rating")]
        public async Task<ActionResult<IEnumerable<ProductRatingReportDto>>> GetProductRating(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            var result = await reportService.GetProductRating(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Динамика выручки по дням
        /// </summary>
        [HttpGet("daily-sales")]
        public async Task<ActionResult<IEnumerable<DailySalesReportDto>>> GetDailySales(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            var result = await reportService.GetDailySales(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Товары без продаж
        /// </summary>
        [HttpGet("underperformers")]
        public async Task<ActionResult<IEnumerable<UnderperformersReportDto>>> GetUnderperformers(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            var result = await reportService.GetUnderperformers(from, to);
            return Ok(result);
        }
    }
}
