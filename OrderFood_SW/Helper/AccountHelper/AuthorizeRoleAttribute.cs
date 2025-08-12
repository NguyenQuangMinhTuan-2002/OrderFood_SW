using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace OrderFood_SW.Helper
{
    /// <summary>
    /// Attribute kiểm tra đăng nhập + quyền truy cập
    /// </summary>
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        // Truyền role vào khi áp dụng
        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = context.HttpContext.Session.GetString("Username");
            var role = context.HttpContext.Session.GetString("Role");

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (controller == "Account" &&
            (action == "Login" || action == "AccessDenied"))
            {
                return;
            }

            // Chưa đăng nhập
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Nếu có truyền role và role hiện tại không nằm trong danh sách cho phép
            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }
    }
}
