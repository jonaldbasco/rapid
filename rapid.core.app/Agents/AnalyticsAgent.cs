using Newtonsoft.Json;
using rapid.core.app.Models;
using rapid.core.app.Prompts;
using rapid.core.app.Services;
using rapid.core.app.Source;
using System.Text.Json;

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

        public async Task ResetUnitSurge(string unitName)
        {
            var unit = _db.Units.FirstOrDefault(u => u.Name == unitName);
            if (unit == null)
                return;

            unit.Incoming = 0;

            await _db.SaveChangesAsync();
        }

        public async Task<string> GetAnalyzedResult(string staffId)
        {
            var negotiation = _db.Negotiations
                .FirstOrDefault(n => n.StaffId == staffId && n.Status != "Active");

            if (negotiation == null)
                return "No completed negotiation found.";

            var history = _db.NegotiationMessages
                .Where(m => m.NegotiationId == negotiation.Id)
                .OrderByDescending(m => m.Timestamp)
                .Take(3)                     // ✅ last 3 messages
                .OrderBy(m => m.Timestamp)
                .Select(m => $"{m.Sender}: {m.Message}")
                .ToList();

            if (!history.Any())
                return "No negotiation messages available.";

            var prompt = $"""
                Analyze the negotiation chat history between the hospital and a nurse.

                Determine:
                1. The nurse’s decision: ACCEPTED, DECLINED, or PENDING
                2. Whether BONUS or PREMIUM pay was mentioned
                3. Whether the acceptance (if any) was voluntary
                4. Whether the nurse accepted without making a counter-offer

                Definitions:
                - ACCEPTED: Clear agreement (e.g., “I accept”, “Yes, I can take the shift”)
                - DECLINED: Clear refusal (e.g., “I can’t take this”, “I’m unavailable”)
                - PENDING: No clear acceptance or refusal
                - BONUS / PREMIUM: Any mention of extra or incentive pay
                - VOLUNTARY: No pressure or coercion language
                - NO COUNTER: No request to change pay, shift, or conditions

                Rules:
                - Use ONLY the chat history
                - Ignore polite acknowledgements or questions
                - If messages conflict, use the last clear nurse message
                - If unclear, return PENDING

                Chat History:
                --------------------
                {string.Join("\n", history)}
                --------------------
                Return JSON:
                """ + """
                {
                  "summary": "<1–2 sentence explanation>"
                }
                """;

            var response = await _ai.ChatAsync(
                AnalyticPrompts.System,
                prompt);

            try
            {
                using var doc = JsonDocument.Parse(response);

                var lastNegotiation = _db.Summary
                         .OrderByDescending(n => n.Id)
                         .FirstOrDefault();
                var id = 1;
                if (lastNegotiation != null)
                    id = lastNegotiation.Id + 1;

                _db.Summary.Add(new SummaryClass
                {
                    Id = id,
                    NegotiationId = negotiation.Id,
                    StaffId = negotiation.StaffId,
                    Summary = doc.RootElement
                              .GetProperty("summary")
                              .GetString()
                           ?? "Analysis completed, but summary was empty."
                });

                await _db.SaveChangesAsync();

                return doc.RootElement
                          .GetProperty("summary")
                          .GetString()
                       ?? "Analysis completed, but summary was empty.";
            }
            catch
            {
                return "Analysis failed due to invalid AI response.";
            }
        }
    }
}
