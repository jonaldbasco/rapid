using rapid.core.app.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace rapid.core.app.Source
{
    public class RapidDBContext(DbContextOptions<RapidDBContext> options) : DbContext(options)
    {
        public DbSet<UserClass> User { get; set; }
        public DbSet<UserDetailsClass> UserDetails { get; set; }
        public DbSet<UnitsClass> Units { get; set; }
        public DbSet<StaffClass> Staff { get; set; }
        public DbSet<SummaryClass> Summary { get; set; }
        public DbSet<NegotiationClass> Negotiations => Set<NegotiationClass>();
        public DbSet<NegotiationMessage> NegotiationMessages => Set<NegotiationMessage>();
        public DbSet<SurgeRequest> SurgeRequests => Set<SurgeRequest>();
        
    }
}
