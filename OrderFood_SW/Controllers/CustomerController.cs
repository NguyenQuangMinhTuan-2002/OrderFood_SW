using Microsoft.AspNetCore.Mvc;

namespace OrderFood_SW.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
    }
}
