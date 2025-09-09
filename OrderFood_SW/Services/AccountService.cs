using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo;
        public AccountService(AccountRepository repo) => _repo = repo;

        public async Task<Users?> AuthenticateAsync(string username, string password)
        {
            var hash = HashPassword(password);
            return await _repo.GetUserAsync(username, hash);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
