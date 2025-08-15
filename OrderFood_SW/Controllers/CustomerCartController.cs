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

            if (tableId == null || tableId == 0)
            {
                // Nếu chưa có tableId, buộc quay về chọn món lại
                TempData["Error"] = "Thiếu thông tin bàn, vui lòng chọn bàn trước khi đặt món.";
                return RedirectToAction("Index", "CustomerOrder");
            }

            ViewBag.TableId = tableId;
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
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            var itemToRemove = cart.FirstOrDefault(x => x.DishId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObject("Cart", cart);
            }

            return Json(new { success = true, count = cart.Count });
        }

        [HttpPost]
        public IActionResult RemoveAllCart()
        {
            HttpContext.Session.Remove("Cart");
            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult UpdateCartQuantity(int dishId, int change)
        {
            try
            {
                var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
                var item = cart.FirstOrDefault(x => x.DishId == dishId);

                if (item != null)
                {
                    item.Quantity += change;

                    // Remove item if quantity becomes 0 or negative
                    if (item.Quantity <= 0)
                    {
                        cart.Remove(item);
                    }

                    HttpContext.Session.SetObject("Cart", cart);
                }

                return Json(new { success = true, count = cart.Sum(x => x.Quantity) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
