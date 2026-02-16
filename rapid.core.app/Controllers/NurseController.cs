using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Agents;
using rapid.core.app.Models;
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
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            //await RunSurge(userId);
            var negotiationId = 1;//HttpContext.Session.GetInt32("NegotiationId");
            var userName = HttpContext.Session.GetString("FirstName") + " " + HttpContext.Session.GetString("LastName");
            ViewBag.Name = userName;
            ViewBag.NegotiationId = negotiationId;
            ViewBag.HideHeader = true;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RunSurge(string userId)
        {
            var negotiationId = await _orchestrator.NegotiationAsync(userId);

            if (negotiationId == 0)
                return BadRequest("Negotiation could not be started.");

            HttpContext.Session.SetInt32("NegotiationId", negotiationId);

            return Ok(new { negotiationId });

            //await _orchestrator.NegotiationAsync(userId);
            //return Ok(new { status = "Orchestrator executed" });
        }
    }
}
