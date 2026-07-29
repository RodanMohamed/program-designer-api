using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgramDesigner.DAL.Data.Models
{
    // Abstract base for TPH inheritance. Step and Group are both stored in one
    // "ProgramItems" table, distinguished by EF Core's discriminator column.
    // This lets a Group hold a mixed list of Steps and Groups as children.
    public abstract class ProgramItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Position among siblings inside the same parent group.
        // Used to detect "points at something that appears later" impossible prerequisites.
        public int Order { get; set; }

        // Self-referencing FK: the parent Group that contains this item.
        // Null only for the single root Group of a program.
        public int? ParentGroupId { get; set; }
        public Group? ParentGroup { get; set; }

        // Self-referencing FK: the item that must be completed before this one unlocks.
        // Null means no prerequisite.
        public int? PrerequisiteItemId { get; set; }
        public ProgramItem? PrerequisiteItem { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
