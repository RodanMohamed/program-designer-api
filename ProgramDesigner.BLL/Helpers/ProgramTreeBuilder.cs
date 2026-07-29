using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Helpers
{
    public static class ProgramTreeBuilder
    {
        public static ProgramTreeBuildResult Build(IReadOnlyList<ProgramItem> flatItems, int rootId)
        {
            Dictionary<int, ProgramItem> itemsById = new Dictionary<int, ProgramItem>();
            foreach (ProgramItem item in flatItems)
            {
                itemsById[item.Id] = item;
            }

            // Group children by their parent's Id, ordered the way the designer intended.
            Dictionary<int, List<ProgramItem>> childrenByParentId = new Dictionary<int, List<ProgramItem>>();
            foreach (ProgramItem item in flatItems)
            {
                if (item.ParentGroupId.HasValue)
                {
                    if (!childrenByParentId.ContainsKey(item.ParentGroupId.Value))
                    {
                        childrenByParentId[item.ParentGroupId.Value] = new List<ProgramItem>();
                    }

                    childrenByParentId[item.ParentGroupId.Value].Add(item);
                }
            }

            foreach (List<ProgramItem> siblingList in childrenByParentId.Values)
            {
                siblingList.Sort((a, b) => a.Order.CompareTo(b.Order));
            }

            // Wire up navigation properties in memory: Children collections and PrerequisiteItem references.
            // This is required because the flat, no-tracking query does not populate these automatically.
            foreach (ProgramItem item in flatItems)
            {
                if (item is Group group)
                {
                    if (childrenByParentId.ContainsKey(group.Id))
                    {
                        group.Children = childrenByParentId[group.Id];
                    }
                    else
                    {
                        group.Children = new List<ProgramItem>();
                    }
                    foreach (ProgramItem child in group.Children)
                    {
                        child.ParentGroup = group;
                    }
                }

                if (item.PrerequisiteItemId.HasValue && itemsById.ContainsKey(item.PrerequisiteItemId.Value))
                {
                    item.PrerequisiteItem = itemsById[item.PrerequisiteItemId.Value];
                }
            }

            ProgramTreeBuildResult result = new ProgramTreeBuildResult();
            result.ItemsById = itemsById;

            if (!itemsById.ContainsKey(rootId))
            {
                throw new InvalidOperationException($"Root item with Id {rootId} was not found among the loaded items.");
            }

            result.Root = itemsById[rootId];

            int counter = 0;
            AssignTraversalOrder(result.Root, result, ref counter);
            Dictionary<int, ProgramItem> reachableItemsById = new Dictionary<int, ProgramItem>();
            foreach (int reachableId in result.EnterOrder.Keys)
            {
                reachableItemsById[reachableId] = itemsById[reachableId];
            }

            result.ItemsById = reachableItemsById;

            return result;
        }

        // Recursive pre-order DFS. Assigns an increasing EnterOrder on the way down,
        // and an ExitOrder once every descendant has been visited.
        private static void AssignTraversalOrder(ProgramItem item, ProgramTreeBuildResult result, ref int counter)
        {
            result.EnterOrder[item.Id] = counter;
            counter++;

            if (item is Group group)
            {
                foreach (ProgramItem child in group.Children)
                {
                    AssignTraversalOrder(child, result, ref counter);
                }
            }

            result.ExitOrder[item.Id] = counter;
            counter++;
        }
    }
    }
