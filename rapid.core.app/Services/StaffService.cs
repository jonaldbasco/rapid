using Microsoft.EntityFrameworkCore;
using rapid.core.app.Agents;
using rapid.core.app.Interface;
using rapid.core.app.Models;
using rapid.core.app.Source;
using System;

namespace rapid.core.app.Services
{
    public class StaffService : IStaffService
    {
        private readonly RapidDBContext _db;
        private readonly AnalyticsAgent _analytics;

        public StaffService(RapidDBContext db, AnalyticsAgent analytics)
        {
            _db = db;
            _analytics = analytics;
        }

        public async Task<List<StaffMember>> GetStaffAsync()
        {
            var staffs = await _db.Staff
                .Where(s => s.Role == "RN"
                        && s.isAvailable == "true"
                        && s.Decision != "Unavailable")
                .ToListAsync();

            var analysisTasks = new Dictionary<string, Task<string>>();

            foreach (var nurse in staffs)
            {
                var cached = _db.NegotiationSummarys
                    .FirstOrDefault(n => n.StaffId == nurse.Id);

                if (cached != null)
                {
                    // wrap cached summary as completed task
                    analysisTasks[nurse.Id] = Task.FromResult(cached.Summary);
                }
                else
                {
                    // call analytics AI
                    analysisTasks[nurse.Id] = _analytics.GetAnalyzedResult(nurse.Id);
                }
            }

            // wait for all analytics calls
            await Task.WhenAll(analysisTasks.Values);

            return staffs.Select(nurse => new StaffMember
            {
                Id = nurse.Id,
                Name = nurse.Name,
                Role = nurse.Role,
                Unit = nurse.Unit,
                Certification = nurse.Certification,
                Status = nurse.Status,
                Decision = nurse.Decision,
                ResponseRate = nurse.ResponseRate,
                DistanceMinutes = nurse.DistanceMinutes,
                isAvailable = nurse.isAvailable,
                UpdatedAtTicks = nurse.UpdatedAtTicks,
                Summary = analysisTasks[nurse.Id].Result
            }).ToList();
        }
    }
}
