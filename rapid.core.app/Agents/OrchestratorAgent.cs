namespace rapid.core.app.Agents
{
    public class OrchestratorAgent
    {
        private readonly StaffingAgent _staffing;
        private readonly AnalyticsAgent _analytics;
        private readonly NegotiationAgent _negotiation;

        public OrchestratorAgent( AnalyticsAgent analytics, StaffingAgent staffing)
        {
            _analytics = analytics;
            _staffing = staffing;
        }
        public async Task HandleSurgeAsync()
        {
            var forecast = await _analytics.ForecastDemandAsync();
            var shortages = _staffing.DetectShortages(forecast);

            if (shortages.Any())
                await _negotiation.InitiateAsync(shortages);
        }
        public async Task ExecuteAsync(Dictionary<string, int> current)
        {
            var forecast = await _analytics.ForecastAsync();
            var shortages = _staffing.Detect(forecast, current);

            // TODO: trigger negotiation agent
        }
    }
}
