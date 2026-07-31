using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Data.Configurations
{
    public class LearningProgramConfiguration : IEntityTypeConfiguration<LearningProgram>
    {
        public void Configure(EntityTypeBuilder<LearningProgram> builder)
        {
            builder.ToTable("LearningPrograms");
            builder.HasKey(p => p.Id);

            builder.UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.CreatedAt);

            builder.HasOne(p => p.RootGroup)
                .WithOne()
                .HasForeignKey<LearningProgram>("RootGroupId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
