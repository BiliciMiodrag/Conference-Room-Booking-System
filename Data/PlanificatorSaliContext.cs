using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanificatorSali.Models;
using PlanificatorSali.Models.Configuration;

namespace PlanificatorSali.Data
{
    public class PlanificatorSaliContext: IdentityDbContext <ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PlanificatorSaliContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
        }
        public DbSet<Evenimente> Evenimente { get; set; }
        public DbSet<Sala> Sala { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }


    }
}
