using System.Collections.Concurrent;

namespace rapid.core.app.Services
{
    public static class StaffDecisionStore
    {
        // nurseId -> decision: "pending" | "confirmed" | "declined"
        private static readonly ConcurrentDictionary<string, string> _decision = new();

        public static string Get(string id) => _decision.TryGetValue(id, out var d) ? d : "pending";
        public static void Confirm(string id) => _decision[id] = "confirmed";
        public static void Decline(string id) => _decision[id] = "declined";

        // optional: preload confirmed nurses for simulation
        public static void SeedConfirmed(params string[] ids)
        {
            foreach (var id in ids) _decision[id] = "confirmed";
        }

        public static IReadOnlyDictionary<string, string> All() => _decision;
    }
}