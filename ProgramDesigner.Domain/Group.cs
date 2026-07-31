using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain
{
    // A container that holds Steps and/or other Groups, nestable to any depth.
    public sealed class Group : ProgramItem
    {
        public GroupRule Rule { get; private set; } = null!;
        private Group() { } // for EF Core materialization

        private Group(string name, GroupRule rule) : base(name)
        {
            Rule = rule;
        }

        private readonly List<ProgramItem> _children = new();
        public IReadOnlyCollection<ProgramItem> Children => _children.AsReadOnly();


        public static Group Create(string name, GroupRule rule) => new Group(name, rule);

        public void AddChild(ProgramItem child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Order = _children.Count;
            child.ParentGroup = this;
            _children.Add(child);
        }

           // Walks this Group and every nested Group, checking each one's "pick N of M" rule
           // against its final child count. Called once, when the whole aggregate is composed.
        internal void EnsureRuleConsistencyRecursively()
                {
                    Rule.EnsureConsistentWith(_children.Count);

                    foreach (ProgramItem child in _children)
                    {
                        if (child is Group group)
                        {
                            group.EnsureRuleConsistencyRecursively();
                        }
                    }
                }
    
    }
}
