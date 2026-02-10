using Microsoft.AspNetCore.Mvc;

namespace rapid.core.app.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
