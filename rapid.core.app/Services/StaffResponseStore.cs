using System.Collections.Concurrent;

namespace rapid.core.app.Services
{
    public static class StaffResponseStore
    {
        // nurseId -> response
        private static readonly ConcurrentDictionary<string, string> _responses = new();

        // allowed values: "notified", "accepted", "declined"
        public static void MarkNotified(IEnumerable<string> ids)
        {
            foreach (var id in ids)
                _responses.TryAdd(id, "notified");
        }

        public static void MarkAccepted(string id)
        {
            _responses[id] = "accepted";
        }

        public static void MarkDeclined(string id)
        {
            _responses[id] = "declined";
        }

        public static string GetResponse(string id)
        {
            return _responses.TryGetValue(id, out var r) ? r : "none";
        }

        public static IReadOnlyDictionary<string, string> All() => _responses;
    }
}