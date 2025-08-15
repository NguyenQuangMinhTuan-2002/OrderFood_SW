using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

[AuthorizeRole("Admin", "Staff")]
public class OrderController : Controller
{
    private const int PageSize = 4;

    private readonly DatabaseHelperEF _db;

    public OrderController(DatabaseHelperEF db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var tableList = _db.Tables.OrderBy(t => t.TableNumber).ToList();

        var model = new OrderPageModel
        {
            FoundTables = tableList
        };

        return View(model);
    }

    public IActionResult OrderList()
    {
        var orderList = _db.Orders.OrderBy(t => t.OrderTime).ToList();

        var model = new OrderPageModel
        {
            FoundOrders = orderList
        };

        return View(model);
    }

    public IActionResult OrderHistory()
    {
        var orderList = _db.Orders
            .OrderByDescending(t => t.OrderTime)
            .ToList();

        var model = new OrderPageModel
        {
            FoundOrders = orderList
        };

        return View(model);
    }

    //-------------------------------------------------------------------------------------------------------------
    // Trang tạo giỏ hàng Order
    public IActionResult Create(string searchKeyword, int page = 1, int? tableId = null)
    {
        if (tableId.HasValue)
        {
            HttpContext.Session.SetInt32("CurrentTableId", tableId.Value);
        }

        int offset = (page - 1) * PageSize;
        ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;

        var query = _db.Dishes.AsQueryable();

        if (!string.IsNullOrEmpty(searchKeyword))
        {
            query = query.Where(d => d.DishName.Contains(searchKeyword));
        }

        int totalItems = query.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / PageSize);

        var dishes = query.OrderBy(d => d.DishName)
                          .Skip(offset)
                          .Take(PageSize)
                          .ToList();

        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

