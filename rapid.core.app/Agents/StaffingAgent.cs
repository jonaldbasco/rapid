namespace rapid.core.app.Agents
{
    public class StaffingAgent
    {
        public List<(string Unit, int Shortage)> Detect(
                List<StaffingForecast> forecasts,
                Dictionary<string, int> current)
        {
            return forecasts
                .Where(f => f.ForecastDemand > current[f.Unit])
                .Select(f => (f.Unit, f.ForecastDemand - current[f.Unit]))
                .ToList();
        }
    }
}
