using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repo;
        private readonly TableRepository _tableRepo;

        public OrderService(OrderRepository repo, TableRepository tableRepo)
        {
            _repo = repo;
            _tableRepo = tableRepo;
        }

        public List<Table> GetAllTables()
        {
            return _repo.GetAllTables();
        }

        public List<Order> GetAllOrders()
        {
            return _repo.GetAllOrders();
        }

        public int GetPendingOrdersCount()
        {
            return _repo.CountPendingOrders();
        }

        public (List<Order> Orders, int TotalPages) GetPagedOrders(int page, int pageSize)
        {
            int totalOrders = _repo.CountOrders();
            int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = _repo.GetPagedOrders(page, pageSize);
            return (orders, totalPages);
        }

        public (List<Dish> Dishes, int TotalPages) GetPagedDishes(string keyword, int page, int pageSize)
        {
            var dishes = _repo.GetPagedDishes(keyword, page, pageSize, out int totalItems);
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            return (dishes, totalPages);
        }

        public Dish? GetById(int dishId)
        {
            return _repo.GetDishById(dishId);
        }

        public async Task<Order> InitOrderAsync(int tableId, List<OrderCartItem> cart, int? existingOrderId)
        {
            Order order;

            if (existingOrderId.HasValue)
            {
                order = _repo.GetOrderById(existingOrderId.Value)
                    ?? throw new Exception("Old order not found!");
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
                _repo.Add(order);
                _repo.SaveChanges();
            }

            // Add dishes
            foreach (var item in cart)
            {
                var existingDetail = _repo.GetOrderDetail(order.OrderId, item.DishId);
                var dish = _repo.GetDishById(item.DishId);

                if (existingDetail != null)
                    existingDetail.Quantity += item.Quantity;
                else
                    _repo.AddOrderDetail(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        DishId = item.DishId,
                        Quantity = item.Quantity,
                        DishStatus = 0,
                        Note = "n/a",
                        TaxRate = dish?.TaxRate ?? 0,
                        TaxAmount = item.Quantity * item.Price * (dish?.TaxRate ?? 0),
                    });
            }

            // Update total amount
            order.TotalAmount = _repo.CalculateTotalAmount(order.OrderId);

            // Update table
            var table = await _tableRepo.GetByIdAsync(order.TableId);
            if (table != null)
            {
                table.Status = "Occupied";
                table.CurrentOrderId = order.OrderId;
            }

            _repo.SaveChanges();
            return order;
        }

        public OrderDetailViewModel GetOrderDetailViewModel(int orderId)
        {
            var order = _repo.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Order not found");

            var orderDetails = _repo.GetOrderDetailsWithDish(orderId);

            return new OrderDetailViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            };
        }

        public async Task UpdateDishStatusAsync(int orderId, int dishId, int dishStatus)
        {
            await _repo.UpdateDishStatusAsync(orderId, dishId, dishStatus);
        }

        public async Task UpdateDishQuantityAsync(int orderId, int dishId, int quantity)
        {
            await _repo.UpdateDishQuantityAsync(orderId, dishId, quantity);
        }

        public async Task<bool> DeleteDishFromOrderAsync(int orderId, int Id)
        {
            return await _repo.DeleteDishFromOrderAsync(orderId, Id);
        }

        public async Task<bool> ToggleDishStatusAsync(int orderId, int Id)
        {
            return await _repo.ToggleDishStatusAsync(orderId, Id);
        }

        public async Task<(bool Success, string Message)> ApproveOrderAsync(int orderId)
        {
            var order = await _repo.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                return (false, "Order not found");

            // Check if all dishes have been served
            bool allServed = order.OrderDetails.All(od => od.DishStatus == 1);
            if (!allServed)
                return (false, "Order can only be approved when all dishes have been served.");

            // Calculate total amount
            decimal total = order.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice);
            decimal taxplus = order.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice * (decimal)od.Dish.TaxRate);
            total += taxplus;
            order.TotalAmount = total;

            // Change order status to "2 = approved"
            order.OrderStatus = 2;

            // Update table status
            var table = await _repo.GetTableByIdAsync(order.TableId);
            if (table != null)
            {
                table.Status = "Available";
            }

            await _repo.SaveChangesAsync();
            return (true, "Order has been approved and total amount calculated successfully.");
        }

        public (bool Success, string Message) CancelOrder(int orderId)
        {
            var order = _repo.GetOrderWithDetails(orderId);
            if (order == null)
                return (false, "Order not found.");

            // If any dish has been served -> cannot cancel
            bool hasServed = order.OrderDetails.Any(d => d.DishStatus == 1);
            if (hasServed)
                return (false, "Cannot cancel order because some dishes have been served.");

            // Mark order as cancelled
            order.OrderStatus = -1;
            order.TotalAmount = 0;

            // Update table status
            var table = _repo.GetTableById(order.TableId);
            if (table != null)
            {
                table.Status = "Available";
                table.CurrentOrderId = null;
            }

            _repo.SaveChanges();
            return (true, "Order has been cancelled (status saved in system).");
        }

        // --- Notes & Duplicate APIs ---
        public async Task<(bool Success, string Message)> UpdateOrderNoteAsync(int orderId, string note)
        {
            var ok = await _repo.UpdateOrderNoteAsync(orderId, note);
            return ok ? (true, "Order note updated successfully.") : (false, "Order not found.");
        }

        public async Task<(bool Success, string Message)> UpdateOrderDetailNoteAsync(int orderDetailId, string note)
        {
            var ok = await _repo.UpdateOrderDetailNoteAsync(orderDetailId, note);
            return ok ? (true, "Dish note updated successfully.") : (false, "Dish not found in order.");
        }

        public async Task<(bool Success, string Message)> DuplicateOrderDetailWithNoteAsync(int orderDetailId, string note)
        {
            return await _repo.DuplicateOrderDetailWithNoteAsync(orderDetailId, note);
        }
    }
}
