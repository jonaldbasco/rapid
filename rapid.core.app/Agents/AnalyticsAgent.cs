using Newtonsoft.Json;
using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;

namespace rapid.core.app.Agents
{
    public class AnalyticsAgent
    {
        private readonly OpenAIService _ai;

        public AnalyticsAgent(OpenAIService ai)
        {
            _ai = ai;
        }

        public Task<List<StaffingForecast>> ForecastAsync()
        {

            // TODO: OpenAI call using real data
            return Task.FromResult(new List<StaffingForecast>
        {
            new() { Id = 1, Unit = "Burn Unit", Hour = DateTime.Now, ForecastDemand = 5 }
        });
        }
    }
}
