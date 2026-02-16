using Newtonsoft.Json;
using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;

namespace rapid.core.app.Agents
{
    public class AnalyticsAgent
    {
        private readonly OpenAIService _openAi;
        private readonly OpenAIClass openAi;

        //public async Task<List<SurgeForecast>> ForecastAsync(RapidDBContext context)//SurgeContext context)
        //{
        //    var prompt = PromptBuilder.BuildForecastPrompt(context);

        //    var response = await openAi.ChatAsync(prompt);

        //    return JsonSerializer.Deserialize<List<SurgeForecast>>(response);
        //}

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
