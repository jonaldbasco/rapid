using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Models;
using static rapid.core.app.Models.PatientAdmission;

namespace rapid.core.app.Services
{
    public static class StatsService
    {
        // Tunable weights by acuity (ESI-1 critical → higher weight)
        // These are multiplicative “importance” weights for waiting-time approximation.
        private static readonly Dictionary<int, double> AcuityWeights = new()
        {
            { 1, 2.0 },   // critical
            { 2, 1.6 },
            { 3, 1.2 },
            { 4, 1.0 },
            { 5, 0.8 },
        };

        public static object Compute()
        {
            var now = DateTime.UtcNow;
            var active = PatientStore.GetActive().ToList();


            // Threshold: default to room capacity
            int threshold = RoomStore.TotalRooms;

            // Patients: all active (Waiting + InRoom)
            var patients = active.Count;

            // Waiting: those not in room
            var waiting = active.Count(p => p.Status == PatientStatus.Waiting);

            // Surge gauge percentage
            double gauge = threshold == 0 ? 0 :
                (active.Count / (double)threshold) * 100;

            // Determine dominant emergency type
            string emergencyType = "Normal Operations";
            if (active.Count > 0)
            {
                emergencyType = active
                    .GroupBy(p => EmergencyClassifier.GetEmergencyType(p.Complaint))
                    .OrderByDescending(g => g.Count())
                    .First().Key;
            }

            string surgeLevel;

            if (gauge >= 100)
                surgeLevel = "surge";
            else if (gauge >= 80)
                surgeLevel = "critical";
            else if (gauge >= 60)
                surgeLevel = "alert";
            else
                surgeLevel = "normal";

            StaffNotificationStore.ResetIfNewSurge(surgeLevel);

            // Velocity: admissions in the last 15 minutes → per hour
            var windowStart = now.AddMinutes(-15);
            var admissionsLast15 = active.Count(p => p.AddedAtUtc >= windowStart);
            var velocityPerHour = admissionsLast15 * (60.0 / 15.0); // scale to per-hour

            // Wait Time: acuity-weighted average wait among waiting patients
            var waits = active.Where(p => p.Status == PatientStatus.Waiting)
                              .Select(p =>
                              {
                                  var minutes = (now - p.AddedAtUtc).TotalMinutes;
                                  var weight = AcuityWeights.TryGetValue(p.AcuityLevel, out var w) ? w : 1.0;
                                  return (minutes, weight);
                              })
                              .ToList();

            double avgWeightedWait = 0;
            if (waits.Count > 0)
            {
                var weightedSum = waits.Sum(x => x.minutes * x.weight);
                var totalWeights = waits.Sum(x => x.weight);
                avgWeightedWait = totalWeights > 0 ? (weightedSum / totalWeights) : 0;
            }

            var staffingReq = StaffingRules.Get(emergencyType, surgeLevel);

            // Count available staff by role
            var availableDoctors = StaffStore.GetAvailable().Count(s => s.Role == "doctor");
            var availableNurses = StaffStore.GetAvailable().Count(s => s.Role == "nurse");

            // Suggestions
            var needDoctors = Math.Max(0, staffingReq.Doctors - availableDoctors);
            var needNurses = Math.Max(0, staffingReq.Nurses - availableNurses);

            // Build suggestion lists (off-duty, best responders, nearest)
            IEnumerable<StaffMember> suggestedDoctors = Enumerable.Empty<StaffMember>();
            IEnumerable<StaffMember> suggestedNurses = Enumerable.Empty<StaffMember>();

            // Only suggest when level is alert/critical/surge (skip normal)
            if (surgeLevel != "normal")
            {
                var offDuty = StaffStore.GetOffDuty().ToList();

                suggestedDoctors = offDuty
                    .Where(s => s.Role == "doctor")
                    .OrderByDescending(s => s.ResponseRate)
                    .ThenBy(s => s.DistanceMinutes)
                    .Take(needDoctors);

                suggestedNurses = offDuty
                    .Where(s => s.Role == "nurse")
                    .OrderByDescending(s => s.ResponseRate)
                    .ThenBy(s => s.DistanceMinutes)
                    .Take(needNurses);
            }


