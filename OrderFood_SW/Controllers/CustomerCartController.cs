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
            ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId");
            return View(cart);
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
        public IActionResult UpdateCartQuantity(int dishId, int change)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            var item = cart.FirstOrDefault(x => x.DishId == dishId);

            if (item != null)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                    cart.Remove(item);

                HttpContext.Session.SetObject("Cart", cart);
            }

            return Json(new { success = true, count = cart.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int tableId)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!cart.Any())
            {
                TempData["Error"] = "Cart is empty!";
                return RedirectToAction("Index");
            }

            var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
            if (table == null)
            {
                TempData["Error"] = "Table not found!";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                note = "n/a",
                UserId = userId
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in cart)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity
                });
            }

            table.Status = "Occupied";
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Detail", "Order", new { orderId = order.OrderId });
        }
    }
}
