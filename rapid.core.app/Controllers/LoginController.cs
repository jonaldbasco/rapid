using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Agents;
using rapid.core.app.Models;
using rapid.core.app.Source;
namespace rapid.core.app.Controllers
{
    public class LoginController : Controller
    {
        private readonly RapidDBContext _db;

        public LoginController(RapidDBContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate(string Uname, string Upass)
        {
            var user = _db.User
                .FirstOrDefault(u => u.UserName == Uname && u.UserPass == Upass);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid username or password"
                });
            }
            else
            {
                var userD = _db.UserDetails
                    .FirstOrDefault(u => u.UserId == user.UserId);

                if (userD == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No User details"
                    });
                }
                else
                {
                    // Optional: set auth cookie / session here

                    // Store only what you need
                    HttpContext.Session.SetString("UserId", userD.UserId);
                    HttpContext.Session.SetString("FirstName", userD.FirstName);
                    HttpContext.Session.SetString("LastName", userD.LastName);
                    HttpContext.Session.SetString("Role", userD.Position);

                    if (userD.Position == "Nurse Manager")
                    {
                        return Json(new
                        {
                            success = true,
                            redirectUrl = Url.Action("Index", "Home")
                        });
                    }
                    else 
                    {
                        return Json(new
                        {
                            success = true,
                            redirectUrl = Url.Action("Chat", "Nurse")
                        });
                    }

                }
            }

        }
    }
}