            // ===== Staffing Requirements by Emergency Group =====

            // Choose source for requirement calculation (Waiting by default)
            var waitingPatients = active.Where(p => p.Status == PatientStatus.Waiting).ToList();

            // Surge multipliers (same as before)
            double surgeMult = surgeLevel switch
            {
                "surge" => 1.6,
                "critical" => 1.3,
                "alert" => 1.1,
                _ => 1.0
            };

            // Group waiting patients by emergency type (Trauma, Fire, Cardiac, ...)
            var grouped = waitingPatients
                .GroupBy(p => EmergencyClassifier.GetEmergencyType(p.Complaint))
                .ToList();

            // ---- Weights (tune anytime) ----
            // Nurse “units” per patient by acuity (heavier than doctors)
            var nurseAcuityWeights = new Dictionary<int, double> {
    { 1, 1.6 }, { 2, 1.2 }, { 3, 0.8 }, { 4, 0.5 }, { 5, 0.4 }
};
            // Doctor “units” per patient by acuity (lighter than nurses)
            var doctorAcuityWeights = new Dictionary<int, double> {
    { 1, 1.0 }, { 2, 0.7 }, { 3, 0.4 }, { 4, 0.2 }, { 5, 0.1 }
};

            // Small helper to compute requirement items per role
            List<object> BuildReqItems(
                string role,
                Dictionary<int, double> weights,
                Func<Models.StaffMember, bool> rolePredicate)
            {
                // availability (ready staff) grouped by specialty→group
                var readyByGroup = StaffStore.GetAvailable()
                    .Where(rolePredicate)
                    .GroupBy(s => EmergencyClassifier.SpecialtyToGroup(s.Specialty))
                    .ToDictionary(g => g.Key, g => g.Count());

                var items = new List<object>();

                foreach (var g in grouped)
                {
                    double units = g.Sum(p => weights.TryGetValue(p.AcuityLevel, out var w) ? w : 0.5);
                    int required = Math.Max(1, (int)Math.Ceiling(units * surgeMult));

                    readyByGroup.TryGetValue(g.Key, out var filled);
                    int safeRequired = Math.Max(required, 1);
                    int clampedFilled = Math.Max(filled, 0);
                    double coverage = safeRequired == 0 ? 0 : (double)clampedFilled / safeRequired * 100;

                    items.Add(new
                    {
                        group = g.Key,
                        role = role,
                        required = safeRequired,
                        filled = clampedFilled,
                        coverage = Math.Round(Math.Min(100, coverage), 0)
                    });
                }

                // Sort biggest gaps first
                return items
                    .OrderByDescending(x => (int)x.GetType().GetProperty("required")!.GetValue(x)!)
                    .ToList();
            }

            // Build nurse/doctor items
            var nurseItems = BuildReqItems("nurse", nurseAcuityWeights, s => s.Role == "nurse");
            var doctorItems = BuildReqItems("doctor", doctorAcuityWeights, s => s.Role == "doctor");

            // Totals
            int nurseTotalReq = nurseItems.Sum(x => (int)x.GetType().GetProperty("required")!.GetValue(x)!);
            int nurseTotalFill = nurseItems.Sum(x => (int)x.GetType().GetProperty("filled")!.GetValue(x)!);
            int doctorTotalReq = doctorItems.Sum(x => (int)x.GetType().GetProperty("required")!.GetValue(x)!);
            int doctorTotalFill = doctorItems.Sum(x => (int)x.GetType().GetProperty("filled")!.GetValue(x)!);

            // Nurse gap (how many still needed)
            var nursesNeeded = Math.Max(0, nurseTotalReq - nurseTotalFill);

