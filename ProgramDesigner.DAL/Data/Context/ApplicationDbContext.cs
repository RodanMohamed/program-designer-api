using Microsoft.EntityFrameworkCore;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProgramItem> ProgramItems => Set<ProgramItem>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<LearningProgram> LearningPrograms => Set<LearningProgram>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applies every IEntityTypeConfiguration<T> class found in this assembly (DAL).
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
