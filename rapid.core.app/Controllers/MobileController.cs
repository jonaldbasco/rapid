using Microsoft.AspNetCore.Mvc;

namespace rapid.core.app.Controllers
{
    public class MobileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
