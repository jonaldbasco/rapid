using System.Security.Cryptography;
using rapid.core.app.Models;

namespace rapid.core.app.Services
{
    public static class AcuityCatalog
    {
        // Presentation mapping for each ESI level
        private static readonly Dictionary<int, (string Label, string ClassName)> AcuityPresentation = new()
        {
            { 1, ("ESI-1", "bg-surge-critical text-white")  },
            { 2, ("ESI-2", "bg-surge-high text-black")      },
            { 3, ("ESI-3", "bg-surge-moderate text-black")  },
            { 4, ("ESI-4", "bg-surge-low text-white")       },
            { 5, ("ESI-5", "bg-surge-normal text-black")    },
        };

        // For each complaint, define likely acuity levels and their weights (sum arbitrary)
        private static readonly Dictionary<string, (int Level, int Weight)[]> ComplaintAcuityWeights = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Cardiac arrest"] = new[] { (1, 70), (2, 25), (3, 5) },
            ["Stroke symptoms"] = new[] { (2, 60), (1, 20), (3, 20) },
            ["Chest pain"] = new[] { (2, 50), (3, 40), (1, 10) },
            ["Difficulty breathing"] = new[] { (2, 55), (3, 35), (1, 10) },
            ["Severe bleeding"] = new[] { (2, 50), (3, 30), (1, 20) },
            ["Head injury"] = new[] { (3, 50), (2, 35), (4, 15) },
            ["Fracture"] = new[] { (3, 60), (4, 30), (2, 10) },
            ["Laceration"] = new[] { (4, 60), (3, 35), (5, 5) },
            ["Abdominal pain"] = new[] { (3, 60), (4, 35), (2, 5) },
            ["Allergic reaction"] = new[] { (3, 50), (2, 40), (4, 10) },
            ["Burns - 2nd degree"] = new[] { (3, 55), (2, 35), (4, 10) },
            ["Smoke inhalation"] = new[] { (2, 60), (3, 30), (1, 10) },
        };

        // Fallback uniform distribution if a complaint is missing
        private static readonly (int Level, int Weight)[] DefaultWeights =
            new[] { (1, 5), (2, 15), (3, 45), (4, 25), (5, 10) };

        public static int PickAcuityForComplaint(string complaint)
        {
            var weights = ComplaintAcuityWeights.TryGetValue(complaint ?? string.Empty, out var w)
                ? w : DefaultWeights;
            var total = weights.Sum(x => x.Weight);
            var roll = Random.Shared.Next(1, total + 1);
            var acc = 0;

            foreach (var (level, weight) in weights)
            {
                acc += weight;
                if (roll <= acc) return level;
            }
            return weights.Last().Level;
        }

        public static (string Label, string ClassName) Present(int acuityLevel)
        {
            if (AcuityPresentation.TryGetValue(acuityLevel, out var tuple))
                return tuple;
            // Fallback safety
            return ("ESI-3", "bg-surge-moderate text-black");
        }

        public static string GenerateSixDigitId()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes);
            return (value % 1_000_000).ToString("D6"); // "000000".."999999"
        }

    }
}