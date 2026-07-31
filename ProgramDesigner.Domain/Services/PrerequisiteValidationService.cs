using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Domain.Results;

namespace ProgramDesigner.Domain.Services
{
    public sealed class PrerequisiteValidationService : IPrerequisiteValidationService
    {
        public Results.ValidationResult Validate(LearningProgram program)
        {
            TreeIndex tree = TreeIndex.Build(program.RootGroup);

            List<PrerequisiteIssue> impossible = new();
            List<PrerequisiteIssue> warnings = new();

            foreach (ProgramItem item in tree.AllItems)
            {
                foreach (ProgramItem prerequisite in item.Prerequisites)
                {
                    PrerequisiteIssue? impossibleIssue = CheckImpossible(item, prerequisite, tree);
                    if (impossibleIssue != null)
                    {
                        impossible.Add(impossibleIssue);
                        continue;
                    }

                    PrerequisiteIssue? warningIssue = CheckReachabilityWarning(item, prerequisite);
                    if (warningIssue != null)
                    {
                        warnings.Add(warningIssue);
                    }
                }
            }

            return new Results.ValidationResult(impossible.Count == 0, impossible, warnings);
        }

        private static PrerequisiteIssue? CheckImpossible(ProgramItem item, ProgramItem target, TreeIndex tree)
        {
            if (ReferenceEquals(item, target))
            {
                return new PrerequisiteIssue(item, target,
                    $"'{item.Name}' has a prerequisite pointing at itself, which can never be satisfied.");
            }

            int itemEnter = tree.EnterOrder[item];
            int itemExit = tree.ExitOrder[item];
            int targetEnter = tree.EnterOrder[target];

            bool targetIsInsideItem = targetEnter > itemEnter && targetEnter < itemExit;
            if (targetIsInsideItem)
            {
                return new PrerequisiteIssue(item, target,
                    $"'{item.Name}' has a prerequisite on '{target.Name}', which is nested inside it. An item can never depend on something inside itself.");
            }

            bool targetAppearsLater = targetEnter > itemEnter;
            if (targetAppearsLater)
            {
                return new PrerequisiteIssue(item, target,
                    $"'{item.Name}' has a prerequisite on '{target.Name}', which appears later in the program. A prerequisite must always come before the item it unlocks.");
            }

            return null;
        }

        private static PrerequisiteIssue? CheckReachabilityWarning(ProgramItem item, ProgramItem target)
        {
            ProgramItem branch = target;
            Group? ancestor = target.ParentGroup;

            while (ancestor != null)
            {
                if (ancestor.Rule.RuleType == GroupRuleType.Choice)
                {
                    int childrenCount = ancestor.Children.Count;
                    bool isRestrictiveChoice = ancestor.Rule.ChoiceCount.HasValue
                        && ancestor.Rule.ChoiceCount.Value < childrenCount;

                    if (isRestrictiveChoice && !IsDescendantOrSelf(item, branch))
                    {
                        string message =
                            $"'{item.Name}' depends on '{target.Name}', which sits inside the choice group " +
                            $"'{ancestor.Name}' (pick {ancestor.Rule.ChoiceCount} of {childrenCount}). " +
                            $"A participant might never select the branch containing '{target.Name}', so this prerequisite may never be satisfied.";

                        return new PrerequisiteIssue(item, target, message);
                    }
                }

                branch = ancestor;
                ancestor = ancestor.ParentGroup;
            }

            return null;
        }

        private static bool IsDescendantOrSelf(ProgramItem candidate, ProgramItem potentialAncestor)
        {
            ProgramItem? current = candidate;
            while (current != null)
            {
                if (ReferenceEquals(current, potentialAncestor))
                {
                    return true;
                }
                current = current.ParentGroup;
            }
            return false;
        }
    }
}