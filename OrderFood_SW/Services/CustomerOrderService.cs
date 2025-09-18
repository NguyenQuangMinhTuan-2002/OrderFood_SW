using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Services
{
    public class CustomerOrderService
    {
        private readonly CustomerOrderRepository _repo;
        public CustomerOrderService(CustomerOrderRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Table>> GetAllTablesAsync() => _repo.GetAllTablesAsync();
        public Task<Table?> GetTableByIdAsync(int tableId) => _repo.GetTableByIdAsync(tableId);
        public Task<Order?> GetOrderWithDetailsAsync(int orderId) => _repo.GetOrderWithDetailsAsync(orderId);
        public Task<Dish?> GetDishByIdAsync(int dishId) => _repo.GetDishByIdAsync(dishId);
        public Task<List<Dish>> GetDishesAsync(int? categoryId) => _repo.GetDishesAsync(categoryId);
        public Task<List<Category>> GetCategoriesAsync() => _repo.GetCategoriesAsync();
        public Task<bool> OrderHasDishAsync(int orderId, int dishId) => _repo.OrderHasDishAsync(orderId, dishId);

        /// <summary>
        /// Add cart items into an existing order. Returns list of DishIds that were already present (not added).
        /// Behavior intentionally mirrors original controller logic: it checks existing items in order.OrderDetails and only adds new ones.
        /// Totals are updated similarly to original implementation.
        /// </summary>
        public async Task<List<int>> AddCartItemsToExistingOrderAsync(Order order, List<OrderCartItem> cart)
        {
            var existedDishIds = new List<int>();

            foreach (var item in cart)
            {
                var existingDetail = order.OrderDetails.FirstOrDefault(od => od.DishId == item.DishId);
                if (existingDetail != null)
                {
                    existedDishIds.Add(item.DishId);
                    continue;
                }

                _repo.AddOrderDetail(new OrderDetail
                {
                    OrderId = order.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity
                });
            }

            // Mirror original: add only sum of those not present in order.OrderDetails (original used the tracked navigation collection)
            order.TotalAmount += cart
                .Where(x => !order.OrderDetails.Any(od => od.DishId == x.DishId))
                .Sum(x => x.Price * x.Quantity);

            await _repo.SaveChangesAsync();
            return existedDishIds;
        }

        /// <summary>
        /// Create a new order from cart and update table status/currentOrderId.
        /// Throws InvalidOperationException if table not found.
        /// </summary>
        public async Task<Order> CreateNewOrderFromCartAsync(int tableId, List<OrderCartItem> cart, int userId)
        {
            var table = await _repo.GetTableByIdAsync(tableId);
            if (table == null)
                throw new InvalidOperationException("table is unavailable!");

            var newOrder = new Order
            {
                TableId = tableId,
                OrderTime = DateTime.Now,
                OrderStatus = 1,
                TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                note = "n/a",
                UserId = userId
            };

            _repo.AddOrder(newOrder);
            await _repo.SaveChangesAsync(); // get newOrder.OrderId

            foreach (var item in cart)
            {
                _repo.AddOrderDetail(new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity,
                    Note = item.Note ?? "n/a"
                });
            }

            table.Status = "Occupied";
            table.CurrentOrderId = newOrder.OrderId;
            _repo.UpdateTable(table);

            await _repo.SaveChangesAsync();

            return newOrder;
        }

        /// <summary>
        /// Copy items from an old order into provided cart (in-memory). Does not persist anything.
        /// </summary>
        public async Task ReOrderToCartAsync(Order oldOrder, List<OrderCartItem> cart)
        {
            foreach (var item in oldOrder.OrderDetails)
            {
                var dish = await _repo.GetDishByIdAsync(item.DishId);
                if (dish == null) continue;

                var existingItem = cart.FirstOrDefault(c => c.DishId == dish.DishId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    cart.Add(new OrderCartItem
                    {
                        DishId = dish.DishId,
                        DishName = dish.DishName,
                        ImageUrl = dish.ImageUrl ?? "nophoto.png",
                        Price = dish.DishPrice,
                        Quantity = item.Quantity,
                        Note = item.Note ?? "n/a"
                    });
                }
            }
        }
    }
}