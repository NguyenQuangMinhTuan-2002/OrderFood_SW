using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    public class CustomerOrderController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public CustomerOrderController(DatabaseHelperEF db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CreateOrder(string searchKeyword, int? tableId = null)
        {
            if (tableId.HasValue)
            {
                HttpContext.Session.SetInt32("CurrentTableId", tableId.Value);
            }

            ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;

            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(d => d.DishName.Contains(searchKeyword));
            }


            var dishes = query.OrderBy(d => d.DishName)
                              .ToList();

            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            var model = new OrderPageModel
            {
                SearchKeyword = searchKeyword,
                FoundDishes = dishes,
                CartItems = cart
            };

            return View(model);
        }
    }
}
