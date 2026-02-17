using Microsoft.EntityFrameworkCore;
using rapid.core.app.Models;
using rapid.core.app.Source;
using System;

namespace rapid.core.app.Agents
{
    public class StaffingAgent
    {
        private readonly RapidDBContext _db;
        private readonly NegotiationAgent _negotiation;

        public StaffingAgent(RapidDBContext db, NegotiationAgent negotiation)
        {
            _db = db;
            _negotiation = negotiation;
        }
        public List<(string Unit, int Shortage)> Detect(
        List<StaffingForecast> forecasts)
        {
            var units = _db.Units
                .ToDictionary(u => u.Name, u => u.CurrentStaff);

            return forecasts
                .Where(f => units.ContainsKey(f.Unit))
                .Where(f => f.ForecastDemand > units[f.Unit])
                .Select(f => (
                    f.Unit,
                    f.ForecastDemand - units[f.Unit]
                ))
                .ToList();
        }
        public async Task<List<StaffClass>> GetStaff()
        {
            return await _db.Staff
            .Where(s => s.Role == "RN")
            .ToListAsync();
        }

        public async Task<int> CheckStaffNegotiation(string userId)
        {
            var negotiation = 0;
            var staff = _db.Staff.FirstOrDefault(s => s.Id == userId);

            if (staff.Decision == "Negotiating")
                negotiation = await _negotiation.CreateNegotiationAsync(userId);

            return negotiation;
        }
        public async Task<bool> SetStafftoAvailable()
        {
            var nurse = _db.Staff
                           .Where(s => s.isAvailable == "true")
                           .ToList();
            if (nurse == null)
                return false;


            foreach (var staff in nurse)
            {
                if (staff.Decision == "Unavailable")
                {
                    staff.Decision = "Available";
                }
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ResetStaffContact()
        {
            var nurse = _db.Staff
               .Where(s => s.isAvailable == "true")
               .ToList();
            if (nurse == null)
                return;

            foreach (var staff in nurse)
            {
                staff.Decision = "Unavailable";
            }
            await _db.SaveChangesAsync();
        }
    }
}
