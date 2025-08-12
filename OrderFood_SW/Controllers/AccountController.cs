using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using System.Security.Cryptography;
using System.Text;

namespace OrderFood_SW.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelperEF _db;

        public AccountController(DatabaseHelperEF db)
        {
            _db = db;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập thì chuyển hướng luôn
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            string passwordHash = HashPassword(password);

            var user = _db.Users
                          .FirstOrDefault(u => u.Username == username
                                            && u.PasswordHash == passwordHash
                                            && u.IsActive);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                // Chuyển hướng tùy role
                if (user.Role == "Admin" || user.Role == "Staff")
                    return RedirectToAction("Index", "Home");
                else if (user.Role == "Customer")
                    return RedirectToAction("Index", "CustomerOrder");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
