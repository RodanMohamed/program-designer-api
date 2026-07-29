using ProgramDesigner.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Models
{
    // A container that holds Steps and/or other Groups, nestable to any depth.
    public sealed class Group : ProgramItem
    {
        public GroupRuleType RuleType { get; set; }

        // Only meaningful when RuleType == Choice. Represents "pick N of M".
        // Null when RuleType == InOrder.
        public int? ChoiceCount { get; set; }

        // Inverse navigation: all direct children (Steps and/or Groups) of this group.
        public ICollection<ProgramItem> Children { get; set; } = new List<ProgramItem>();
    }
}
