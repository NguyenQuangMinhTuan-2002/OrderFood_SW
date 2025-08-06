using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
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
            var queryCategories = _db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(d => d.DishName.Contains(searchKeyword));
            }


            var dishes = query.OrderBy(d => d.DishName).ToList();
            var categories = queryCategories.OrderBy(c => c.CategoryName).ToList();

            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            var model = new OrderPageModel
            {
                SearchKeyword = searchKeyword,
                FoundDishes = dishes,
                DishCategories = categories,
                CartItems = cart
            };

            return View(model);
        }

        // Customer cart management **** **** **** ****
        [HttpPost]
        public IActionResult AddCart(int dishId, int Quantity)
        {
            try
            {
                var dish = _db.Dishes.Find(dishId);
                if (dish == null)
                {
                    return Json(new { success = false, message = "Dish not found!" });
                }

                var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
                var existing = cart.FirstOrDefault(x => x.DishId == dishId);

                if (existing != null)
                    existing.Quantity += Quantity;
                else
                    cart.Add(new OrderCartItem
                    {
                        DishId = dish.DishId,
                        ImageUrl = dish.ImageUrl,
                        DishName = dish.DishName,
                        Price = dish.DishPrice,
                        Quantity = Quantity
                    });

                HttpContext.Session.SetObject("Cart", cart);

                // Check if it's an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Added {dish.DishName} to cart!",
                        cartCount = cart.Sum(x => x.Quantity)
                    });
                }

                // Fallback for non-AJAX requests
                return RedirectToAction("CreateOrder");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error adding to cart: " + ex.Message });
                }

                TempData["Error"] = "Error adding to cart: " + ex.Message;
                return RedirectToAction("CreateOrder");
            }
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
        public async Task<IActionResult> OrderInitAsync(int tableId)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Create", new { tableId });
            }

            var order = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                note = "n/a"
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity
                };

                _db.OrderDetails.Add(orderDetail);
            }

            var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableId == order.TableId);
            if (table != null)
            {
                table.Status = "Occupied";
            }


            _db.SaveChanges();
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Detail", new { orderId = order.OrderId });
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
