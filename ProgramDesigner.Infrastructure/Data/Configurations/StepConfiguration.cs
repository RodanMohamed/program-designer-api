using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Data.Configurations
{
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            builder.Property(s => s.StepType).IsRequired().HasMaxLength(100);
        }
    }
}
