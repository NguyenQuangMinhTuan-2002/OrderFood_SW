using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff", "Customer")]
    public class CustomerOrderController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public CustomerOrderController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var query = _db.Tables
                .ToList();
            return View(query);
        }

        public IActionResult CreateOrder(string searchKeyword, int? tableId = null, int? categoryId = null)
        {
            if (tableId.HasValue)
            {
                HttpContext.Session.SetInt32("CurrentTableId", tableId.Value);
                ViewBag.TableId = tableId;
            }

            ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;

            var query = _db.Dishes.AsQueryable();
            var queryCategories = _db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(d => d.DishName.Contains(searchKeyword));
            }

            // 👇 Chỉ lọc nếu categoryId khác 0 và khác null
            if (categoryId.HasValue && categoryId.Value != 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var dishes = query.OrderBy(d => d.CategoryId).ToList();
            var categories = queryCategories.OrderBy(c => c.CategoryName).ToList();
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            var model = new OrderPageModel
            {
                SearchKeyword = searchKeyword,
                FoundDishes = dishes,
                DishCategories = categories,
                CartItems = cart,
                SelectedCategoryId = categoryId ?? 0
            };

            return View(model);
        }

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


        [HttpPost]
        public async Task<IActionResult> OrderInitAsync(int tableId)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            var UserId = HttpContext.Session.GetInt32("UserId");

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("CreateOrder", new { tableId });
            }

            var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
            if (table == null)
            {
                TempData["Error"] = "Bàn không tồn tại!";
                return RedirectToAction("CreateOrder", new { tableId = 0 });
            }

            var order = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                note = "n/a",
                UserId = UserId
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

            return RedirectToAction("OrderProcessing", "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> ReOrder(int orderId)
        {
            // Lấy order cũ
            var oldOrder = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (oldOrder == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cũ!";
                return RedirectToAction("OrderHistory");
            }

            // Lấy giỏ hàng hiện tại từ session
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart")
                       ?? new List<OrderCartItem>();

            // Thêm/cộng dồn món từ order cũ vào giỏ hàng
            foreach (var item in oldOrder.OrderDetails)
            {
                var dish = await _db.Dishes.FirstOrDefaultAsync(d => d.DishId == item.DishId);
                if (dish != null)
                {
                    var existingItem = cart.FirstOrDefault(c => c.DishId == dish.DishId);
                    if (existingItem != null)
                    {
                        // Nếu món đã có thì cộng thêm số lượng
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        // Nếu món chưa có thì thêm mới
                        cart.Add(new OrderCartItem
                        {
                            DishId = dish.DishId,
                            DishName = dish.DishName,
                            ImageUrl = dish.ImageUrl,
                            Price = dish.DishPrice,
                            Quantity = item.Quantity
                        });
                    }
                }
            }

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.SetObject("Cart", cart);

            TempData["Success"] = $"Đã thêm {oldOrder.OrderDetails.Count} món từ đơn #{oldOrder.OrderId} vào giỏ hàng!";
            return RedirectToAction("Index", "CustomerCart");
        }
    }
}
