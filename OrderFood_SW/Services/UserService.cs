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

        public async Task UpdateAsync(Users user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            await _repo.UpdateAsync(user);
        }

        public Task DeleteAsync(Users user) => _repo.DeleteAsync(user);

        private string HashPassword(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}
