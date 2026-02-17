using Newtonsoft.Json;
using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;

namespace rapid.core.app.Agents
{
    public class AnalyticsAgent
    {
        private readonly OpenAIService _ai;
        private readonly RapidDBContext _db;

        public AnalyticsAgent(OpenAIService ai, RapidDBContext db)
        {
            _ai = ai;
            _db = db;
        }

        public Task<List<StaffingForecast>> ForecastAsync()
        {

            // TODO: OpenAI call using real data
                return Task.FromResult(new List<StaffingForecast>
            {
                new() { Id = 1, Unit = "Burn Unit", Hour = DateTime.Now, ForecastDemand = 15 }
            });
        }
        public async Task SetStafftoNegotiating()
        {
            var nurse = _db.Staff
                           .Where(s => s.Decision == "Available" && s.isAvailable == "true")
                           .ToList();
            if (nurse == null)
                return;


            foreach (var staff in nurse)
            {
                if (staff.Decision == "Available")
                {
                    staff.Decision = "Negotiating";
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
