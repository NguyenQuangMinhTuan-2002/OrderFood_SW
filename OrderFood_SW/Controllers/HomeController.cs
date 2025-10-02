using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.ViewModels;
using System.Diagnostics;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();

        public IActionResult Documentation() => View();

        public IActionResult Privacy() => View();

        [Route("Home/Error")]
        public IActionResult Error(int? statusCode = null)
        {
            var exFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var currentStatusCode = statusCode ?? HttpContext.Response.StatusCode;
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = currentStatusCode,
                Path = exFeature?.Path ?? HttpContext.Request.Path,
                StackTrace = exFeature?.Error.StackTrace
            };

            // If there's an exception, use its message, otherwise use status code message
            if (exFeature?.Error != null)
            {
                model.Message = exFeature.Error.Message;
                _logger.LogError(exFeature.Error, "Unhandled exception occurred at path {Path}", model.Path);
            }
            else
            {
                model.Message = model.GetStatusCodeMessage();
                _logger.LogWarning("Status code {StatusCode} at path {Path}", currentStatusCode, model.Path);
            }

            return View(model);
        }

        [Route("Home/StatusCode")]
        public IActionResult StatusCodeHandler(int code)
        {
            // Redirect to unified Error page with status code
            return RedirectToAction("Error", new { statusCode = code });
        }

        public IActionResult Crash()
        {
            int x = 0;
            int y = 10 / x; // DivideByZeroException
            return Content(y.ToString());
        }

        public IActionResult Crash2()
        {
            throw new InvalidOperationException("Error test Serilog!");
        }
    }
}
