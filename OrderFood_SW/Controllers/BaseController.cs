using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrderFood_SW.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Dùng context.HttpContext để tránh NullReference trong một số trường hợp
            var http = context.HttpContext;

            ViewBag.FullName = http.Session.GetString("FullName") ?? string.Empty;
            ViewBag.Username = http.Session.GetString("Username") ?? string.Empty;
            ViewBag.Role = http.Session.GetString("Role") ?? string.Empty;

            base.OnActionExecuting(context);
        }
    }
}
