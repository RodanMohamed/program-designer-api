using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using ProgramDesigner.Common.Enums;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Validators.ProgramValidationEngine
{
    public class ProgramValidationEngine : IProgramValidationEngine
    {
        public ValidateProgramResponseDto Validate(ProgramTreeBuildResult tree)
        {
            List<PrerequisiteIssueDto> impossiblePrerequisites = new List<PrerequisiteIssueDto>();
            List<PrerequisiteIssueDto> reachabilityWarnings = new List<PrerequisiteIssueDto>();

            foreach (ProgramItem item in tree.ItemsById.Values)
            {
                if (!item.PrerequisiteItemId.HasValue || item.PrerequisiteItem == null)
                {
                    continue;
                }

                ProgramItem target = item.PrerequisiteItem;

                PrerequisiteIssueDto? impossibleIssue = CheckImpossible(item, target, tree);
                if (impossibleIssue != null)
                {
                    impossiblePrerequisites.Add(impossibleIssue);
                    continue;
                }

                PrerequisiteIssueDto? warningIssue = CheckReachabilityWarning(item, target,tree);
                if (warningIssue != null)
                {
                    reachabilityWarnings.Add(warningIssue);
                }
            }

            ValidateProgramResponseDto response = new ValidateProgramResponseDto();
            response.IsValid = impossiblePrerequisites.Count == 0;
            response.ImpossiblePrerequisites = impossiblePrerequisites;
            response.ReachabilityWarnings = reachabilityWarnings;

            return response;
        }

        private PrerequisiteIssueDto? CheckImpossible(ProgramItem item, ProgramItem target, ProgramTreeBuildResult tree)
        {
            if (target.Id == item.Id)
            {
                string message = "'" + item.Name + "' has a prerequisite pointing at itself, which can never be satisfied.";
                return BuildIssue(item, target, message);
            }

            int itemEnter = tree.EnterOrder[item.Id];
            int itemExit = tree.ExitOrder[item.Id];
            int targetEnter = tree.EnterOrder[target.Id];

            bool targetIsInsideItem = targetEnter > itemEnter && targetEnter < itemExit;
            if (targetIsInsideItem)
            {
                string message = "'" + item.Name + "' has a prerequisite on '" + target.Name + "', which is nested inside it. An item can never depend on something inside itself.";
                return BuildIssue(item, target, message);
            }

            bool targetAppearsLater = targetEnter > itemEnter;
            if (targetAppearsLater)
            {
                string message = "'" + item.Name + "' has a prerequisite on '" + target.Name + "', which appears later in the program. A prerequisite must always come before the item it unlocks.";
                return BuildIssue(item, target, message);
            }

            return null;
        }

        private PrerequisiteIssueDto? CheckReachabilityWarning(ProgramItem item, ProgramItem target, ProgramTreeBuildResult tree)
        {
            ProgramItem branch = target;
            ProgramItem? ancestor = target.ParentGroup;

            while (ancestor != null)
            {
                if (ancestor is Group group && group.RuleType == GroupRuleType.Choice)
                {
                    int childrenCount = group.Children.Count;
                    bool isRestrictiveChoice = group.ChoiceCount.HasValue && group.ChoiceCount.Value < childrenCount;

                    if (isRestrictiveChoice)
                    {
                        // If the item itself only becomes reachable through this very same
                        // branch, then choosing that branch is already guaranteed by the
                        // fact the item was attempted at all — no real risk here.
                        bool itemIsInSameBranch = IsDescendantOrSelf(item, branch, tree);

                        if (!itemIsInSameBranch)
                        {
                            string message = "'" + item.Name + "' depends on '" + target.Name + "', which sits inside the choice group '" +
                                group.Name + "' (pick " + group.ChoiceCount + " of " + childrenCount +
                                "). A participant might never select the branch containing '" + target.Name + "', so this prerequisite may never be satisfied.";

                            return BuildIssue(item, target, message);
                        }
                    }
                }

                branch = ancestor;
                ancestor = ancestor.ParentGroup;
            }

            return null;
        }
        private bool IsDescendantOrSelf(ProgramItem candidate, ProgramItem potentialAncestor, ProgramTreeBuildResult tree)
        {
            if (candidate.Id == potentialAncestor.Id)
            {
                return true;
            }

            int candidateEnter = tree.EnterOrder[candidate.Id];
            int ancestorEnter = tree.EnterOrder[potentialAncestor.Id];
            int ancestorExit = tree.ExitOrder[potentialAncestor.Id];

            return candidateEnter > ancestorEnter && candidateEnter < ancestorExit;
        }
        private PrerequisiteIssueDto BuildIssue(ProgramItem item, ProgramItem target, string description)
        {
            PrerequisiteIssueDto issue = new PrerequisiteIssueDto();
            issue.ItemId = item.Id;
            issue.ItemName = item.Name;
            issue.PrerequisiteItemId = target.Id;
            issue.PrerequisiteItemName = target.Name;
            issue.Description = description;

            return issue;
        }

    }
}