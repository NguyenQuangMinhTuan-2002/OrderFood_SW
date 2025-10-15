using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using OrderFood_SW.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo;

        public CustomerService(CustomerRepository repo)
        {
            _repo = repo;
        }

        public List<OrderHistoryViewModel> GetRecentOrders(int userId)
        {
            return _repo.GetRecentOrdersByUser(userId, 4);
        }

        public (List<OrderHistoryViewModel> Orders, int TotalOrders, int TotalPages)
            GetOrderHistory(int userId, string status, DateTime? fromDate, DateTime? toDate, int page, int pageSize = 10)
        {
            return _repo.GetOrderHistory(userId, status, fromDate, toDate, page, pageSize);
        }

        public List<OrderHistoryViewModel> GetProcessingOrders(int userId)
        {
            return _repo.GetProcessingOrders(userId);
        }

        public (bool Success, string Message, int? TableId) CancelOrder(int orderId)
        {
            var order = _repo.GetOrderWithDetails(orderId);
            if (order == null)
                return (false, "Order not found.", null);

            bool hasServed = order.OrderDetails.Any(d => d.DishStatus == 1);
            if (hasServed)
                return (false, "Cannot cancel order because some dishes have been served.", order.TableId);

            // Update order status
            order.OrderStatus = -1;
            order.TotalAmount = 0;
            _repo.UpdateOrder(order);

            // Update table status
            var table = _repo.GetTableById(order.TableId);
            if (table != null)
            {
                table.Status = "Available";
                table.CurrentOrderId = null;
                _repo.UpdateTable(table);
            }

            _repo.Save();
            return (true, "Order has been cancelled (status saved in system).", order.TableId);
        }

        public async Task<(bool Success, string? Message, OrderDetailViewModel? Data, int? TableNumber)>
            GetOrderDetailsAsync(int orderId)
        {
            var order = await _repo.GetOrderByIdAsync(orderId);
            if (order == null)
                return (false, "Order not found.", null, null);

            var table = await _repo.GetTableByIdAsync(order.TableId);

            var details = await _repo.GetOrderDetailsWithDishesAsync(orderId);

            var vm = new OrderDetailViewModel
            {
                Order = order,
                OrderDetails = details
            };

            return (true, null, vm, table?.TableNumber);
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;

            return new EditUserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                ImageAvat = user.ImageAvat,
                NewPassword = user.PasswordHash
            };
        }

        public async Task<Users?> UpdateUserAsync(EditUserViewModel vm)
        {
            var user = await _repo.GetByIdAsync(vm.UserId);
            if (user == null) return null;

            user.UserId = vm.UserId;
            user.Username = vm.Username;
            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.Role = vm.Role;
            user.ImageAvat = vm.ImageAvat;
            user.IsActive = vm.IsActive;

            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                user.PasswordHash = ComputeSha256Hash(vm.NewPassword);
            }

            await _repo.UpdateAsync(user);
            return user;
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _repo.GetByIdAsync(userId);
        }

        public Task<bool> UserExistsAsync(int id)
        {
            return _repo.ExistsAsync(id);
        }

        public async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Users");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "nophoto1.png") return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Users", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
