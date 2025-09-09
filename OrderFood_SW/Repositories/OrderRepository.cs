using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using static NuGet.Packaging.PackagingConstants;

namespace OrderFood_SW.Repositories
{
    public class OrderRepository
    {
        private readonly DatabaseHelperEF _db;

        public OrderRepository(DatabaseHelperEF db)
        {
            _db = db;
        }

        public List<Table> GetAllTables()
        {
            return _db.Tables
                      .OrderBy(t => t.TableNumber)
                      .ToList();
        }

        public List<Order> GetAllOrders()
        {
            return _db.Orders
                      .OrderBy(o => o.OrderTime)
                      .ToList();
        }

        public int CountOrders()
        {
            return _db.Orders.Count();
        }

        public List<Order> GetPagedOrders(int page, int pageSize)
        {
            return _db.Orders
                      .OrderByDescending(o => o.OrderTime)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();
        }

        public List<Dish> GetPagedDishes(string keyword, int page, int pageSize, out int totalItems)
        {
            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.DishName.Contains(keyword));
            }

            totalItems = query.Count();

            return query.OrderBy(d => d.DishName)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }

        public Dish? GetDishById(int dishId)
        {
            return _db.Dishes.FirstOrDefault(d => d.DishId == dishId);
        }

        public Order? GetOrderById(int id) =>
        _db.Orders.FirstOrDefault(o => o.OrderId == id);

        public async Task<Order?> GetByIdAsync(int id) =>
            await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

        public void Add(Order order) => _db.Orders.Add(order);

        public IQueryable<OrderDetail> GetOrderDetails(int orderId) =>
            _db.OrderDetails.Where(od => od.OrderId == orderId);

        public void AddOrderDetail(OrderDetail detail) =>
            _db.OrderDetails.Add(detail);

        public OrderDetail? GetOrderDetail(int orderId, int dishId) =>
            _db.OrderDetails.FirstOrDefault(od => od.OrderId == orderId && od.DishId == dishId);

        public decimal CalculateTotalAmount(int orderId)
        {
            return _db.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Join(_db.Dishes,
                      od => od.DishId,
                      d => d.DishId,
                      (od, d) => od.Quantity * d.DishPrice)
                .Sum();
        }

        public List<DetailsWithDish> GetOrderDetailsWithDish(int orderId)
        {
            return (
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
                    OrderId = od.OrderId
                }).ToList();
        }
        public async Task UpdateDishStatusAsync(int orderId, int dishId, int dishStatus)
        {
            var detail = await _db.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.DishId == dishId);

            if (detail != null)
            {
                detail.DishStatus = dishStatus;
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateDishQuantityAsync(int orderId, int dishId, int quantity)
        {
            var detail = await _db.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.DishId == dishId);

            if (detail != null && quantity > 0)
            {
                detail.Quantity = quantity;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteDishFromOrderAsync(int orderId, int dishId)
        {
            var detail = await _db.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.DishId == dishId);

            if (detail == null) return false;

            _db.OrderDetails.Remove(detail);
            await _db.SaveChangesAsync();

            // Kiểm tra còn món nào trong đơn
            bool hasRemaining = await _db.OrderDetails.AnyAsync(od => od.OrderId == orderId);

            if (!hasRemaining)
            {
                var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order != null)
                {
                    _db.Orders.Remove(order);
                    await _db.SaveChangesAsync();
                    return true; // đơn hàng bị xóa
                }
            }

            return false; // chỉ xóa 1 món
        }

        public async Task<bool> ToggleDishStatusAsync(int orderId, int dishId)
        {
            var orderDetail = await _db.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.DishId == dishId);

            if (orderDetail == null) return false;

            // Toggle trạng thái 0 ↔ 1
            orderDetail.DishStatus = (orderDetail.DishStatus == 0) ? 1 : 0;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Table?> GetTableByIdAsync(int tableId)
        {
            return await _db.Tables.FirstOrDefaultAsync(t => t.TableId == tableId);
        }

        public Order? GetOrderWithDetails(int orderId)
        {
            return _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);
        }

        public Table? GetTableById(int tableId)
        {
            return _db.Tables.FirstOrDefault(t => t.TableId == tableId);
        }

        public void SaveChanges() => _db.SaveChanges();

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
