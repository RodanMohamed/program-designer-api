using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Data.Configurations
{
    public class ProgramItemConfiguration : IEntityTypeConfiguration<ProgramItem>
    {
        public void Configure(EntityTypeBuilder<ProgramItem> builder)
        {
            builder.ToTable("ProgramItems");
            builder.HasKey(i => i.Id);

            // Read/write straight through backing fields — Name, Order etc. keep their
            // protected/internal setters exactly as the Domain defines them. EF Core
            // reaches them via reflection; the Domain stays unaware of persistence.
            builder.UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
            builder.Property(i => i.Order);
            builder.Property(i => i.CreatedAt);

            // Self-referencing tree: a Group's Children / a child's ParentGroup.
            // "ParentGroupId" is a shadow FK — no such property exists on ProgramItem itself.
            builder.HasOne(i => i.ParentGroup)
                .WithMany(g => g.Children)
                .HasForeignKey("ParentGroupId")
                .OnDelete(DeleteBehavior.Restrict);

            // Multiple prerequisites (AND semantics): self-referencing many-to-many,
            // backed by a plain join table instead of a single PrerequisiteItemId FK.
            builder.HasMany(i => i.Prerequisites)
     .WithMany()
     .UsingEntity<Dictionary<string, object>>(
         "ProgramItemPrerequisites",
         right => right.HasOne<ProgramItem>().WithMany().HasForeignKey("PrerequisiteId").OnDelete(DeleteBehavior.Restrict),
         left => left.HasOne<ProgramItem>().WithMany().HasForeignKey("ProgramItemId").OnDelete(DeleteBehavior.Restrict)
     );
    

            builder.HasDiscriminator<string>("ItemType")
                .HasValue<Step>("Step")
                .HasValue<Group>("Group");
        }
    }
}
