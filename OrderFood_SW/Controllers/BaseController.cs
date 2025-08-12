using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var fullName = HttpContext.Session.GetString("FullName");
        var username = HttpContext.Session.GetString("Username");
        var role = HttpContext.Session.GetString("Role");

        ViewBag.FullName = fullName;
        ViewBag.Username = username;
        ViewBag.Role = role;

        base.OnActionExecuting(context);
    }
}
