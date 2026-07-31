using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<LearningProgram> LearningPrograms => Set<LearningProgram>();
        public DbSet<ProgramItem> ProgramItems => Set<ProgramItem>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
