using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _service;

        public AccountController(AccountService service)
        {
            _service = service;
        }

        public IActionResult AccessDenied() => View();

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = await _service.AuthenticateAsync(username, password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetInt32("IsActive", user.IsActive ? 1 : 0);
                HttpContext.Session.SetString("ImageAvat", user.ImageAvat ?? "nophoto1.png");

                if (user.Role == "Admin" || user.Role == "Staff")
                    return RedirectToAction("Index", "Home");
                else if (user.Role == "Customer")
                    return RedirectToAction("Index", "CustomerOrder");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string fullName, string email)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }

            var success = await _service.RegisterCustomerAsync(username, password, fullName, email);
            
            if (success)
            {
                ViewBag.Success = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return View("Login");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên đăng nhập khác.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
