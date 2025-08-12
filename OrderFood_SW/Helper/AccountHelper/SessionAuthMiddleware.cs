public class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Lấy URL hiện tại
        var path = context.Request.Path.Value?.ToLower();

        // Bỏ qua các trang Login, AccessDenied, css/js
        if (!path.Contains("/account/login") &&
            !path.Contains("/account/accessdenied") &&
            !path.Contains("/css") &&
            !path.Contains("/js"))
        {
            var username = context.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await _next(context);
    }
}
