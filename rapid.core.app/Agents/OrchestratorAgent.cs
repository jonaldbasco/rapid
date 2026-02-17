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
        public async Task ExecuteSurgeAsync()
        {
            //var request = _db.SurgeRequests.Where

            //                var _id = Guid.NewGuid().ToString("N");

            //_db.SurgeRequests.Add(new SurgeRequest
            //{
            //    Id = _id,
            //    Unit = "Burn Unit",
            //    SpecialtyTag = "Burn Unit",
            //    IsCritical = true,
            //    Description = "Mass casualty event. Burn Unit support urgently needed.",
            //    Location = "Metro General Hospital",
            //    PayMultiplier = 1.5,
            //    DurationHours = 4,
            //    CreatedAtUtc = DateTime.UtcNow
            //});
            //_db.SaveChanges();

            //This code is for updating the existing surge request the upper code should insert new surge request
            var surge = _db.SurgeRequests.Where(s => s.isActive == 0).FirstOrDefault();

            if (surge != null)
            {
                surge.isActive = 1;

                var saveIncoming = _db.Units.FirstOrDefault(i => i.Name == "Burn Unit"); // You can change this to unitId

                if (saveIncoming != null)
                {
                    // Update only specific properties
                    saveIncoming.Incoming = 15;  // This data can come from another application for now static

                    // Save changes to the database
                    _db.SaveChanges();
                }
                _db.SaveChanges();
            }

            var forecast = await _analytics.ForecastAsync();
            
            var shortages = _staffing.Detect(forecast);

            if (!shortages.Any())
                return;

            bool set =  await _staffing.SetStafftoAvailable();

            if (set == true)
                await _analytics.SetStafftoNegotiating();

            //foreach (var shortage in shortages)
            //{

            //await _negotiation.RunAsync(shortage.Unit, shortage.Shortage);
            //}    
        }
        public async Task<int> NegotiationAsync(string userId)
        {
            var negotiationId = 0;
            if (!await _negotiation.HasActiveNegotiationAsync(userId))
            {
                var checkStaff = await _staffing.CheckStaffNegotiation(userId);

                if (checkStaff != 0)
                    negotiationId = checkStaff;
                else
                    return 0;
            }
            else
            {
                // There is active data
                negotiationId = await _db.Negotiations
                    .Where(n => n.StaffId == userId && n.Status == "Active")
                    .Select(n => n.Id)
                    .FirstOrDefaultAsync();

                if (negotiationId == 0)
                    return 0;
            }

            return await _negotiation.StartNegotiationAsync(negotiationId);
        }
    }
}
