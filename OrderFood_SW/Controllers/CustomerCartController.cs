using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff", "Customer")]
    public class CustomerCartController : Controller
    {
        private readonly CartService _cartService;

        public CustomerCartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            var (tableId, tableNumber) = _cartService.GetCurrentTable();

            if (tableId == null || tableId == 0)
            {
                TempData["Error"] = "Thiếu thông tin bàn, vui lòng chọn bàn trước khi đặt món.";
                return RedirectToAction("Index", "CustomerOrder");
            }

            ViewBag.TableId = tableId;
            ViewBag.TableNumber = tableNumber;
            return View(cart);
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            int count = _cartService.GetCartCount();
            return Json(new { count });
        }

        public IActionResult GetCart()
        {
            var cart = _cartService.GetCart();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_OrderCartPartial", cart);

            return PartialView("_CartPartial", cart);
        }

        [HttpPost]
        public IActionResult RemoveAllCart()
        {
            _cartService.ClearCart();
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Count()
        {
            int count = _cartService.GetCartCount();
            return Json(new { count });
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity(string id, int change)
        {
            var result = _cartService.UpdateQuantity(id, change);
            return Json(new { success = result.Success, message = result.Message, count = result.Count });
        }


        [HttpPost]
        public IActionResult RemoveFromCart(string id)
        {
            var result = _cartService.RemoveFromCart(id);
            return Json(new { success = result.Success, count = result.Count });
        }

        [HttpPost]
        public IActionResult UpdateNote(string id, string note)
        {
            var result = _cartService.UpdateNote(id, note);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult SaveAsNew(string id, string note)
        {
            var result = _cartService.DuplicateItemWithNote(id, note);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }

            // Load lại trang giỏ hàng
            return RedirectToAction("Index");
        }
    }
}
