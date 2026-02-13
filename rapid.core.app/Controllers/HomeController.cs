using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Models;
using rapid.core.app.Services;
using System.Diagnostics;
using System.Linq;
using static rapid.core.app.Models.PatientAdmission;

namespace rapid.core.app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ============================
        // OPS / SURGE DASHBOARD
        // ============================
        public IActionResult Index()
        {
            //Bringdown();

            var patients = PatientStore.GetActive();
            var staff = StaffStore.GetAll();

            var vm = new DashboardViewModel
            {
                Patients = patients.ToList(),
                Staff = staff.ToList()
            };

            return View(vm);
        }

        // ============================
        // NURSE DASHBOARD
        // URL: /Home/Nurse
        // ============================
        [HttpGet("/Home/Nurse")]
        public IActionResult Nurse()
        {
            ViewBag.HideHeader = true;
            // TEMP: simulate logged-in nurse
            var nurse = StaffStore.GetById("s3"); // RN Emily Rodriguez

            if (nurse == null || nurse.Role != "nurse")
                return Unauthorized();

            var requests = SurgeStore.GetActiveForSpecialty(nurse.Specialty)
                .Select(r => new NurseRequestItem
                {
                    Request = r,
                    Decision = SurgeStore.GetDecision(r.Id, nurse.Id),
                    TimeAgoLabel = TimeAgo(r.CreatedAtUtc)
                })
                .ToList();

            var vm = new NurseDashboardViewModel
            {
                Nurse = nurse,
                Requests = requests
            };

            return View("NurseDashboard", vm);
        }

        // ============================
        // NURSE RESPOND VIEW
        // URL: /Home/NurseRespond/{id}
        // ============================
        [HttpGet("/Home/NurseRespond/{id}")]
        public IActionResult NurseRespond(string id)
        {
            ViewBag.HideHeader = true;
            var nurse = StaffStore.GetById("s3");
            if (nurse == null) return Unauthorized();

            var req = SurgeStore.GetById(id);
            if (req == null) return NotFound();

            var vm = new NurseRespondViewModel
            {
                Nurse = nurse,
                Request = req,
                CurrentDecision = SurgeStore.GetDecision(req.Id, nurse.Id)
            };

            return View("NurseRespond", vm);
        }

        // ============================
        // NURSE DECISIONS
        // ============================
        [HttpPost("/Home/NurseAccept")]
        public IActionResult NurseAccept(string requestId)
        {
            var nurse = StaffStore.GetById("s3");
            if (nurse == null) return Unauthorized();

            SurgeStore.Accept(requestId, nurse.Id);

            StaffCallDecisionStore.SetAccepted(nurse.Id);

            // Optional: reflect nurse is now available
            StaffStore.SetStatus(nurse.Id, "available");

            return RedirectToAction("Nurse");
        }

        [HttpPost("/Home/NurseDecline")]
        public IActionResult NurseDecline(string requestId)
        {
            var nurse = StaffStore.GetById("s3");
            if (nurse == null) return Unauthorized();

            SurgeStore.Decline(requestId, nurse.Id);

            StaffCallDecisionStore.SetDeclined(nurse.Id);

            return RedirectToAction("Nurse");
        }

        // ============================
        // HELPER
        // ============================
        private static string TimeAgo(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;

            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }

        // ============================
        // EXISTING PATIENT / SIM LOGIC
        // ============================
        private static readonly string[] Names = {
            "John Smith", "Maria Garcia", "James Johnson", "Sarah Williams", "Michael Brown",
            "Emily Davis", "David Miller", "Jennifer Wilson", "Robert Moore", "Lisa Taylor"
        };

        private static readonly string[] Complaints = {
            "Chest pain", "Difficulty breathing", "Burns - 2nd degree", "Smoke inhalation",
            "Fracture", "Laceration", "Abdominal pain", "Head injury", "Cardiac arrest",
            "Allergic reaction", "Stroke symptoms", "Severe bleeding"
        };

        private PatientAdmission GenerateRandomPatient()
        {
            var patientName = Names[Random.Shared.Next(Names.Length)];
            var complaint = Complaints[Random.Shared.Next(Complaints.Length)];

            var acuityLevel = AcuityCatalog.PickAcuityForComplaint(complaint);
            var (label, className) = AcuityCatalog.Present(acuityLevel);

            return new PatientAdmission
            {
                Id = AcuityCatalog.GenerateSixDigitId(),
                PatientName = patientName,
                Complaint = complaint,
                AcuityLevel = acuityLevel,
                AcuityLabel = label,
                AcuityClassName = className,
                AddedAtUtc = DateTime.UtcNow,
                Status = PatientStatus.Waiting
            };
        }

        public IActionResult PatientAdmission()
        {
            return View(GenerateRandomPatient());
        }

        [HttpGet]
        public IActionResult AddPatientCard()
        {
            var patient = GenerateRandomPatient();

            if (RoomStore.TryAssign(patient.Id))
            {
                patient.Status = PatientStatus.InRoom;
                patient.AssignedAtUtc = DateTime.UtcNow;
            }

            PatientStore.Add(patient);
            return PartialView("_PatientCard", patient);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // ============================
        // STATS / SURGE ENDPOINTS
        // ============================
        [HttpGet]
        public IActionResult GetStats()
        {
            return Json(StatsService.Compute());
        }

        [HttpPost]
        public IActionResult ActivateStaff([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
                return BadRequest("No staff ids were provided.");

            StaffResponseStore.MarkNotified(ids);

            int changed = 0;
            foreach (var id in ids)
            {
                if (StaffStore.SetStatus(id, "available"))
                    changed++;
            }

            return Ok(new { ok = true, notified = ids.Length, activated = changed });
        }

        [HttpPost]
        public IActionResult StaffDecision(string id, string decision)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Missing id.");

            decision = (decision ?? "").ToLowerInvariant();
            if (decision == "confirmed")
            {
                StaffDecisionStore.Confirm(id);
                StaffStore.SetStatus(id, "on_duty");
            }
            else if (decision == "declined")
            {
                StaffDecisionStore.Decline(id);
            }
            else
            {
                return BadRequest("Invalid decision.");
            }

            return Ok(new { ok = true, id, decision });
        }

        // ============================
        // TRIGGER SURGE
        // ============================
        [HttpPost]
        public IActionResult TriggerSurge(int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                var patient = GenerateRandomPatient();

                if (RoomStore.TryAssign(patient.Id))
                {
                    patient.Status = PatientStatus.InRoom;
                    patient.AssignedAtUtc = DateTime.UtcNow;
                }

                PatientStore.Add(patient);
            }

            SurgeStore.ActivateSurge();
            SurgeStore.CreateRequest(
                unit: "Trauma Bay",
                specialtyTag: "Trauma",
                critical: true,
                description: "Mass casualty event. Trauma support urgently needed.",
                location: "Metro General Hospital - Trauma Bay",
                payMultiplier: 1.5,
                durationHours: 4
            );

            return Ok(new
            {
                ok = true,
                added = count,
                surgeActive = SurgeStore.IsSurgeActive()
            });

        }

        [HttpGet]
        public IActionResult GetSurgeStatus()
        {
            return Json(new
            {
                active = SurgeStore.IsSurgeActive(),
            });
        }


        [HttpPost("/Home/ResetSurge")]
        public IActionResult ResetSurge()
        {
            SurgeStore.ResetSurge();

            // optional but recommended
            StaffStore.ResetStatusesToInitial();

            return Ok(new { ok = true, active = SurgeStore.IsSurgeActive() });
        }


        [HttpGet("/Home/NurseRequests")]
        public IActionResult NurseRequests()
        {
            var nurse = StaffStore.GetById("s3");
            if (nurse == null) return Unauthorized();

            var reqs = SurgeStore.GetActiveForSpecialty(nurse.Specialty).ToList();
            return Json(new { count = reqs.Count });
        }

        [HttpGet]
        public IActionResult NurseRequestsPartial()
        {
            var nurse = StaffStore.GetById("s3");
            if (nurse == null) return Unauthorized();

            var requests = SurgeStore.GetActiveForSpecialty(nurse.Specialty)
                .Select(r => new NurseRequestItem
                {
                    Request = r,
                    Decision = SurgeStore.GetDecision(r.Id, nurse.Id),
                    TimeAgoLabel = TimeAgo(r.CreatedAtUtc)
                })
                .ToList();

            return PartialView("_NurseRequestList", requests);
        }

        [HttpGet]
        public IActionResult GetPatientCounts()
        {
            var patients = PatientStore.GetActive();

            var inpatient = patients.Count(p => p.Status == PatientStatus.InRoom);
            var incoming = patients.Count(p => p.Status == PatientStatus.Waiting);

            return Json(new { inpatient, incoming });
        }

    }
}