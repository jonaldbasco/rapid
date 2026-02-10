//using Microsoft.AspNetCore.Mvc;
//using rapid.core.app.Models;
//using System.Collections.Concurrent;


//namespace rapid.core.app.Services
//{
//    public static class PatientStore
//    {
//        public static ConcurrentBag<PatientAdmission> Patients { get; } = new();

//        public static void Add(PatientAdmission patient)
//        {
//            Patients.Add(patient);
//        }

//        public static IEnumerable<PatientAdmission> GetAll()
//        {
//            return Patients.OrderByDescending(p => p.AddedAtUtc);
//        }
//    }

//}

using rapid.core.app.Models;
using System.Collections.Concurrent;
using static rapid.core.app.Models.PatientAdmission;

namespace rapid.core.app.Services
{
    public static class PatientStore
    {
        private static readonly ConcurrentDictionary<string, PatientAdmission> _patients = new();

        public static IEnumerable<PatientAdmission> GetAll() =>
            _patients.Values.OrderByDescending(p => p.AddedAtUtc);

        public static IEnumerable<PatientAdmission> GetActive() =>
            _patients.Values.Where(p => p.Status != PatientStatus.Discharged)
                            .OrderByDescending(p => p.AddedAtUtc);

        public static void Add(PatientAdmission p) => _patients[p.Id] = p;

        public static bool TryGet(string id, out PatientAdmission? p) => _patients.TryGetValue(id, out p);

        public static void SetStatus(string id, PatientStatus status, DateTime? whenUtc = null)
        {
            if (TryGet(id, out var p) && p != null)
            {
                p.Status = status;
                if (status == PatientStatus.InRoom)
                    p.AssignedAtUtc ??= whenUtc ?? DateTime.UtcNow;
                if (status == PatientStatus.Discharged)
                    p.DischargedAtUtc ??= whenUtc ?? DateTime.UtcNow;
            }
        }
    }
}
