using System.Linq;
using rapid.core.app.Models;

namespace rapid.core.app.Services
{
    public static class StaffStore
    {
        private static readonly List<StaffMember> _staff = new()
        {
            new StaffMember {
                Id = "s1",
                Name = "Dr. Sarah Chen",
                Role = "doctor",
                Specialty = "Emergency Medicine",
                Certifications = new() { "ACLS", "PALS", "Trauma" },
                Status = "off_duty",
                ResponseRate = 0.92,
                DistanceMinutes = 15
            },

            new StaffMember {
                Id = "s2",
                Name = "Dr. James Wilson",
                Role = "doctor",
                Specialty = "Critical Care",
                Certifications = new() { "ACLS", "FCCS" },
                Status = "off_duty",
                ResponseRate = 0.88,
                DistanceMinutes = 22
            },

            new StaffMember {
                Id = "s3",
                Name = "RN Emily Rodriguez",
                Role = "nurse",
                Specialty = "Trauma",
                Certifications = new() { "BLS", "TNCC" },
                Status = "off_duty",
                ResponseRate = 0.95,
                DistanceMinutes = 8
            },

            new StaffMember {
                Id = "s4",
                Name = "RN Michael Park",
                Role = "nurse",
                Specialty = "Burn Unit",
                Certifications = new() { "BLS", "Burn Care" },
                Status = "off_duty",
                ResponseRate = 0.78,
                DistanceMinutes = 30
            },

            new StaffMember {
                Id = "s5",
                Name = "RN Amanda Foster",
                Role = "nurse",
                Specialty = "ICU",
                Certifications = new() { "CCRN", "BLS" },
                Status = "off_duty",
                ResponseRate = 0.91,
                DistanceMinutes = 12
            },

            new StaffMember {
                Id = "s6",
                Name = "Dr. Robert Kim",
                Role = "specialist",
                Specialty = "Cardiology",
                Certifications = new() { "ACLS", "Board Certified" },
                Status = "off_duty",
                ResponseRate = 0.85,
                DistanceMinutes = 25
            },

            new StaffMember {
                Id = "s7",
                Name = "RN Lisa Thompson",
                Role = "nurse",
                Specialty = "Emergency",
                Certifications = new() { "CEN", "BLS" },
                Status = "available",
                ResponseRate = 0.89,
                DistanceMinutes = 5
            },

            new StaffMember {
                Id = "s8",
                Name = "RN David Chang",
                Role = "nurse",
                Specialty = "Pediatric",
                Certifications = new() { "PALS", "BLS" },
                Status = "on_duty",
                ResponseRate = 0.94,
                DistanceMinutes = 0
            },

            new StaffMember {
                Id = "s9",
                Name = "RN Carlos Mendoza",
                Role = "nurse",
                Specialty = "Emergency",
                Certifications = new() { "BLS", "CEN" },
                Status = "off_duty",
                ResponseRate = 0.87,
                DistanceMinutes = 18
            },

            new StaffMember {
                Id = "s10",
                Name = "RN Julia Reyes",
                Role = "nurse",
                Specialty = "ICU",
                Certifications = new() { "CCRN", "BLS" },
                Status = "off_duty",
                ResponseRate = 0.93,
                DistanceMinutes = 10
            }
        };


        private static readonly Dictionary<string, string> _initialStatus =
            _staff.ToDictionary(s => s.Id, s => s.Status);

        public static IEnumerable<StaffMember> GetAll() => _staff;

        public static IEnumerable<StaffMember> GetOffDuty() =>
            _staff.Where(s => s.Status == "off_duty");

        public static IEnumerable<StaffMember> GetAvailable() =>
            _staff.Where(s => s.Status == "available" || s.Status == "on_duty");

        // ✅ Keep ONLY ONE GetById method (fixes CS0111)
        public static StaffMember? GetById(string id) =>
            _staff.FirstOrDefault(s => s.Id == id);

        // Controlled status change
        public static bool SetStatus(string id, string status)
        {
            var staff = _staff.FirstOrDefault(s => s.Id == id);
            if (staff == null) return false;

            staff.Status = status;
            return true;
        }

        public static StaffMember? ActivateNearest(string role)
        {
            var pick = GetOffDuty()
                .Where(s => s.Role == role)
                .OrderByDescending(s => s.ResponseRate)
                .ThenBy(s => s.DistanceMinutes)
                .FirstOrDefault();

            if (pick != null)
                pick.Status = "available";

            return pick;
        }

        public static void ResetStatusesToInitial()
        {
            foreach (var s in _staff)
            {
                if (_initialStatus.TryGetValue(s.Id, out var status))
                    s.Status = status;
            }
        }

    }
}