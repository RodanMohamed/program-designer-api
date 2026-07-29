using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            // Store the enum as a readable string (e.g. "InOrder", "Choice")
            // instead of an opaque integer, for easier manual DB inspection.
            builder.Property(g => g.RuleType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Nullable: only meaningful when RuleType == Choice.
            builder.Property(g => g.ChoiceCount)
                .IsRequired(false);
        }
    }
}
