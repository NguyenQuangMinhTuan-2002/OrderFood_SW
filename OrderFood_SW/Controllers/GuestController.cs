using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Services;

namespace OrderFood_SW.Controllers
{
    [AllowAnonymous]
    public class GuestController : Controller
    {
        private readonly GuestService _service;

        public GuestController(GuestService service)
        {
            _service = service;
        }

        // http://localhost:7000/Guest/QRCheck?tableId=1
        // http://localhost:19443/Guest/QRCheck?tableId=1
        public IActionResult QRCheck(int tableId)
        {
            var (action, routeValues, error) = _service.HandleQRCheck(tableId);

            if (!string.IsNullOrEmpty(error))
            {
                TempData["Error"] = error;
            }

            if (routeValues != null)
            {
                var parts = action.Split('/');
                return RedirectToAction(parts[1], parts[0], routeValues);
            }
            else
            {
                var parts = action.Split('/');
                return RedirectToAction(parts[1], parts[0]);
            }
        }
    }
}
