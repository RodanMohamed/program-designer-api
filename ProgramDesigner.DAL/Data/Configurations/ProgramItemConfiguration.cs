using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Configurations
{
    public class ProgramItemConfiguration : IEntityTypeConfiguration<ProgramItem>
    {
        public void Configure(EntityTypeBuilder<ProgramItem> builder)
        {
            builder.ToTable("ProgramItems");

            // The "ItemType" column tells EF which concrete type to materialize each row as.
            builder.HasDiscriminator<string>("ItemType")
                .HasValue<Step>("Step")
                .HasValue<Group>("Group");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Self-reference
            // Cascade is fine here: deleting a group should delete everything nested inside it.
            builder.HasOne(pi => pi.ParentGroup)
                .WithMany(g => g.Children)
                .HasForeignKey(pi => pi.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-reference: PrerequisiteItem.
            // Must be Restrict, otherwise SQL Server rejects the model with
            // "may cause cycles or multiple cascade paths" because this and
            // ParentGroup both point back into the same table.
            builder.HasOne(pi => pi.PrerequisiteItem)
                .WithMany()
                .HasForeignKey(pi => pi.PrerequisiteItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}