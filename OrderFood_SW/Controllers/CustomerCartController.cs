using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff", "Customer")]
    public class CustomerCartController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public CustomerCartController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            var tableId = HttpContext.Session.GetInt32("CurrentTableId");
            var tableNumber = _db.Tables
                .Where(t => t.TableId == tableId)
                .Select(t => t.TableNumber)
                .FirstOrDefault();

            if (tableId == null || tableId == 0)
            {
                // Nếu chưa có tableId, buộc quay về chọn món lại
                TempData["Error"] = "Thiếu thông tin bàn, vui lòng chọn bàn trước khi đặt món.";
                return RedirectToAction("Index", "CustomerOrder");
            }

            ViewBag.TableId = tableId;
            ViewBag.TableNumber = tableNumber;
            return View(cart);
        }


        // Get cart count for counter updates
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            return Json(new { count = cart.Sum(x => x.Quantity) });
        }

        public IActionResult GetCart()
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OrderCartPartial", cart);
            }

            // Return full view for regular requests
            return PartialView("_CartPartial", cart);
        }

        [HttpPost]
        public IActionResult RemoveAllCart()
        {
            HttpContext.Session.Remove("Cart");
            return Json(new { success = true });
        }


        [HttpGet]
        public IActionResult Count()
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new();
            int count = cart.Sum(x => x.Quantity);
            return Json(new { count });
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity(int dishId, int change)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.DishId == dishId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found", count = cart.Sum(x => x.Quantity) });
            }

            item.Quantity += change;
            if (item.Quantity <= 0) cart.Remove(item);

            HttpContext.Session.SetObject("Cart", cart);
            int count = cart.Sum(x => x.Quantity);
            return Json(new { success = true, count });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new();
            var item = cart.FirstOrDefault(x => x.DishId == id);
            if (item != null) cart.Remove(item);

            HttpContext.Session.SetObject("Cart", cart);
            int count = cart.Sum(x => x.Quantity);
            return Json(new { success = true, count });
        }

    }
}
