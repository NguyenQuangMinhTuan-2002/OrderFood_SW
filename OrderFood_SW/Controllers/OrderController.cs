using Dapper;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;
using static NuGet.Packaging.PackagingConstants;

public class OrderController : Controller
{
    private readonly DatabaseHelper _db;
    private const int PageSize = 5;

    public OrderController(DatabaseHelper db)
    {
        _db = db;
    }
    public IActionResult Index()
    {
        // Truy vấn danh sách bàn ăn
        string query = "SELECT * FROM Tables ORDER BY TableNumber";

        var tableList = _db.Query<Table>(query);

        var model = new OrderPageModel
        {
            FoundTables = tableList.ToList()
        };

        return View(model);
    }


    // GET: /Order/Create
    public IActionResult Create(string searchKeyword, int page = 1, int? tableId = null)
    {
        if (tableId.HasValue)
        {
            HttpContext.Session.SetInt32("CurrentTableId", tableId.Value);
        }
        int offset = (page - 1) * PageSize;

        ViewBag.TableId = HttpContext.Session.GetInt32("CurrentTableId") ?? 0;


        string whereClause = string.IsNullOrEmpty(searchKeyword) ? "" : "WHERE DishName LIKE @kw";
        string countSql = $"SELECT COUNT(*) FROM Dishes {whereClause}";
        string querySql = $@"
            SELECT * FROM Dishes
            {whereClause}
            ORDER BY DishName
            OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

        var parameters = new
        {
            kw = $"%{searchKeyword}%",
            offset,
            limit = PageSize
        };

        int totalItems = _db.QuerySingle<int>(countSql, string.IsNullOrEmpty(searchKeyword) ? null : parameters);
        int totalPages = (int)Math.Ceiling((double)totalItems / PageSize);

        var dishes = _db.Query<Dish>(querySql, string.IsNullOrEmpty(searchKeyword) ? new { offset, limit = PageSize } : parameters);
        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

        var model = new OrderPageModel
        {
            SearchKeyword = searchKeyword,
            FoundDishes = (List<Dish>)dishes,
            CartItems = cart
        };

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(model);
    }

    // POST: /Order/SearchDish
    [HttpPost]
    public IActionResult SearchDish(string keyword)
    {
        var dishes = _db.Query<Dish>(
            "SELECT * " +
            "FROM Dishes WHERE DishName LIKE @kw OR cast(CategoryId as nvarchar) like @kw" +
            "ORDER BY DishId",
            new { kw = $"%{keyword}%" });

        return PartialView("_DishListPartial", dishes);
    }

    // POST: /Order/AddCart
    [HttpPost]
    public IActionResult AddCart(int dishId, int Quantity)
    {
        var dish = _db.QuerySingle<Dish>("SELECT * FROM Dishes WHERE DishId = @id", new { id = dishId });
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
                Quantity = Quantity
            });

        HttpContext.Session.SetObject("Cart", cart);
        return RedirectToAction("Create");
    }

    // GET: /Order/GetCart
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
        // Xoá toàn bộ giỏ hàng bằng cách remove key khỏi session
        HttpContext.Session.Remove("Cart");

        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult OrderInit(int tableId)
    {
        var cart = HttpContext.Session.GetObject<List<OrderCartItem>>("Cart") ?? new List<OrderCartItem>();

        if (cart == null || cart.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng trống!";
            return RedirectToAction("Create", new { tableId });
        }

        var order = new
        {
            TableId = tableId,
            OrderTime = DateTime.Now,
            OrderStatus = 1, // pending
            TotalAmount = cart.Sum(x => x.Price * x.Quantity),
            Note = "n/a"
        };

        string insertOrderSql = @"
        INSERT INTO Orders (TableId, OrderTime, OrderStatus, TotalAmount, Note)
        VALUES (@TableId, @OrderTime, @OrderStatus, @TotalAmount, @Note);
        SELECT CAST(SCOPE_IDENTITY() as int);";

        int newOrderId = _db.QuerySingle<int>(insertOrderSql, order);

        foreach (var item in cart)
        {
            _db.Execute("INSERT INTO OrderDetails (OrderId, DishId, Quantity) VALUES (@OrderId, @DishId, @Quantity);",
                new { OrderId = newOrderId, DishId = item.DishId, Quantity = item.Quantity });
        }

        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Detail", new { orderId = newOrderId });
    }

    public IActionResult Detail(int orderId)
    {
        // 1. Lấy đơn hàng
        var orderSql = @"SELECT * FROM Orders WHERE OrderId = @OrderId";
        var order = _db.QuerySingleOrDefault<Order>(orderSql, new { OrderId = orderId });
        if (order == null)
        {
            return NotFound("Không tìm thấy đơn hàng");
        }
        // 2. Lấy chi tiết món ăn đã đặt
        var detailsSql = @"
        SELECT od.DishId, d.DishName, od.Quantity, d.DishPrice
        FROM OrderDetails od
        INNER JOIN Dishes d ON od.DishId = d.DishId
        WHERE od.OrderId = @OrderId";
        var orderDetails = _db.Query<OrderDetailsWithDish>(detailsSql, new { OrderId = orderId }).ToList();

        // 3. Gói vào ViewModel
        var viewModel = new OrderDetailViewModel
        {
            Order = order,
            OrderDetails = orderDetails
        };

        return View(viewModel);
    }
}
