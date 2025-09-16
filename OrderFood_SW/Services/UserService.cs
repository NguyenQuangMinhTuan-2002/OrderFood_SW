using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Services
{
    public class UserService
    {
        private readonly UserRepository _repo;
        public UserService(UserRepository repo) => _repo = repo;

        public Task<(List<Users> Users, int TotalRows)> GetPagedAsync(string keyword, int page, int pageSize) =>
            _repo.GetPagedAsync(keyword, page, pageSize);

        public Task<Users?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task AddAsync(Users user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            await _repo.AddAsync(user);
        }

        public async Task<Users?> UpdateAsync(Users vm)
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
                user.PasswordHash = HashPassword(vm.NewPassword);
            }

            await _repo.UpdateAsync(user);
            return user;
        }

        public Task DeleteAsync(Users user) => _repo.DeleteAsync(user);

        private string HashPassword(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
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
