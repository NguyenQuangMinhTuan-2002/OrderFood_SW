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
                    ?? throw new Exception("Không tìm thấy đơn hàng cũ!");
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

            // Thêm món
            foreach (var item in cart)
            {
                var existingDetail = _repo.GetOrderDetail(order.OrderId, item.DishId);

                if (existingDetail != null)
                    existingDetail.Quantity += item.Quantity;
                else
                    _repo.AddOrderDetail(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        DishId = item.DishId,
                        Quantity = item.Quantity,
                        DishStatus = 0
                    });
            }

            // Cập nhật tổng tiền
            order.TotalAmount = _repo.CalculateTotalAmount(order.OrderId);

            // Cập nhật bàn
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
                throw new Exception("Không tìm thấy đơn hàng");

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

        public async Task<bool> DeleteDishFromOrderAsync(int orderId, int dishId)
        {
            return await _repo.DeleteDishFromOrderAsync(orderId, dishId);
        }

        public async Task<bool> ToggleDishStatusAsync(int orderId, int dishId)
        {
            return await _repo.ToggleDishStatusAsync(orderId, dishId);
        }

        public async Task<(bool Success, string Message)> ApproveOrderAsync(int orderId)
        {
            var order = await _repo.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng");

            // Kiểm tra tất cả món đã được phục vụ
            bool allServed = order.OrderDetails.All(od => od.DishStatus == 1);
            if (!allServed)
                return (false, "Chỉ được duyệt đơn khi tất cả món đã được phục vụ.");

            // Tính tổng tiền
            decimal total = order.OrderDetails.Sum(od => od.Quantity * od.Dish.DishPrice);
            order.TotalAmount = total;

            // Đổi trạng thái đơn hàng sang "2 = đã duyệt"
            order.OrderStatus = 2;

            // Cập nhật trạng thái bàn
            var table = await _repo.GetTableByIdAsync(order.TableId);
            if (table != null)
            {
                table.Status = "Available";
            }

            await _repo.SaveChangesAsync();
            return (true, "Đơn hàng đã được duyệt và tính tổng tiền thành công.");
        }

        public (bool Success, string Message) CancelOrder(int orderId)
        {
            var order = _repo.GetOrderWithDetails(orderId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            // Nếu có món đã phục vụ -> không hủy
            bool hasServed = order.OrderDetails.Any(d => d.DishStatus == 1);
            if (hasServed)
                return (false, "Không thể hủy đơn vì đã có món được phục vụ.");

            // Đánh dấu đơn hàng hủy
            order.OrderStatus = -1;
            order.TotalAmount = 0;

            // Cập nhật trạng thái bàn
            var table = _repo.GetTableById(order.TableId);
            if (table != null)
            {
                table.Status = "Available";
                table.CurrentOrderId = null;
            }

            _repo.SaveChanges();
            return (true, "Đơn hàng đã được hủy (lưu trạng thái trong hệ thống).");
        }
    }
}