            // Count notified nurses (only nurses)
            var notifiedIds = StaffNotificationStore.AllNotifiedIds();
            var notifiedNursesCount = StaffStore.GetAll()
                .Where(s => s.Role == "nurse" && notifiedIds.Contains(s.Id))
                .Count();

            // percent notified vs needed (if needed=0 -> 100%)
            double staffReqPercent = nursesNeeded == 0 ? 100 : (double)notifiedNursesCount / nursesNeeded * 100;
            staffReqPercent = Math.Max(0, Math.Min(100, staffReqPercent));

            // ===== Nurse response feed (for staffReqFeed) =====

            var nurseIds = StaffStore.GetAll()
                .Where(s => s.Role == "nurse")
                .Select(s => s.Id)
                .ToList();

            var nurseResponses = StaffStore.GetAll()
                .Where(s => s.Role == "nurse")
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Specialty,
                    s.DistanceMinutes,
                    s.ResponseRate,
                    Response = StaffResponseStore.GetResponse(s.Id) // none | notified | accepted | declined
                })
                .ToList();

            int acceptedCount = nurseResponses.Count(n => n.Response == "accepted");
            int notifiedCount = nurseResponses.Count(n => n.Response == "notified");

            // progress = accepted / needed
            double reqPercent =
                nursesNeeded == 0 ? 100 :
                Math.Min(100, (double)acceptedCount / nursesNeeded * 100);

            // ===== Existing suggested staff (keep what you already have) =====
            // Assuming you've built: suggestedNurses, suggestedDoctors (IEnumerable<StaffMember>)

            var nursePool = StaffStore.GetAll()
            .Where(s => s.Role == "nurse")
            .Select(s => new {
                s.Id,
                s.Name,
                s.Specialty,
                s.DistanceMinutes,
                s.ResponseRate,
                s.Status,
                Decision = StaffDecisionStore.Get(s.Id) // pending|confirmed|declined
            })
            .ToList();

            var confirmedNurses = nursePool.Where(n => n.Decision == "confirmed").ToList();
            StaffDecisionStore.SeedConfirmed("s7");


            return new
            {
                patients,
                waiting,
                avgWait = Math.Round(avgWeightedWait, 1),
                velocity = Math.Round(velocityPerHour, 1),
                rooms = new
                {
                    total = RoomStore.TotalRooms,
                    occupied = RoomStore.OccupiedCount,
                    available = RoomStore.AvailableCount
                },

                surge = new
                {
                    percent = Math.Min(100, Math.Round(gauge, 0)),
                    threshold,
                    emergency = emergencyType,
                    level = surgeLevel
                },


                staffing = new
                {
                    emergency = emergencyType,
                    level = surgeLevel,
                    need = new { nurses = needNurses, doctors = needDoctors },
                    available = new { nurses = availableNurses, doctors = availableDoctors }
                },

                suggestedStaff = new
                {
                    nurses = suggestedNurses.Select(s => new { s.Id, s.Name, s.DistanceMinutes, s.ResponseRate }),
                    doctors = suggestedDoctors.Select(s => new { s.Id, s.Name, s.DistanceMinutes, s.ResponseRate })
                },

                staffingRequirementsNurses = new
                {
                    role = "nurse",
                    totalRequired = nurseTotalReq,
                    totalFilled = nurseTotalFill,
                    items = nurseItems
                },

                staffingRequirementsDoctors = new
                {
                    role = "doctor",
                    totalRequired = doctorTotalReq,
                    totalFilled = doctorTotalFill,
                    items = doctorItems
                },
                staffReqFeed = new
                {
                    role = "nurse",
                    required = nurseTotalReq,
                    filled = nurseTotalFill,
                    needed = nursesNeeded,

                    accepted = acceptedCount,
                    notified = notifiedCount,
                    percent = Math.Round(reqPercent, 0),

                    nurses = nurseResponses
                },

                staffingUI = new
                {
                    nursePool,
                    confirmedNurses
                }

            };
        }
    }
}
