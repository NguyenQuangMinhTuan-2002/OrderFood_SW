using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Services;
using OrderFood_SW.ViewModels;

[AuthorizeRole("Staff")]
public class OrderController : Controller
{
    private const int PageSize = 4;
    private readonly OrderService _service;

    public OrderController(OrderService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        var model = new OrderPageModel
        {
            FoundTables = _service.GetAllTables()
        };

        return View(model);
    }

    public IActionResult OrderList()
    {
        var model = new OrderPageModel
        {
            FoundOrders = _service.GetAllOrders()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult GetPendingOrdersCount()
    {
        var count = _service.GetPendingOrdersCount();
        return Json(new { count = count });
    }

    [HttpGet]
    public IActionResult DebugOrders()
    {
        var allOrders = _service.GetAllOrders();
        var debugInfo = allOrders.Select(o => new { 
            OrderId = o.OrderId, 
            OrderStatus = o.OrderStatus, 
            OrderTime = o.OrderTime.ToString("yyyy-MM-dd HH:mm:ss"),
            TableId = o.TableId
        }).ToList();
        
        return Json(new { 
            totalOrders = allOrders.Count,
            pendingOrders = allOrders.Count(o => o.OrderStatus == 1),
            orders = debugInfo
        });
    }

    public IActionResult OrderHistory(int page = 1, int pageSize = 20)
    {
        var (orders, totalPages) = _service.GetPagedOrders(page, pageSize);

        var model = new OrderPageModel
        {
            FoundOrders = orders,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(model);
    }


    //-------------------------------------------------------------------------------------------------------------
    // Trang tạo giỏ hàng Order
    public IActionResult Create(string searchKeyword, int page = 1, int? tableId = null)
    {
        if (tableId.HasValue)
            HttpContext.Session.SetInt32("CurrentTableId", tableId.Value);

        int tableIdFromSession = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;
        ViewBag.TableId = tableIdFromSession;

        var (dishes, totalPages) = _service.GetPagedDishes(searchKeyword, page, PageSize);

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
        var dish = _service.GetById(dishId);
        if (dish == null) return NotFound();

        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

        var existing = cart.FirstOrDefault(x => x.DishId == dishId);
        if (existing != null)
            existing.Quantity += quantity;
        else
            cart.Add(new OrderCartItem
            {
                DishId = dish.DishId,
                DishName = dish.DishName,
                Price = dish.DishPrice,
                Quantity = quantity,
                TaxRate = dish.TaxRate ?? 0.1m,
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

        try
        {
            var order = await _service.InitOrderAsync(tableId, cart, existingOrderId);

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CurrentOrderId");

            return RedirectToAction("Detail", new { orderId = order.OrderId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    //-----------------------------------------------------------------------------------------------------------------
    // Trang chi tiết đơn hàng Order detail

    [Route("Order/Detail/{orderId}")]
    public IActionResult Detail(int orderId)
    {
        try
        {
            var viewModel = _service.GetOrderDetailViewModel(orderId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDishStatus(int orderId, int dishId, int dishStatus)
    {
        await _service.UpdateDishStatusAsync(orderId, dishId, dishStatus);
        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> EditDishQuantity(int orderId, int dishId, int quantity)
    {
        await _service.UpdateDishQuantityAsync(orderId, dishId, quantity);
        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDishFromOrder(int orderId, int OrderDetailId)
    {
        bool orderDeleted = await _service.DeleteDishFromOrderAsync(orderId, OrderDetailId);

        if (orderDeleted)
            return RedirectToAction("Index"); // đơn hàng bị xóa toàn bộ

        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleDishStatus(int orderId, int OrderDetailId)
    {
        var success = await _service.ToggleDishStatusAsync(orderId, OrderDetailId);

        if (!success)
            return NotFound();

        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveOrder(int orderId)
    {
        var result = await _service.ApproveOrderAsync(orderId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Detail", new { orderId });
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Detail", new { orderId });
    }


    [HttpPost]
    public IActionResult CancelOrder(int orderId)
    {
        var result = _service.CancelOrder(orderId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Detail", new { orderId });
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("OrderList");
    }

    [HttpPost]
    public IActionResult AddDishToExistingOrder(int orderId, int tableId)
    {
        HttpContext.Session.SetInt32("CurrentTableId", tableId);
        HttpContext.Session.SetInt32("CurrentOrderId", orderId);
        return RedirectToAction("Create");
    }

    // --- Notes & Duplicate APIs ---
    [HttpPost]
    public async Task<IActionResult> UpdateOrderNote(int orderId, string note)
    {
        var result = await _service.UpdateOrderNoteAsync(orderId, note);
        if (!result.Success)
            TempData["Error"] = result.Message;
        else
            TempData["Success"] = result.Message;

        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderDetailNote(int orderId, int orderDetailId, string note, bool saveAsNew = false)
    {
        (bool Success, string Message) result;
        if (saveAsNew)
            result = await _service.DuplicateOrderDetailWithNoteAsync(orderDetailId, note);
        else
            result = await _service.UpdateOrderDetailNoteAsync(orderDetailId, note);

        if (!result.Success)
            TempData["Error"] = result.Message;
        else
            TempData["Success"] = result.Message;

        return RedirectToAction("Detail", new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> SaveAsNewDetail(int orderId, int orderDetailId, string note)
    {
        var result = await _service.DuplicateOrderDetailWithNoteAsync(orderDetailId, note);
        if (!result.Success)
            TempData["Error"] = result.Message;
        else
            TempData["Success"] = result.Message;

        return RedirectToAction("Detail", new { orderId });
    }
}
