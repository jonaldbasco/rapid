using rapid.core.app.Models;

namespace rapid.core.app.Services
{
    public static class SurgeStore
    {
        private static readonly List<SurgeRequest> _requests = new();

        private static readonly Dictionary<(string requestId, string nurseId), NurseDecision> _responses
            = new();

        private static bool _seeded = false;

        
        private static bool _surgeActive = false;

        public static bool IsSurgeActive() => _surgeActive;

        private static DateTime? _activatedAtUtc;
        private static int _activationCount = 0;

        public static void ActivateSurge()
        {

            _activatedAtUtc = DateTime.UtcNow;
            _activationCount++;
            _surgeActive = true;

            // Optional: seed demo requests only when surge is first activated
            EnsureSeeded();
        }

        public static void ResetSurge()
        {
            _surgeActive = false;
            _requests.Clear();
            _responses.Clear();
            _seeded = false;
        }

        public static void EnsureSeeded()
        {
            if (_seeded) return;
            _seeded = true;

            if (!_surgeActive) return;


            // (Because ActivateSurge() calls EnsureSeeded())
            _requests.Add(new SurgeRequest
            {
                Id = "req-trauma-accepted",
                IsCritical = true,
                Unit = "Trauma Bay",
                Location = "Metro General Hospital - Trauma Bay",
                Description = "Mass casualty incident. Multiple trauma cases incoming.",
                PayMultiplier = 1.5,
                DurationHours = 4,
                SpecialtyTag = "Trauma",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30)
            });

            _requests.Add(new SurgeRequest
            {
                Id = "req-trauma-declined",
                IsCritical = false,
                Unit = "Trauma Ward",
                Location = "Metro General Hospital",
                Description = "Additional trauma coverage requested for night shift.",
                PayMultiplier = 1.25,
                DurationHours = 6,
                SpecialtyTag = "Trauma",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
            });

            _requests.Add(new SurgeRequest
            {
                Id = "req-trauma-pending",
                IsCritical = false,
                Unit = "Emergency Overflow",
                Location = "Metro General Hospital",
                Description = "Overflow trauma support needed due to increased admissions.",
                PayMultiplier = 1.0,
                DurationHours = 8,
                SpecialtyTag = "Trauma",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-5)
            });

            // Demo decisions for nurse s3 (optional)
            _responses[("req-trauma-accepted", "s3")] = NurseDecision.Accepted;
            _responses[("req-trauma-declined", "s3")] = NurseDecision.Declined;
        }

        public static IEnumerable<SurgeRequest> GetActiveForSpecialty(string specialty)
        {
            
            if (!_surgeActive) return Enumerable.Empty<SurgeRequest>();

            return _requests
                .Where(r => string.Equals(r.SpecialtyTag, specialty, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.CreatedAtUtc);
        }

        public static SurgeRequest? GetById(string id)
        {
            if (!_surgeActive) return null;
            return _requests.FirstOrDefault(r => r.Id == id);
        }

        public static NurseDecision GetDecision(string requestId, string nurseId)
        {
            if (_responses.TryGetValue((requestId, nurseId), out var d))
                return d;
            return NurseDecision.Pending;
        }

        public static void Accept(string requestId, string nurseId)
        {
            _responses[(requestId, nurseId)] = NurseDecision.Accepted;
        }

        public static void Decline(string requestId, string nurseId)
        {
            _responses[(requestId, nurseId)] = NurseDecision.Declined;
        }

        public static SurgeRequest CreateRequest(
            string unit,
            string specialtyTag,
            bool critical,
            string description,
            string location,
            double payMultiplier,
            int durationHours)
        {
            
            _surgeActive = true;

            var r = new SurgeRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Unit = unit,
                SpecialtyTag = specialtyTag,
                IsCritical = critical,
                Description = description,
                Location = location,
                PayMultiplier = payMultiplier,
                DurationHours = durationHours,
                CreatedAtUtc = DateTime.UtcNow
            };

            _requests.Add(r);
            return r;
        }
    }
}