using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Services;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class StatisticController : Controller
    {
        private readonly StatisticService _service;

        public StatisticController(StatisticService service)
        {
            _service = service;
        }

        public IActionResult Revenue(string period = "day")
        {
            var model = _service.GetRevenue(period);
            return View(model);
        }

        public async Task<IActionResult> Order(int? month, int? year)
        {
            var model = await _service.GetOrderStatistic(month, year);
            ViewBag.month = month;
            ViewBag.year = year;
            return View(model);
        }

        public async Task<IActionResult> Employee()
        {
            return View();
        }
    }
}
