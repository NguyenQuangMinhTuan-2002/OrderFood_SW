using Microsoft.AspNetCore.Http;
using OrderFood_SW.Models;
using OrderFood_SW.Repositories;

namespace OrderFood_SW.Services
{
    public class GuestService
    {
        private readonly GuestRepository _repo;
        private readonly IHttpContextAccessor _httpContext;

        public GuestService(GuestRepository repo, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _httpContext = httpContext;
        }

        public (string action, object? routeValues, string? error) HandleQRCheck(int tableId)
        {
            var table = _repo.GetTableById(tableId);
            if (table == null)
            {
                return ("Home/Index", null, "Table not found.");
            }

            var session = _httpContext.HttpContext!.Session;

            // Set session cho guest
            session.SetInt32("CurrentTableId", table.TableId);
            session.SetInt32("TableId", table.TableId);
            session.SetString("Role", "Customer");

            if (!session.GetInt32("UserId").HasValue)
                session.SetInt32("UserId", 1); // Default guest

            var currentUserId = session.GetInt32("UserId");

            // If table has open order
            if (table.CurrentOrderId.HasValue)
            {
                var order = _repo.GetOrderById(table.CurrentOrderId.Value);

                if (order != null && order.OrderStatus == 1)
                {
                    if (currentUserId == 1) // Guest scanning QR
                    {
                        if (order.UserId == 1)
                        {
                            session.SetInt32("CurrentOrderId", order.OrderId);
                            return ("Customer/OrderDetails", new { orderId = order.OrderId }, null);
                        }
                        else
                        {
                            return ("Account/AccessDenied", null, null);
                        }
                    }
                }

                // Order closed/cancelled → reset table
                table.Status = "Available";
                table.CurrentOrderId = null;
                _repo.UpdateTable(table);
            }

            // If we reach here → no order yet -> Guest can create new order
            session.Remove("CurrentOrderId");
            session.Remove("Cart");

            return ("CustomerOrder/CreateOrder", new { tableId = table.TableId }, null);
        }
    }
}
