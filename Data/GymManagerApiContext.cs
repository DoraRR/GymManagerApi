using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GymManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GymManagerApi.Models;

namespace GymManagerApi.Data
{
    public class GymManagerApiContext : IdentityDbContext<IdentityUser>

    {
        public GymManagerApiContext (DbContextOptions<GymManagerApiContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<GymManager.Models.Workout> Workout { get; set; } = default!;

        public DbSet<GymManager.Models.WorkoutPlan> WorkoutPlan { get; set; }

        public DbSet<GymManager.Models.GymUser> GymUser { get; set; }

        public DbSet<GymManager.Models.Trainer> Trainer { get; set; }
        public DbSet<UserApiKey> UserApiKeys { get; set; }

    }
}
