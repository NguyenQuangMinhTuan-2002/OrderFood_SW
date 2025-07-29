using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;

public class OrderController : Controller
{
    private const int PageSize = 5;

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
    public IActionResult OrderInit(int tableId)
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

        _db.SaveChanges();
        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Detail", new { orderId = order.OrderId });
    }

    public IActionResult Detail(int orderId)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
            return NotFound("Không tìm thấy đơn hàng");

        var orderDetails = _db.OrderDetails
            .Include(od => od.Dish)
            .Where(od => od.OrderId == orderId)
            .Select(od => new OrderDetailsWithDish
            {
                DishId = od.DishId,
                DishName = od.Dish.DishName,
                DishPrice = od.Dish.DishPrice,
                Quantity = od.Quantity
            })
            .ToList();

        var viewModel = new OrderDetailViewModel
        {
            Order = order,
            OrderDetails = orderDetails
        };

        return View(viewModel);
    }
}
