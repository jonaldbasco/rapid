using rapid.core.app.Services;

namespace rapid.core.app.Agents
{
    public class AnalyticsAgent
    {
        private readonly OpenAIService _openAi;

        public async Task<List<SurgeForecast>> ForecastAsync(SurgeContext context)
        {
            var prompt = PromptBuilder.BuildForecastPrompt(context);

            var response = await _openAi.ChatAsync(prompt);

            return JsonSerializer.Deserialize<List<SurgeForecast>>(response);
        }
    }
}
