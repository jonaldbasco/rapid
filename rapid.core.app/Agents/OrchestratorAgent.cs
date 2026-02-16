using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using rapid.core.app.Hub;
using rapid.core.app.Models;
using rapid.core.app.Source;
using System;

namespace rapid.core.app.Agents
{
    public class OrchestratorAgent
    {
        private readonly AnalyticsAgent _analytics;
        private readonly StaffingAgent _staffing;
        private readonly NegotiationAgent _negotiation;
        private readonly RapidDBContext _db;
        //private readonly IHubContext<SurgeHub> _hub;

        public OrchestratorAgent( 
            AnalyticsAgent analytics, 
            StaffingAgent staffing,
            NegotiationAgent negotation,
            RapidDBContext db)
        {
            _analytics = analytics;
            _staffing = staffing;
            _negotiation = negotation;
            _db = db;
        }
        public async Task ExecuteAsync()
        {
            var forecast = await _analytics.ForecastAsync();
            
            var shortages = _staffing.Detect(forecast);

            if (!shortages.Any())
            {
                //await _hub.Clients.All.SendAsync(
                //    "Update", "✅ No shortages detected");
                return;
            }

            foreach (var shortage in shortages)
            {
                //await _hub.Clients.All.SendAsync(
                //    "Update",
                //    $"⚠️ {shortage.Unit} shortage: {shortage.Shortage}");
                await _staffing.GetStaff();
                await _negotiation.RunAsync(shortage.Unit, shortage.Shortage);
            }    
        }
        public async Task<int> NegotiationAsync(string userId)
        {
            if (!await _negotiation.HasActiveNegotiationAsync())
                return 0;

            // There is active data
            var negotiationId = await _db.Negotiations
                .Where(n => n.StaffId == userId && n.Status == "Active")
                .Select(n => n.Id)
                .FirstOrDefaultAsync();

            if (negotiationId == 0)
                return 0;

            return await _negotiation.StartNegotiationAsync(negotiationId);
            //var id = _db.Negotiations.Where(n => n.StaffId == userId && n.Status == "Active").FirstOrDefault();
            //var negotiationId = await _negotiation.StartNegotiationAsync(id?.Id);
            //return negotiationId;

        }
    }
}
