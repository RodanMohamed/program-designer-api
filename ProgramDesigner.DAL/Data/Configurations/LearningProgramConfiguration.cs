using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Configurations
{
    public class LearningProgramConfiguration : IEntityTypeConfiguration<LearningProgram>
    {
        public void Configure(EntityTypeBuilder<LearningProgram> builder)
        {
            builder.ToTable("LearningPrograms");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Restrict: prevents deleting a Group directly if it's still
            // referenced as a program's root, avoiding accidental orphaned programs.
            builder.HasOne(p => p.RootGroup)
                .WithMany()
                .HasForeignKey(p => p.RootGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
