using ProgramDesigner.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Services
{
    // Internal helper: walks the aggregate once and assigns DFS pre-order
    // enter/exit numbers. Uses reference equality (not Entity.Id) so it works
    // correctly even on trees that haven't been persisted yet (unit tests).
    internal sealed class TreeIndex
    {
        public List<ProgramItem> AllItems { get; } = new();
        public Dictionary<ProgramItem, int> EnterOrder { get; } = new(ReferenceEqualityComparer.Instance);
        public Dictionary<ProgramItem, int> ExitOrder { get; } = new(ReferenceEqualityComparer.Instance);

        public static TreeIndex Build(Group root)
        {
            TreeIndex index = new TreeIndex();
            int counter = 0;
            Visit(root, index, ref counter);
            return index;
        }

        private static void Visit(ProgramItem item, TreeIndex index, ref int counter)
        {
            index.AllItems.Add(item);
            index.EnterOrder[item] = counter;
            counter++;

            if (item is Group group)
            {
                foreach (ProgramItem child in group.Children)
                {
                    Visit(child, index, ref counter);
                }
            }

            index.ExitOrder[item] = counter;
            counter++;
        }
    }
}
