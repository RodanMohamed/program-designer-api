using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Models
{
    // Represents a program as an administrative unit (name, id, creation date).
    // Wraps the actual structure, which lives in a single top-level Group.
    public sealed class LearningProgram
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int RootGroupId { get; set; }
        public Group RootGroup { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
