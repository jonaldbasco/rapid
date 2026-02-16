using Microsoft.EntityFrameworkCore;
using rapid.core.app.Models;
using rapid.core.app.Source;
using System;

namespace rapid.core.app.Agents
{
    public class StaffingAgent
    {
        private readonly RapidDBContext _db;

        public StaffingAgent(RapidDBContext db)
        {
            _db = db;
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
    }
}
