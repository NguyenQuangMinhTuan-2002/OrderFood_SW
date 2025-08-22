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
        
        public IActionResult CreateOrder(int? tableId = null, int? categoryId = null)
        {
            if (tableId.HasValue)
            {
                var table = _db.Tables.FirstOrDefault(t => t.TableId == tableId.Value);
                if (table != null)
                {
                    // 🚨 Nếu bàn không khả dụng thì chặn
                    if (table.Status != "Available")
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }

                    // Nếu bàn Available thì reset session
                    HttpContext.Session.Remove("CurrentOrderId");
                    HttpContext.Session.Remove("Cart");

                    HttpContext.Session.SetInt32("CurrentTableId", table.TableId);
                    ViewBag.TableId = table.TableId;
                }
            }

            // Nếu session CurrentOrderId đang trỏ tới order cũ mà đã closed thì reset
            var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (currentOrderId.HasValue && currentOrderId.Value > 0)
            {
                var order = _db.Orders.FirstOrDefault(o => o.OrderId == currentOrderId.Value);
                if (order == null || order.OrderStatus == 2 || order.OrderStatus == -1)
                {
                    HttpContext.Session.Remove("CurrentOrderId");
                    HttpContext.Session.Remove("Cart");
                }
            }

            ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;
            ViewBag.CurrentOrderId = HttpContext.Session.GetInt32("CurrentOrderId") ?? 0;

            var query = _db.Dishes.AsQueryable();
            var queryCategories = _db.Categories.AsQueryable();

            if (categoryId.HasValue && categoryId.Value != 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var dishes = query.OrderBy(d => d.CategoryId).ToList();
            var categories = queryCategories.OrderBy(c => c.CategoryName).ToList();
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            var model = new OrderPageModel
            {
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
                var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");

                // 1) Đang thêm vào đơn hàng cũ -> chặn nếu đã có trong OrderDetails
                if (currentOrderId is int orderId && orderId > 0)
                {
                    bool existsInOrder = _db.OrderDetails
                        .Any(od => od.OrderId == orderId && od.DishId == dishId);

                    if (existsInOrder)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Món này đã có trong đơn hàng hiện tại. Hãy yêu cầu nhân viên chỉnh số lượng.",
                            cartCount = GetCartCount()
                        });
                    }
                }

                // 2) Kiểm tra món
                var dish = _db.Dishes.Find(dishId);
                if (dish == null)
                {
                    return Json(new { success = false, message = "Dish not found!", cartCount = GetCartCount() });
                }

                // 3) Cart trong session
                var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

                // HARD BLOCK: Không cho add trùng trong cart
                bool existsInCart = cart.Any(x => x.DishId == dishId);
                if (existsInCart)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Món này đã có trong giỏ. Hãy chỉnh số lượng ở giỏ hàng.",
                        cartCount = cart.Sum(x => x.Quantity)
                    });
                }

                // 4) Thêm mới vào cart (chỉ khi chưa có)
                cart.Add(new OrderCartItem
                {
                    DishId = dish.DishId,
                    ImageUrl = dish.ImageUrl,
                    DishName = dish.DishName,
                    Price = dish.DishPrice,
                    Quantity = Quantity
                });

                HttpContext.Session.SetObject("Cart", cart);

                // AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Added {dish.DishName} to cart!",
                        cartCount = cart.Sum(x => x.Quantity)
                    });
                }

                // Non-AJAX
                return RedirectToAction("CreateOrder");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error adding to cart: " + ex.Message, cartCount = GetCartCount() });
                }

                TempData["Error"] = "Error adding to cart: " + ex.Message;
                return RedirectToAction("CreateOrder");
            }
        }

        private int GetCartCount()
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            return cart.Sum(x => x.Quantity);
        }

        [HttpPost]
        public async Task<IActionResult> OrderInitAsync(int tableId)
        {
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
            var userId = HttpContext.Session.GetInt32("UserId");
            var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("CreateOrder", new { tableId });
            }

            // 🔹 Nếu đang thêm vào đơn cũ
            if (currentOrderId.HasValue && currentOrderId.Value > 0)
            {
                var order = await _db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == currentOrderId.Value && o.OrderStatus == 1);

                if (order == null)
                {
                    HttpContext.Session.Remove("CurrentOrderId");
                    HttpContext.Session.Remove("Cart");

                    TempData["Error"] = "Đơn hàng không khả dụng để thêm!";
                    return RedirectToAction("CreateOrder", new { tableId });
                }

                foreach (var item in cart)
                {
                    // 🔹 Kiểm tra nếu món đã tồn tại trong OrderDetail thì bỏ qua
                    var existingDetail = order.OrderDetails
                        .FirstOrDefault(od => od.DishId == item.DishId);

                    if (existingDetail != null)
                    {
                        // 👉 Có thể chọn: bỏ qua hoặc báo cho user biết
                        TempData["Warning"] = $"Món {item.DishId} đã có trong đơn hàng, không thể thêm trùng!";
                        continue;
                    }

                    _db.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        DishId = item.DishId,
                        Quantity = item.Quantity
                    });
                }

                order.TotalAmount += cart
                    .Where(x => !order.OrderDetails.Any(od => od.DishId == x.DishId))
                    .Sum(x => x.Price * x.Quantity);

                await _db.SaveChangesAsync();

                HttpContext.Session.Remove("Cart");
                TempData["Success"] = "Đã thêm món mới vào đơn hàng hiện tại!";
                return RedirectToAction("OrderProcessing", "Customer");
            }

            // 🔹 Nếu là đơn mới
            var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
            if (table == null)
            {
                TempData["Error"] = "Bàn không tồn tại!";
                return RedirectToAction("CreateOrder", new { tableId = 0 });
            }

            var newOrder = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                note = "n/a",
                UserId = userId
            };

            _db.Orders.Add(newOrder);
            await _db.SaveChangesAsync();

            foreach (var item in cart)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity
                });
            }

            table.Status = "Occupied";
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Đơn hàng mới đã được tạo!";
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

        [HttpGet]
        public IActionResult AddMoreOrder(int orderId, int tableId)
        {
            // Lưu lại OrderId vào session để biết đang thêm vào đơn nào
            HttpContext.Session.SetInt32("CurrentOrderId", orderId);
            HttpContext.Session.SetInt32("CurrentTableId", tableId);

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("CreateOrder", new { tableId });
        }

    }
}
