using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// This attribute checks if the user has the required roles to access a controller or action method.
namespace OrderFood_SW.Helper
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }
    }
}