        var model = new OrderPageModel
        {
            SearchKeyword = searchKeyword,
            FoundDishes = dishes,
            CartItems = cart
        };

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(model);
    }

    [HttpPost]
    public IActionResult AddCart(int dishId, int quantity)
    {
        var dish = _db.Dishes.Find(dishId);
        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

        var existing = cart.FirstOrDefault(x => x.DishId == dishId);
        if (existing != null)
            existing.Quantity++;
        else
            cart.Add(new OrderCartItem
            {
                DishId = dish.DishId,
                DishName = dish.DishName,
                Price = dish.DishPrice,
                Quantity = quantity
            });

        HttpContext.Session.SetObject("Cart", cart);
        return RedirectToAction("Create");
    }

    public IActionResult GetCart()
    {
        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();
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

        var existingOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
        Order order;

        if (existingOrderId.HasValue)
        {
            order = _db.Orders.FirstOrDefault(o => o.OrderId == existingOrderId.Value);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cũ!";
                return RedirectToAction("Index");
            }
        }
        else
        {
            order = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = 0,
                note = "n/a",
                UserId = 1,
            };
            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        // Thêm món mới vào đơn
        foreach (var item in cart)
        {
            var existingDetail = _db.OrderDetails
                .FirstOrDefault(od => od.OrderId == order.OrderId && od.DishId == item.DishId);

            if (existingDetail != null)
            {
                existingDetail.Quantity += item.Quantity;
            }
            else
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity,
                    DishStatus = 0
                };
                _db.OrderDetails.Add(orderDetail);
            }
        }

        // Cập nhật tổng tiền
        order.TotalAmount = _db.OrderDetails
            .Where(od => od.OrderId == order.OrderId)
            .Join(_db.Dishes,
                  od => od.DishId,
                  d => d.DishId,
                  (od, d) => od.Quantity * d.DishPrice)
            .Sum();

        // Cập nhật trạng thái bàn nếu lần đầu
        var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableId == order.TableId);
        if (table != null)
        {
            table.Status = "Occupied";
        }

        _db.SaveChanges();

        // Xoá giỏ hàng + xoá OrderId khỏi Session (để lần sau là đơn mới)
        HttpContext.Session.Remove("Cart");
        HttpContext.Session.Remove("CurrentOrderId");

        return RedirectToAction("Detail", new { orderId = order.OrderId });
    }


    //-----------------------------------------------------------------------------------------------------------------
    // Trang chi tiết đơn hàng Order detail

    [Route("Order/Detail/{orderId}")]
    public IActionResult Detail(int orderId)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
            return NotFound("Không tìm thấy đơn hàng");

        var orderDetails = (
            from od in _db.OrderDetails
            join d in _db.Dishes on od.DishId equals d.DishId
            where od.OrderId == orderId
            select new DetailsWithDish
            {
                DishId = d.DishId,
                ImageUrl = d.ImageUrl ?? "/images/nophoto.png",
                DishName = d.DishName,
                Quantity = od.Quantity,
                DishPrice = d.DishPrice,
                DishStatus = od.DishStatus,
                OrderId = od.OrderId,
            }).ToList();

        var viewModel = new OrderDetailViewModel
        {
            Order = order,
            OrderDetails = orderDetails
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDishStatus(int OrderId, int DishId, int DishStatus)
    {
        var detail = await _db.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == OrderId && od.DishId == DishId);

        if (detail != null)
        {
            detail.DishStatus = DishStatus;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Detail", new { id = OrderId });
    }

    [HttpPost]
    public async Task<IActionResult> EditDishQuantity(int OrderId, int DishId, int Quantity)
    {
        var detail = await _db.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == OrderId && od.DishId == DishId);

        if (detail != null && Quantity > 0)
        {
            detail.Quantity = Quantity;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Detail", new { id = OrderId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDishFromOrder(int OrderId, int DishId)
    {
        var detail = await _db.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == OrderId && od.DishId == DishId);

        if (detail != null)
        {
            _db.OrderDetails.Remove(detail);
            await _db.SaveChangesAsync();
        }

        // Kiểm tra còn món nào trong đơn nữa không
        var remainingItems = _db.OrderDetails
            .Where(od => od.OrderId == OrderId)
            .ToList();

        if (!remainingItems.Any())
        {
            // Xóa cả đơn hàng nếu không còn món
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == OrderId);
            if (order != null)
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }

            // Redirect về danh sách đơn
            return RedirectToAction("Index", "Order");
        }

        return RedirectToAction("Detail", new { id = OrderId });
    }

    [HttpPost]
    public IActionResult ToggleDishStatus(int orderId, int dishId)
    {
        var orderDetail = _db.OrderDetails
            .FirstOrDefault(od => od.OrderId == orderId && od.DishId == dishId);

        if (orderDetail == null)
        {
            return NotFound();
        }

        // Toggle trạng thái: 0 <-> 1
        orderDetail.DishStatus = (orderDetail.DishStatus == 0) ? 1 : 0;

        _db.SaveChanges();

        return RedirectToAction("Detail", new { orderId = orderId });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveOrder(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return NotFound();

        // Kiểm tra tất cả món đã được phục vụ
        bool allServed = order.OrderDetails.All(od => od.DishStatus == 1);
        if (!allServed)
        {
            TempData["Error"] = "Chỉ được duyệt đơn khi tất cả món đã được phục vụ.";
            return RedirectToAction("Detail", new { orderId = orderId });
        }

        // Tính tổng tiền
        decimal total = order.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice);
        order.TotalAmount = total;

        // Đổi trạng thái đơn hàng sang "2 = đã duyệt"
        order.OrderStatus = 2;

        // Cập nhật trạng thái bàn
        var table = _db.Tables.FirstOrDefault(t => t.TableId == order.TableId);
        if (table != null)
        {
            table.Status = "Available";
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đơn hàng đã được duyệt và tính tổng tiền thành công.";
        return RedirectToAction("Detail", new { orderId = orderId });
    }

    [HttpPost]
    public IActionResult CancelOrder(int orderId)
    {
        var order = _db.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return NotFound();

        // Kiểm tra nếu có món nào đã được phục vụ
        bool hasServed = order.OrderDetails.Any(d => d.DishStatus == 1);
        if (hasServed)
        {
            TempData["Error"] = "Không thể hủy đơn vì đã có món được phục vụ.";
            return RedirectToAction("Detail", new { orderId });
        }

        // Cập nhật trạng thái đơn hàng sang -1 (bị hủy)
        order.OrderStatus = -1;
        order.TotalAmount = 0;

        // Cập nhật trạng thái bàn về "Available"
        var table = _db.Tables.FirstOrDefault(t => t.TableId == order.TableId);
        if (table != null)
        {
            table.Status = "Available";
        }

        _db.SaveChanges();

        TempData["Success"] = "Đơn hàng đã được hủy (lưu trạng thái trong hệ thống).";
        return RedirectToAction("OrderList");
    }

    [HttpPost]
    public IActionResult AddDishToExistingOrder(int orderId, int tableId)
    {
        HttpContext.Session.SetInt32("CurrentTableId", tableId);
        HttpContext.Session.SetInt32("CurrentOrderId", orderId);
        return RedirectToAction("Create");
    }

}
