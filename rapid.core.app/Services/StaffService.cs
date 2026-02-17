using Microsoft.EntityFrameworkCore;
using rapid.core.app.Interface;
using rapid.core.app.Models;
using rapid.core.app.Source;
using System;

namespace rapid.core.app.Services
{
    public class StaffService : IStaffService
    {
        private readonly RapidDBContext _db;

        public StaffService(RapidDBContext db)
        {
            _db = db;
        }

        public async Task<List<StaffClass>> GetStaffAsync()
        {
            return await _db.Staff
                .Where(s => s.Role == "RN" && s.isAvailable == "true" && s.Decision != "Unavailable")
                .ToListAsync();
        }
    }
}
