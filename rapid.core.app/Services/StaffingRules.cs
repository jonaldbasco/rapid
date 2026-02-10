using rapid.core.app.Models;

namespace rapid.core.app.Services
{
    public static class StaffingRules
    {
        // Recommended staffing based on emergency type + level
        public static readonly Dictionary<(string emergency, string level), StaffingRequirement> Rules =
            new()
            {
                { ("Trauma",    "alert"),    new StaffingRequirement { Nurses = 1, Doctors = 0 } },
                { ("Trauma",    "critical"), new StaffingRequirement { Nurses = 2, Doctors = 1 } },
                { ("Trauma",    "surge"),    new StaffingRequirement { Nurses = 3, Doctors = 2 } },

                { ("Fire",      "alert"),    new StaffingRequirement { Nurses = 1, Doctors = 0 } },
                { ("Fire",      "critical"), new StaffingRequirement { Nurses = 2, Doctors = 1 } },
                { ("Fire",      "surge"),    new StaffingRequirement { Nurses = 3, Doctors = 1 } },

                { ("Cardiac",   "alert"),    new StaffingRequirement { Nurses = 1, Doctors = 1 } },
                { ("Cardiac",   "critical"), new StaffingRequirement { Nurses = 2, Doctors = 1 } },
                { ("Cardiac",   "surge"),    new StaffingRequirement { Nurses = 3, Doctors = 2 } },

                { ("Respiratory","alert"),   new StaffingRequirement { Nurses = 1, Doctors = 1 } },
                { ("Respiratory","critical"),new StaffingRequirement { Nurses = 1, Doctors = 2 } },
                { ("Respiratory","surge"),   new StaffingRequirement { Nurses = 2, Doctors = 2 } }
            };

        public static StaffingRequirement Get(string emergency, string level)
        {
            if (Rules.TryGetValue((emergency, level), out var req))
                return req;

            // fallback for unknown emergencies
            return new StaffingRequirement { Nurses = 1, Doctors = 0 };
        }
    }

    public class StaffingRequirement
    {
        public int Nurses { get; set; }
        public int Doctors { get; set; }
    }
}