using Microsoft.IdentityModel.Tokens;
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
        /// Allows same dish with different notes to be added as separate items.
        /// </summary>
        public async Task<List<int>> AddCartItemsToExistingOrderAsync(Order order, List<OrderCartItem> cart)
        {
            var existedDishIds = new List<int>();

            foreach (var item in cart)
            {
                // Check if exact same dish with same note already exists
                var existingDetail = order.OrderDetails.FirstOrDefault(od => 
                    od.DishId == item.DishId && 
                    od.Note == (string.IsNullOrWhiteSpace(item.Note) ? "n/a" : item.Note));
                
                if (existingDetail != null)
                {
                    // Same dish with same note - just increase quantity
                    existingDetail.Quantity += item.Quantity;
                    existedDishIds.Add(item.DishId);
                    continue;
                }

                // Always fetch Dish to get correct TaxRate and DishPrice
                var dish = await GetDishByIdAsync(item.DishId);
                decimal taxRate = dish?.TaxRate ?? 0.1m;
                decimal price = dish?.DishPrice ?? item.Price;

                // Different note or new dish - add as new detail
                _repo.AddOrderDetail(new OrderDetail
                {
                    OrderId = order.OrderId,
                    DishId = item.DishId,
                    Quantity = item.Quantity,
                    Note = string.IsNullOrWhiteSpace(item.Note) ? "n/a" : item.Note,
                    TaxRate = taxRate,
                    TaxAmount = price * item.Quantity * taxRate
                });
            }

            // Update total amount for all new items (including duplicates with different notes)
            order.TotalAmount += cart
                .Where(x => !existedDishIds.Contains(x.DishId))
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
                    Note = string.IsNullOrWhiteSpace(item.Note) ? "n/a" : item.Note,
                    TaxRate = item.TaxRate,
                    TaxAmount = item.Price * item.Quantity * item.TaxRate
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
        /// Handles same dish with different notes as separate cart items.
        /// </summary>
        public async Task ReOrderToCartAsync(Order oldOrder, List<OrderCartItem> cart)
        {
            foreach (var item in oldOrder.OrderDetails)
            {
                var dish = await _repo.GetDishByIdAsync(item.DishId);
                if (dish == null) continue;

                // Check for exact match (same dish AND same note)
                var existingItem = cart.FirstOrDefault(c => 
                    c.DishId == dish.DishId && 
                    c.Note == (item.Note ?? "n/a"));
                
                if (existingItem != null)
                {
                    // Same dish with same note - increase quantity
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    // Different note or new dish - add as new cart item
                    cart.Add(new OrderCartItem
                    {
                        CartItemId = Guid.NewGuid().ToString(), // Generate new ID for cart item
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