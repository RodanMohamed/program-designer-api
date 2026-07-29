using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Configurations
{
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            // No ToTable() call here on purpose: Step shares the "ProgramItems" table
            // via TPH, configured already in ProgramItemConfiguration.
            builder.Property(s => s.StepType)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
