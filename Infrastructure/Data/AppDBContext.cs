using System;
using System.Collections.Generic;
using rapid.core.app.mode

namespace Infrastructure.Data
{
    public class AppDBContext
    {
        public DbSet<HospitalUnit> Units => Set<HospitalUnit>();
        public DbSet<Staff> Staff => Set<Staff>();
        public DbSet<StaffingForecast> Forecasts => Set<StaffingForecast>();
        public DbSet<Negotiation> Negotiations => Set<Negotiation>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}
