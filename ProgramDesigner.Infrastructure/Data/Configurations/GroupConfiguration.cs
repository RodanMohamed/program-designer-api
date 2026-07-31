using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            // GroupRule is a Value Object (RuleType + ChoiceCount) — stored as two plain
            // columns on the same ProgramItems row (owned type), not a separate table.
            builder.OwnsOne(g => g.Rule, rule =>
            {
                rule.Property(r => r.RuleType).HasColumnName("RuleType");
                rule.Property(r => r.ChoiceCount).HasColumnName("ChoiceCount");
            });
        }
    }
}
