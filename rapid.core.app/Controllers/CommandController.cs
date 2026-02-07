using Microsoft.AspNetCore.Mvc;

namespace rapid.core.app.Controllers
{
    public class CommandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
