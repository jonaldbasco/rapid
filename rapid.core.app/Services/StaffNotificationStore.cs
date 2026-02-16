using System.Collections.Concurrent;

namespace rapid.core.app.Services
{
    public static class StaffNotificationStore
    {
        private static readonly ConcurrentDictionary<string, byte> _notified = new();
        private static string _currentSurgeKey = "normal"; // used to reset when surge ends

        public static void ResetIfNewSurge(string surgeKey)
        {
            // surgeKey can be: "normal", "alert", "critical", "surge"
            // reset notifications when going back to normal OR when surge type changes (optional)
            if (surgeKey == "normal" && _currentSurgeKey != "normal")
            {
                _notified.Clear();
            }

            _currentSurgeKey = surgeKey;
        }

        public static void MarkNotified(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    _notified.TryAdd(id, 1);
            }
        }

        public static bool IsNotified(string id) => _notified.ContainsKey(id);

        public static IReadOnlyCollection<string> AllNotifiedIds() => _notified.Keys.ToList();
    }
}