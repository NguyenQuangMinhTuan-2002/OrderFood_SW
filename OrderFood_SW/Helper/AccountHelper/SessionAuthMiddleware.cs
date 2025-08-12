// login check middleware
public class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Bỏ qua các trang không yêu cầu đăng nhập
        if (path.Contains("/account/login") ||
            path.Contains("/account/accessdenied") ||
            path.Contains("/css") ||
            path.Contains("/js"))
        {
            await _next(context);
            return;
        }

        var username = context.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        await _next(context);
    }
}
