using Microsoft.AspNetCore.Mvc;
using rapid.core.app.Models;
using rapid.core.app.Services;
using System.Diagnostics;
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

        public IActionResult Index()
        {
            Bringdown();

            var patients = PatientStore.GetActive();
            var staff = StaffStore.GetAll();

            var vm = new DashboardViewModel
            {
                Patients = patients.ToList(),
                Staff = staff.ToList()
            };

            return View(vm);

        }

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


        public void Bringdown()
        {

        }
        public IActionResult PatientAdmission()
        {
            var patientAdmission = GenerateRandomPatient();
            return View(patientAdmission);
        }


        [HttpGet]
        public IActionResult AddPatientCard()
        {
            var patient = GenerateRandomPatient();

            // Assign room if available; else remain Waiting
            if (RoomStore.TryAssign(patient.Id))
            {
                patient.Status = PatientStatus.InRoom;
                patient.AssignedAtUtc = DateTime.UtcNow;
            }

            PatientStore.Add(patient);
            return PartialView("_PatientCard", patient);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // === STATS ENDPOINT ===
        [HttpGet]
        public IActionResult GetStats()
        {
            var stats = StatsService.Compute();
            return Json(stats);
        }


        // === SEED endpoint (for simulation control) ===
        // Examples:
        //   /Home/Seed?rooms=8&waiting=12&inRoom=6
        // Seeds random patients to reach desired counts.
        [HttpPost]
        public IActionResult Seed(int rooms = 10, int waiting = 8, int inRoom = 4)
        {
            RoomStore.Configure(rooms);

            // Simple reset: for a quick simulation, we can clear and reseed.
            // If you don't want to clear, comment out the following two lines.
            // (We didn't include a clear in PatientStore; add one if you want full reset.)
            // ---- Optional reset (uncomment if you add Clear method) ----
            // PatientStore.Clear();
            // (For now, we’ll just add on top.)

            // Fill rooms first
            for (int i = 0; i < inRoom; i++)
            {
                var p = GenerateRandomPatient();
                p.Status = PatientStatus.InRoom;
                p.AssignedAtUtc = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(5, 45));
                PatientStore.Add(p);
                RoomStore.TryAssign(p.Id);
            }

            // Then seed waiting
            for (int i = 0; i < waiting; i++)
            {
                var p = GenerateRandomPatient();
                p.Status = PatientStatus.Waiting;
                p.AddedAtUtc = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 60));
                PatientStore.Add(p);
            }

            return Ok(new { ok = true, rooms, waiting, inRoom });
        }

        [HttpPost]
        public IActionResult SetRooms(int rooms)
        {
            if (rooms < 0)
                return BadRequest("Room count cannot be negative.");

            RoomStore.Configure(rooms);

            return Ok(new
            {
                ok = true,
                rooms = RoomStore.TotalRooms,
                available = RoomStore.AvailableCount,
                occupied = RoomStore.OccupiedCount
            });
        }

        [HttpPost]
        public IActionResult ActivateStaff([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0) return BadRequest("No staff ids were provided.");
            int changed = 0;
            foreach (var id in ids)
                if (StaffStore.SetStatus(id, "available")) changed++;

            return Ok(new { ok = true, activated = changed });
        }

    }
}
