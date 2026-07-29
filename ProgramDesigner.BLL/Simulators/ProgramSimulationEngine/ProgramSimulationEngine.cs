using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using ProgramDesigner.Common.Enums;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Simulators.ProgramSimulationEngine
{
    public class ProgramSimulationEngine : IProgramSimulationEngine
    {
        public SimulateProgramResponseDto Simulate(ProgramTreeBuildResult tree, SimulateProgramDto request)
        {
            HashSet<int> completedIds = new HashSet<int>(request.CompletedItemIds);

            Dictionary<int, bool> completeCache = new Dictionary<int, bool>();
            foreach (ProgramItem item in tree.ItemsById.Values)
            {
                ComputeIsComplete(item, completedIds, completeCache);
            }

            Dictionary<int, ProgramItemStatus> statusCache = new Dictionary<int, ProgramItemStatus>();
            Dictionary<int, string?> reasonCache = new Dictionary<int, string?>();

            ResolveStatus(tree.Root, request, completeCache, statusCache, reasonCache);

            List<ProgramItemStateDto> items = new List<ProgramItemStateDto>();

            foreach (ProgramItem item in tree.ItemsById.Values)
            {
                ProgramItemStateDto stateDto = new ProgramItemStateDto();
                stateDto.ItemId = item.Id;
                stateDto.ItemName = item.Name;
                stateDto.ItemType = item is Step ? "Step" : "Group";
                stateDto.Status = statusCache[item.Id].ToString();
                stateDto.Reason = reasonCache.ContainsKey(item.Id) ? reasonCache[item.Id] : null;

                items.Add(stateDto);
            }

            items.Sort(delegate (ProgramItemStateDto a, ProgramItemStateDto b)
            {
                return tree.EnterOrder[a.ItemId].CompareTo(tree.EnterOrder[b.ItemId]);
            });

            SimulateProgramResponseDto response = new SimulateProgramResponseDto();
            response.Items = items;

            return response;
        }

        private bool ComputeIsComplete(ProgramItem item, HashSet<int> completedIds, Dictionary<int, bool> cache)
        {
            if (cache.ContainsKey(item.Id))
            {
                return cache[item.Id];
            }

            bool isComplete;

            if (item is Step)
            {
                isComplete = completedIds.Contains(item.Id);
            }
            else
            {
                Group group = (Group)item;
                int completedChildrenCount = 0;

                foreach (ProgramItem child in group.Children)
                {
                    bool childIsComplete = ComputeIsComplete(child, completedIds, cache);
                    if (childIsComplete)
                    {
                        completedChildrenCount++;
                    }
                }

                if (group.RuleType == GroupRuleType.InOrder)
                {
                    isComplete = completedChildrenCount == group.Children.Count;
                }
                else
                {
                    int requiredCount = group.ChoiceCount ?? group.Children.Count;
                    isComplete = completedChildrenCount >= requiredCount;
                }
            }

            cache[item.Id] = isComplete;
            return isComplete;
        }

        private ProgramItemStatus ResolveStatus(
            ProgramItem item,
            SimulateProgramDto request,
            Dictionary<int, bool> completeCache,
            Dictionary<int, ProgramItemStatus> statusCache,
            Dictionary<int, string?> reasonCache)
        {
            if (statusCache.ContainsKey(item.Id))
            {
                return statusCache[item.Id];
            }

            ProgramItemStatus status;
            string? reason = null;

            if (completeCache[item.Id])
            {
                status = ProgramItemStatus.Complete;
            }
            else if (item.ParentGroup != null && IsExcludedByChoice(item, request))
            {
                status = ProgramItemStatus.Excluded;
                reason = "This branch was not selected in the participant's choices, so it can never be completed.";
            }
            else
            {
                ProgramItemStatus? inheritedStatus = null;

                if (item.ParentGroup != null)
                {
                    ProgramItemStatus parentStatus = ResolveStatus(item.ParentGroup, request, completeCache, statusCache, reasonCache);

                    if (parentStatus == ProgramItemStatus.Blocked || parentStatus == ProgramItemStatus.Excluded)
                    {
                        inheritedStatus = parentStatus;
                        reason = "Blocked because its containing group ('" + item.ParentGroup.Name + "') is not yet unlocked.";
                    }
                }

                if (inheritedStatus.HasValue)
                {
                    status = inheritedStatus.Value;
                }
                else if (item.PrerequisiteItemId.HasValue && item.PrerequisiteItem != null && !completeCache[item.PrerequisiteItem.Id])
                {
                    status = ProgramItemStatus.Blocked;
                    reason = "Waiting on prerequisite '" + item.PrerequisiteItem.Name + "' to be completed.";
                }
                else
                {
                    string? sequenceReason;
                    bool blockedBySequence = IsBlockedBySequence(item, completeCache, out sequenceReason);

                    if (blockedBySequence)
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

            statusCache[item.Id] = status;
            reasonCache[item.Id] = reason;

            if (item is Group groupItem)
            {
                foreach (ProgramItem child in groupItem.Children)
                {
                    ResolveStatus(child, request, completeCache, statusCache, reasonCache);
                }
            }

            return status;
        }

        private bool IsBlockedBySequence(ProgramItem item, Dictionary<int, bool> completeCache, out string? reason)
        {
            reason = null;

            if (item.ParentGroup == null || item.ParentGroup.RuleType != GroupRuleType.InOrder)
            {
                return false;
            }

            foreach (ProgramItem sibling in item.ParentGroup.Children)
            {
                if (sibling.Id == item.Id)
                {
                    break;
                }

                if (sibling.Order < item.Order && !completeCache[sibling.Id])
                {
                    reason = "Waiting on '" + sibling.Name + "' to be completed first (must be done in order).";
                    return true;
                }
            }

            return false;
        }

        private bool IsExcludedByChoice(ProgramItem item, SimulateProgramDto request)
        {
            ProgramItem branch = item;
            ProgramItem? ancestor = item.ParentGroup;

            while (ancestor != null)
            {
                if (ancestor is Group group && group.RuleType == GroupRuleType.Choice)
                {
                    if (request.Choices.ContainsKey(group.Id))
                    {
                        List<int> chosenIds = request.Choices[group.Id];

                        if (!chosenIds.Contains(branch.Id))
                        {
                            return true;
                        }
                    }
                }

                branch = ancestor;
                ancestor = ancestor.ParentGroup;
            }

            return false;
        }
    }
}

