using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using rapid.core.app.Hub;
using rapid.core.app.Models;
using rapid.core.app.Prompts;
using rapid.core.app.Services;
using rapid.core.app.Source;
using System;
using System.Text.Json;
namespace rapid.core.app.Agents
{
    public class NegotiationAgent
    {
        private const int MaxRounds = 3;

        private readonly RapidDBContext _db;
        private readonly OpenAIService _ai;

        public NegotiationAgent(RapidDBContext db, OpenAIService ai)
        {
            _db = db;
            _ai = ai;
        }
        public async Task<bool> HasActiveNegotiationAsync(string userId)
        {
            return await _db.Negotiations
                .AnyAsync(n => n.StaffId == userId && n.Status == "Active");
        }

        public async Task RunNextRoundAsync(int negotiationId)
        {
            try
            {
                var negotiation = await _db.Negotiations.FindAsync(negotiationId);
                //var ai = await _db.NegotiationMessages.FindAsync(negotiationId);
                if (negotiation == null)
                    throw new Exception($"Negotiation {negotiationId} not found");
                if (negotiation == null || negotiation.Status != "Active")
                    return;
                //if (ai == null)
                //    throw new Exception($"Negotiation {negotiationId} not found");
                //if (ai == null || ai.NegotiationId != negotiation.Id)
                //    return;
                //// 🔹 NEW: AI always starts
                //if (negotiation.CurrentRound == 0)
                //{
                //    await StartNegotiationAsync(negotiation?.Id);
                //    return;
                //}
                if (negotiation.CurrentRound >= MaxRounds)
                {
                    negotiation.Status = "Escalated";
                    await _db.SaveChangesAsync();
                    return;
                }

                negotiation.CurrentRound++;

                var history = _db.NegotiationMessages
                    .Where(m => m.NegotiationId == negotiationId)
                    .OrderBy(m => m.Timestamp)
                    .Select(m => $"{m.Sender}: {m.Message}");

                var prompt = $"""
                UNIT: {negotiation.Unit}
                ROUND: {negotiation.CurrentRound}

                CONVERSATION:
                {string.Join("\n", history)}

                Return JSON:
                """ + """
                {
                  "aiMessage": "",
                  "nurseResponse": "Accept | Decline | Counter",
                  "counterMessage": ""
                }
                """;

                var response = await _ai.ChatAsync(
                    NegotiationPrompts.System,
                    prompt);

                var json = JsonDocument.Parse(response).RootElement;

                await SaveMessage(negotiationId, "AI",
                    json.GetProperty("aiMessage").GetString()!,
                    negotiation.CurrentRound);

                var nurseResponse = json.GetProperty("nurseResponse").GetString()!;

                if (nurseResponse == "Accept")
                {
                    negotiation.Status = "Accepted";
                    await NegotiationDecision(negotiationId, negotiation.Status, negotiation.CurrentRound);
                }
                else if (nurseResponse == "Decline")
                {
                    negotiation.Status = "Declined";
                    await NegotiationDecision(negotiationId, negotiation.Status,negotiation.CurrentRound);
                }
                else
                {
                    //await SaveMessage(negotiationId, negotiation.NurseName,
                    //    json.GetProperty("counterMessage").GetString()!,
                    //    negotiation.CurrentRound);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Negotiation Error:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task SaveMessage(
            int negotiationId,
            string sender,
            string message,
            int round)
        {
            var lastNegotiation = _db.NegotiationMessages
                         .OrderByDescending(n => n.Id)
                         .FirstOrDefault();
            var id = 1;

            if (lastNegotiation != null)
                id = lastNegotiation.Id + 1;

            _db.NegotiationMessages.Add(new NegotiationMessage
            {
                Id = id,
                NegotiationId = negotiationId,
                Sender = sender,
                Message = message,
                Round = round
            });

            await _db.SaveChangesAsync();
        }
        private async Task NegotiationDecision(
        int negotiationId,
        string status,
        int round)
        {
            var lastNegotiation = _db.Negotiations.FirstOrDefault(n => n.Id == negotiationId);

            if (lastNegotiation != null)
            {
                // Update only specific properties
                lastNegotiation.Status = status;  // Example: updating only the "Message" column
                lastNegotiation.CurrentRound = round;

                // Save changes to the database
                _db.SaveChanges();
            }

            var staffDecision = _db.Staff.FirstOrDefault(n => n.Id == lastNegotiation.StaffId);

            if (staffDecision != null)
            {
                // Update only specific properties
                staffDecision.Decision = status;  // Example: updating only the "Message" column

                // Save changes to the database
                _db.SaveChanges();
            }
            //await _hub.Clients.All.SendAsync(
            //    "NegotiationMessage",
            //    new { negotiationId, sender, message, round });
        }
        public async Task<int> StartNegotiationAsync(int? negotiationId)
        {
            var negotiation = _db.Negotiations
                .Where(u => u.Id == negotiationId).FirstOrDefault();
            var severity = "Surge";
            var prompt = NegotiationPrompts.Opening(negotiation.Unit, negotiation.NurseName, severity);

            var response = await _ai.ChatAsync(
                NegotiationPrompts.System,
                prompt);

            if (string.IsNullOrWhiteSpace(response))
                throw new Exception("OpenAI returned empty response");

            var aiMessage = SafeExtract(response, "aiMessage");
            var json = JsonDocument.Parse(response).RootElement;

            var messageIndex = _db.NegotiationMessages.OrderByDescending(x => x.Id).FirstOrDefault();
            var id = 0;
            if (messageIndex != null)
                id = messageIndex.Id + 1;
            else
                id = 1;
            //Guid.NewGuid() for string Id
                _db.NegotiationMessages.Add(new NegotiationMessage
                {
                    Id = id,
                    NegotiationId = negotiation.Id,
                    Sender = "AI",
                    Message = json.GetProperty("aiMessage").GetString()!,
                    Round = 0
                });

            //negotiation.CurrentRound = 1;

            await _db.SaveChangesAsync();
            return negotiation.Id;
        }
        public async Task<int> CreateNegotiationAsync(string userId)
        {
            // TODO: trigger createnegotiation
            var nurse = _db.Staff.FirstOrDefault(s => s.Id == userId);
            var surge = _db.SurgeRequests.FirstOrDefault(r => r.isActive == 1);
            if (nurse == null)
                return 0;

            var lastNegotiation = _db.Negotiations
                         .OrderByDescending(n => n.Id)
                         .FirstOrDefault();
            var id = 1;

            if (lastNegotiation != null)
                id = lastNegotiation.Id + 1;

            var negotiation = new NegotiationClass
            {
                Id = id,
                StaffId = nurse.Id,
                NurseName = nurse.Name,
                Unit = surge.Unit
            };

            _db.Negotiations.Add(negotiation);
            await _db.SaveChangesAsync();
            return id;
        }

        private static string SafeExtract(
        string response,
        string propertyName)
        {
            try
            {
                var doc = JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty(propertyName, out var prop))
                    return prop.GetString() ?? "";
            }
            catch { }

            return response; // fallback to raw text
        }




        //public async Task NegotiateAsync(
        //string unit,
        //int shortage,
        //string severity)
        //{
        //    var nurse = _db.Staff
        //        .Where(s => s.isAvailable == "true")
        //        .FirstOrDefault();

        //    if (nurse == null)
        //    {
        //        //await _hub.Clients.All.SendAsync(
        //        //    "Update", $"❌ No nurses available for {unit}");
        //        return;
        //    }

        //    var negotiation = new NegotiationClass
        //    {
        //        NurseName = nurse.Name,
        //        Unit = unit
        //    };

        //    _db.Negotiations.Add(negotiation);
        //    await _db.SaveChangesAsync();

        //    while (negotiation.CurrentRound < MaxRounds &&
        //       negotiation.Status == "Active")
        //    {
        //        negotiation.CurrentRound++;

        //        var history = _db.NegotiationMessages
        //            .Where(m => m.NegotiationId == negotiation.Id)
        //            .OrderBy(m => m.Timestamp)
        //            .Select(m => $"{m.Sender}: {m.Message}");

        //        var userPrompt = $$$"""
        //        UNIT: {unit}
        //        SEVERITY: {severity}
        //        SHORTAGE: {shortage}
        //        ROUND: {negotiation.CurrentRound}

        //        CONVERSATION:
        //        {string.Join("\n", history)}

        //        Return JSON:
        //        {{
        //          "aiMessage": "",
        //          "nurseResponse": "Accept | Decline | Counter",
        //          "counterMessage": ""
        //        }}
        //        """;

        //        var response = await _ai.ChatAsync(
        //            system: NegotiationPrompts.System,
        //            user: userPrompt);

        //        var json = JsonDocument.Parse(response).RootElement;

        //        var aiMessage = json.GetProperty("aiMessage").GetString()!;
        //        var nurseResponse = json.GetProperty("nurseResponse").GetString()!;
        //        var counter = json.GetProperty("counterMessage").GetString()!;

        //        await SaveAndBroadcast(negotiation.Id, "AI", aiMessage, negotiation.CurrentRound);

        //        if (nurseResponse == "Accept")
        //        {
        //            negotiation.Status = "Accepted";
        //            await SaveAndBroadcast(negotiation.Id, "Nurse", "I can take the shift.", negotiation.CurrentRound);
        //        }
        //        else if (nurseResponse == "Decline")
        //        {
        //            negotiation.Status = "Declined";
        //            await SaveAndBroadcast(negotiation.Id, "Nurse", "I’m unable to take this shift.", negotiation.CurrentRound);
        //        }
        //        else
        //        {
        //            await SaveAndBroadcast(negotiation.Id, "Nurse", counter, negotiation.CurrentRound);
        //        }

        //        await _db.SaveChangesAsync();
        //    }

        //    var negotiation = new NegotiationClass
        //    {
        //        NurseName = nurse.Name,
        //        Unit = unit,
        //        OfferMessage = result.GetProperty("offer").GetString()!,
        //        NurseResponse = result.GetProperty("response").GetString()!,
        //        Status = result.GetProperty("response").GetString()!
        //    };

        //    _db.Negotiations.Add(negotiation);
        //    await _db.SaveChangesAsync();

        //    await _hub.Clients.All.SendAsync(
        //        "Update",
        //        $"🤝 Negotiation with {nurse.Name}: {negotiation.Status}");
        //}
    }
}
