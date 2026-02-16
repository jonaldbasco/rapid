using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Agents;
using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;
using System;
using System.Diagnostics;

namespace rapid.core.app.Controllers
{
    public class NegotiationController : Controller
    {
        private readonly RapidDBContext _db;
        private readonly NegotiationAgent _agent;

        public NegotiationController(RapidDBContext db, NegotiationAgent agent)
        {
            _db = db;
            _agent = agent;
        }
        //[HttpPost]
        //public async Task<IActionResult> NextRound(int negotiationId)
        //{
        //    await _agent.RunNextRoundAsync(negotiationId);
        //    return Ok();
        //}

        [HttpGet]
        public IActionResult GetMessages(int negotiationId)
        {
            var messages = _db.NegotiationMessages
                .Where(m => m.NegotiationId == negotiationId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Sender,
                    m.Message,
                    m.Round,
                    m.Timestamp
                });

            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> NurseReply(
        int negotiationId,
        string message)
        {
            var negotiation = await _db.Negotiations.FindAsync(negotiationId);
            if (negotiation == null || negotiation.Status != "Active")
                return BadRequest("Negotiation closed");

            var lastNegotiation = _db.NegotiationMessages
                         .OrderByDescending(n => n.Id)
                         .FirstOrDefault();
            var id = 1;

            if (lastNegotiation != null)
                id = lastNegotiation.Id + 1;
            // Save human reply
            _db.NegotiationMessages.Add(new NegotiationMessage
            {
                Id = id,
                NegotiationId = negotiationId,
                Sender = negotiation.NurseName,
                Message = message,
                Round = negotiation.CurrentRound
            });

            await _db.SaveChangesAsync();

            var lastMessage = _db.NegotiationMessages
                .Where(m => m.NegotiationId == negotiationId)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            if (lastMessage?.Sender == "AI")
                return Ok();
            // 🔥 AUTO-TRIGGER AI RESPONSE
            await _agent.RunNextRoundAsync(negotiationId);

            return Ok();
        }
    }
}
