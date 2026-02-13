namespace rapid.core.app.Services
{
    public static class StaffCallDecisionStore
    {
        // staffId -> "pending" | "accepted" | "declined"
        private static readonly Dictionary<string, string> _decisions = new();

        public static string GetDecision(string staffId)
        {
            if (_decisions.TryGetValue(staffId, out var d))
                return d;
            return "pending";
        }

        public static void SetAccepted(string staffId) => _decisions[staffId] = "accepted";
        public static void SetDeclined(string staffId) => _decisions[staffId] = "declined";
        public static void SetPending(string staffId) => _decisions[staffId] = "pending";
    }
}