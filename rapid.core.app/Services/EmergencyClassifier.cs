using System.Text.RegularExpressions;
using rapid.core.app.Models;

namespace rapid.core.app.Services
{
    public static class EmergencyClassifier
    {
        private static readonly Dictionary<string, string> ComplaintGroups =
            new(StringComparer.OrdinalIgnoreCase)
        {
            { "Chest pain", "Cardiac" },
            { "Cardiac arrest", "Cardiac" },

            { "Difficulty breathing", "Respiratory" },
            { "Smoke inhalation", "Respiratory" },

            { "Burns - 2nd degree", "Fire" },
            { "Severe bleeding", "Trauma" },
            { "Head injury", "Trauma" },
            { "Fracture", "Trauma" },
            { "Laceration", "Trauma" },

            { "Stroke symptoms", "Neurological" },
            { "Abdominal pain", "Gastrointestinal" },
            { "Allergic reaction", "Allergic" }
        };

        // Complaint -> Group
        public static string GetEmergencyType(string complaint)
            => ComplaintGroups.TryGetValue(complaint ?? "", out var g) ? g : "General Emergency";

        // Map staff specialty to the closest group
        public static string SpecialtyToGroup(string? specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty)) return "General Emergency";
            var s = specialty.ToLowerInvariant();

            if (s.Contains("trauma")) return "Trauma";
            if (s.Contains("burn")) return "Fire";
            if (s.Contains("cardio")) return "Cardiac";
            if (s.Contains("resp") || s.Contains("pulmo") || s.Contains("emergency")) return "Respiratory";
            if (s.Contains("icu") || s.Contains("critical")) return "Critical Care";
            if (s.Contains("pediatric")) return "Pediatric";
            if (s.Contains("neuro")) return "Neurological";
            if (s.Contains("gastro")) return "Gastrointestinal";

            return "General Emergency";
        }
    }
}
