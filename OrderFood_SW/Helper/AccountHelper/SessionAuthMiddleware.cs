using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // (A) Public endpoints – cho qua luôn
        if (path.StartsWith("/account") ||
            path.StartsWith("/customer/qrcheck") ||
            path.StartsWith("/guest/qrcheck") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/images"))
        {
            await _next(context);
            return;
        }

        // (B) Đã login? → OK
        var userId = context.Session.GetInt32("UserId");
        if (userId != null)
        {
            var role = context.Session.GetString("Role") ?? "Customer";
            context.Session.SetString("Role", role);
            context.Items["Role"] = role;
            await _next(context);
            return;
        }

        // (C) Guest qua QR? → gán như Customer
        var tableId = context.Session.GetInt32("CurrentTableId");
        if (tableId != null)
        {
            // ép role "Customer" để đi chung luồng
            context.Session.SetString("Role", "Customer");
            context.Items["Role"] = "Customer";
            await _next(context);
            return;
        }

        // (D) Không có gì → login
        context.Response.Redirect("/Account/Login");
    }

}
