namespace rapid.core.app.Models
{
    public class StaffingForecast
    {
        public int Id { get; set; }
        public string Unit { get; set; }
        public DateTime Hour { get; set; }
        public int ForecastDemand { get; set; }
    }
}
