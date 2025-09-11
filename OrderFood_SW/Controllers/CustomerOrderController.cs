using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff", "Customer")]
    public class CustomerOrderController : Controller
    {
        private readonly CustomerOrderService _service;

        public CustomerOrderController(CustomerOrderService service)
        {
            _service = service;
        }

        // Index -> show tables
        public async Task<IActionResult> Index()
        {
            var query = await _service.GetAllTablesAsync();
            return View(query);
        }

        // CreateOrder -> read-only page composition
        public async Task<IActionResult> CreateOrder(int? tableId = null, int? categoryId = null)
        {
            if (tableId.HasValue)
            {
                var table = await _service.GetTableByIdAsync(tableId.Value);
                if (table != null)
                {
                    var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
                    var role = HttpContext.Session.GetString("Role");

                    if (table.Status != "Available" && !currentOrderId.HasValue)
                    {
                        if (role == "Customer" && HttpContext.Session.GetInt32("UserId") == 1)
                        {
                            if (table.CurrentOrderId.HasValue)
                            {
                                return RedirectToAction("OrderDetails", "Customer", new { orderId = table.CurrentOrderId.Value });
                            }
                        }

                        return RedirectToAction("AccessDenied", "Account");
                    }

                    if (table.Status == "Available")
                    {
                        HttpContext.Session.Remove("CurrentOrderId");
                        HttpContext.Session.Remove("Cart");
                    }

                    HttpContext.Session.SetInt32("CurrentTableId", table.TableId);
                    ViewBag.TableId = table.TableId;
                }
            }

            // Nếu session CurrentOrderId đang trỏ tới order cũ mà đã closed thì reset
            var currentOrder = HttpContext.Session.GetInt32("CurrentOrderId");
            if (currentOrder.HasValue && currentOrder.Value > 0)
            {
                var order = await _service.GetOrderWithDetailsAsync(currentOrder.Value);
                if (order == null || order.OrderStatus == 2 || order.OrderStatus == -1)
                {
                    HttpContext.Session.Remove("CurrentOrderId");
                    HttpContext.Session.Remove("Cart");
                }
            }

            ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;
            ViewBag.CurrentOrderId = HttpContext.Session.GetInt32("CurrentOrderId") ?? 0;

            var dishes = await _service.GetDishesAsync(categoryId);
            var categories = await _service.GetCategoriesAsync();
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
        public async Task<IActionResult> AddCart(int dishId, int Quantity)
        {
            try
            {
                var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");

                // 1) Đang thêm vào đơn hàng cũ -> chặn nếu đã có trong OrderDetails
                if (currentOrderId is int orderId && orderId > 0)
                {
                    bool existsInOrder = await _service.OrderHasDishAsync(orderId, dishId);

                    if (existsInOrder)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "This dish is already in current order, call waiter to edit quantity.",
                            cartCount = GetCartCount()
                        });
                    }
                }

                // 2) Kiểm tra món
                var dish = await _service.GetDishByIdAsync(dishId);
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
                        message = "this dish is already in cart, go to cart to edit quantity.",
                        cartCount = cart.Sum(x => x.Quantity)
                    });
                }

                // 4) Thêm mới vào cart (chỉ khi chưa có)
                cart.Add(new OrderCartItem
                {
                    DishId = dish.DishId,
                    ImageUrl = dish.ImageUrl ?? "nophoto.png",
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
            var userId = HttpContext.Session.GetInt32("UserId") ?? 1; // nếu null thì coi như Guest (id = 1)
            var currentOrderId = HttpContext.Session.GetInt32("CurrentOrderId");

            if (!cart.Any())
            {
                TempData["Error"] = "Empty cart!";
                return RedirectToAction("CreateOrder", new { tableId });
            }

            // 🔹 Nếu đang thêm vào đơn cũ
            if (currentOrderId.HasValue && currentOrderId.Value > 0)
            {
                var order = await _service.GetOrderWithDetailsAsync(currentOrderId.Value);

                // original logic required order.OrderStatus == 1 when fetching; mimic validation here
                if (order == null || order.OrderStatus != 1)
                {
                    HttpContext.Session.Remove("CurrentOrderId");
                    HttpContext.Session.Remove("Cart");

                    TempData["Error"] = "This order is not available to add!";
                    return RedirectToAction("CreateOrder", new { tableId });
                }

                var existedDishIds = await _service.AddCartItemsToExistingOrderAsync(order, cart);

                HttpContext.Session.Remove("Cart");

                // 🔹 Nếu user là Guest (id = 1) → redirect thẳng vào OrderDetails
                if (userId == 1)
                {
                    return RedirectToAction("OrderDetails", "Customer", new { orderId = order.OrderId });
                }

                if (existedDishIds.Any())
                {
                    // Preserve original behavior of warning per existing dish (keeps last message but lists IDs)
                    TempData["Warning"] = string.Join("; ", existedDishIds.Select(id => $"{id} already in your order, can't add the same dish!"));
                }

                TempData["Success"] = "added new dish to your order!";
                return RedirectToAction("OrderProcessing", "Customer");
            }

            // 🔹 Nếu là đơn mới
            Order newOrder;
            try
            {
                newOrder = await _service.CreateNewOrderFromCartAsync(tableId, cart, userId);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "table is unavailable!";
                return RedirectToAction("CreateOrder", new { tableId = 0 });
            }

            HttpContext.Session.Remove("Cart");

            // 🔹 Nếu user là Guest (id = 1) → redirect thẳng vào OrderDetails
            if (userId == 1)
            {
                HttpContext.Session.SetInt32("GuessOrderId", newOrder.OrderId);
                return RedirectToAction("OrderDetails", "Customer", new { orderId = newOrder.OrderId });
            }

            TempData["Success"] = "new order has been added!";
            return RedirectToAction("OrderProcessing", "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> ReOrder(int orderId)
        {
            // Lấy order cũ
            var oldOrder = await _service.GetOrderWithDetailsAsync(orderId);

            if (oldOrder == null)
            {
                TempData["Error"] = "can't find your current order";
                return RedirectToAction("OrderHistory");
            }

            // Lấy giỏ hàng hiện tại từ session
            var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

            // Thêm/cộng dồn món từ order cũ vào giỏ hàng (in-memory)
            await _service.ReOrderToCartAsync(oldOrder, cart);

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.SetObject("Cart", cart);

            TempData["Success"] = $"added {oldOrder.OrderDetails.Count} from old order #{oldOrder.OrderId} to cart!";
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