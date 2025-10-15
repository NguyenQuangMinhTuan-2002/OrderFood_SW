using Microsoft.AspNetCore.Mvc;
using OrderFood_SW.Helper;

namespace OrderFood_SW.Controllers
{
    [AuthorizeRole("Admin")]
    public class TestController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Send(string token)
        {
            var fcm = new FirebaseV1Helper("App_Data/firebase-key.json");
            var result = await fcm.SendAsync(token, "Data Update", "Page will auto reload");
            return Content(result, "application/json");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

    }
}
