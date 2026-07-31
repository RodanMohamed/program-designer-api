using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Domain.Requests;
using ProgramDesigner.Domain.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Services
{
    public sealed class ProgramSimulationService : IProgramSimulationService
    {
        public SimulationResult Simulate(LearningProgram program, SimulationInput input)
        {
            TreeIndex tree = TreeIndex.Build(program.RootGroup);

            Dictionary<ProgramItem, bool> completeCache = new(ReferenceEqualityComparer.Instance);
            foreach (ProgramItem item in tree.AllItems)
            {
                ComputeIsComplete(item, input, completeCache);
            }

            Dictionary<ProgramItem, ProgramItemStatus> statusCache = new(ReferenceEqualityComparer.Instance);
            Dictionary<ProgramItem, string?> reasonCache = new(ReferenceEqualityComparer.Instance);

            ResolveStatus(program.RootGroup, input, completeCache, statusCache, reasonCache);

            List<ProgramItemState> states = new();
            foreach (ProgramItem item in tree.AllItems)
            {
                states.Add(new ProgramItemState(item, statusCache[item], reasonCache[item]));
            }

            // Keep the same top-to-bottom reading order as the designer sees the tree.
            states.Sort((a, b) => tree.EnterOrder[a.Item].CompareTo(tree.EnterOrder[b.Item]));

            return new SimulationResult(states);
        }

        private static bool ComputeIsComplete(ProgramItem item, SimulationInput input, Dictionary<ProgramItem, bool> cache)
        {
            if (cache.TryGetValue(item, out bool cached))
            {
                return cached;
            }

            bool isComplete;

            if (item is Step step)
            {
                isComplete = input.CompletedItemIds.Contains(step.Id);
            }
            else
            {
                Group group = (Group)item;
                int completedChildrenCount = 0;

                foreach (ProgramItem child in group.Children)
                {
                    if (ComputeIsComplete(child, input, cache))
                    {
                        completedChildrenCount++;
                    }
                }

                if (group.Rule.RuleType == GroupRuleType.InOrder)
                {
                    isComplete = completedChildrenCount == group.Children.Count;
                }
                else
                {
                    int requiredCount = group.Rule.ChoiceCount ?? group.Children.Count;
                    isComplete = completedChildrenCount >= requiredCount;
                }
            }

            cache[item] = isComplete;
            return isComplete;
        }

        private static ProgramItemStatus ResolveStatus(
            ProgramItem item,
            SimulationInput input,
            Dictionary<ProgramItem, bool> completeCache,
            Dictionary<ProgramItem, ProgramItemStatus> statusCache,
            Dictionary<ProgramItem, string?> reasonCache)
        {
            if (statusCache.TryGetValue(item, out ProgramItemStatus cachedStatus))
            {
                return cachedStatus;
            }

            ProgramItemStatus status;
            string? reason = null;

            if (completeCache[item])
            {
                status = ProgramItemStatus.Complete;
            }
            else if (item.ParentGroup != null && IsExcludedByChoice(item, input))
            {
                status = ProgramItemStatus.Excluded;
                reason = "This branch was not selected in the participant's choices, so it can never be completed.";
            }
            else
            {
                ProgramItemStatus? inheritedStatus = null;

                if (item.ParentGroup != null)
                {
                    ProgramItemStatus parentStatus = ResolveStatus(item.ParentGroup, input, completeCache, statusCache, reasonCache);

                    if (parentStatus == ProgramItemStatus.Blocked || parentStatus == ProgramItemStatus.Excluded)
                    {
                        inheritedStatus = parentStatus;
                        reason = $"Blocked because its containing group ('{item.ParentGroup.Name}') is not yet unlocked.";
                    }
                }

                if (inheritedStatus.HasValue)
                {
                    status = inheritedStatus.Value;
                }
                else
                {
                    // AND semantics: every prerequisite must be complete. List every one
                    // that isn't, so the participant knows exactly what's still pending.
                    List<string> incompleteNames = new();
                    foreach (ProgramItem prerequisite in item.Prerequisites)
                    {
                        if (!completeCache[prerequisite])
                        {
                            incompleteNames.Add(prerequisite.Name);
                        }
                    }

                    if (incompleteNames.Count > 0)
                    {
                        status = ProgramItemStatus.Blocked;
                        reason = "Waiting on: " + string.Join(", ", incompleteNames);
                    }
                    else if (IsBlockedBySequence(item, completeCache, out string? sequenceReason))
                    {
                        status = ProgramItemStatus.Blocked;
                        reason = sequenceReason;
                    }
                    else
                    {
                        status = ProgramItemStatus.Unlocked;
                    }
                }
            }

            statusCache[item] = status;
            reasonCache[item] = reason;

            if (item is Group groupItem)
            {
                foreach (ProgramItem child in groupItem.Children)
                {
                    ResolveStatus(child, input, completeCache, statusCache, reasonCache);
                }
            }

            return status;
        }

        private static bool IsBlockedBySequence(ProgramItem item, Dictionary<ProgramItem, bool> completeCache, out string? reason)
        {
            reason = null;

            if (item.ParentGroup == null || item.ParentGroup.Rule.RuleType != GroupRuleType.InOrder)
            {
                return false;
            }

            foreach (ProgramItem sibling in item.ParentGroup.Children)
            {
                if (ReferenceEquals(sibling, item))
                {
                    break;
                }

                if (sibling.Order < item.Order && !completeCache[sibling])
                {
                    reason = $"Waiting on '{sibling.Name}' to be completed first (must be done in order).";
                    return true;
                }
            }

            return false;
        }

        private static bool IsExcludedByChoice(ProgramItem item, SimulationInput input)
        {
            ProgramItem branch = item;
            Group? ancestor = item.ParentGroup;

            while (ancestor != null)
            {
                if (ancestor.Rule.RuleType == GroupRuleType.Choice
                    && input.Choices.TryGetValue(ancestor.Id, out IReadOnlyList<int>? chosenIds)
                    && !chosenIds.Contains(branch.Id))
                {
                    return true;
                }

                branch = ancestor;
                ancestor = ancestor.ParentGroup;
            }

            return false;
        }
    }
}
