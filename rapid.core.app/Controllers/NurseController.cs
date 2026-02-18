using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Agents;
using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;

namespace rapid.core.app.Controllers
{
    public class NurseController : Controller
    {
        private readonly RapidDBContext _db;
        private readonly NegotiationAgent _agent;
        private readonly OrchestratorAgent _orchestrator;

        public NurseController(RapidDBContext db, NegotiationAgent agent, OrchestratorAgent orchestrator)
        {
            _db = db;
            _agent = agent;
            _orchestrator = orchestrator;
        }
        //public async Task<IActionResult> Index()
        //{
        //    var userId = HttpContext.Session.GetString("UserId");
        //    var userName = HttpContext.Session.GetString("FirstName") + " " + HttpContext.Session.GetString("LastName");
        //    ViewBag.Name = userName;

        //    ViewBag.HideHeader = true;
        //    var nurse = StaffStore.GetById("s3"); // RN Emily Rodriguez

        //    //if (nurse == null || nurse.Role != "nurse")
        //    //    return Unauthorized();

        //    //var requests = SurgeStore.GetActiveForSpecialty(nurse.Specialty)
        //    //    .Select(r => new NurseRequestItem
        //    //    {
        //    //        Request = r,
        //    //        Decision = SurgeStore.GetDecision(r.Id, nurse.Id),
        //    //        TimeAgoLabel = TimeAgo(r.CreatedAtUtc)
        //    //    })
        //    //    .ToList();
        //    ViewBag.HideHeader = true;
        //    return View();
        //}
        public async Task<IActionResult> Chat()
        {
            var userId = HttpContext.Session.GetString("UserId");
            await RunSurge(userId);
            var negotiationId = HttpContext.Session.GetInt32("NegotiationId");
            var userName = HttpContext.Session.GetString("FirstName") + " " + HttpContext.Session.GetString("LastName");
            ViewBag.Name = userName;
            ViewBag.NegotiationId = negotiationId;
            ViewBag.HideHeader = true;
            return View();
        }

        //public async Task<IActionResult> NurseChat()
        //{
        //    var userId = HttpContext.Session.GetString("UserId");
        //    //await RunSurge(userId);
        //    var negotiationId = 1;//HttpContext.Session.GetInt32("NegotiationId");
        //    var userName = HttpContext.Session.GetString("FirstName") + " " + HttpContext.Session.GetString("LastName");
        //    ViewBag.Name = userName;
        //    ViewBag.NegotiationId = negotiationId;
        //    ViewBag.HideHeader = true;
        //    return View();
        //}
        [HttpPost]
        public async Task<IActionResult> RunSurge(string userId)
        {
            var negotiationId = await _orchestrator.NegotiationAsync(userId);

            if (negotiationId == 0)
                return Ok(new { status = "No Active Negotiation" });
            ;
                //No Active Negotiation
                //return BadRequest("Negotiation could not be started.");

            HttpContext.Session.SetInt32("NegotiationId", negotiationId);

            return Ok(new { negotiationId });

            //await _orchestrator.NegotiationAsync(userId);
            //return Ok(new { status = "Orchestrator executed" });
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();   // remove all session data
            //await HttpContext.SignOutAsync();    // remove auth cookie (if any)

            return RedirectToAction("Index", "Login");
        }
    }
}
