using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.ValueObjects
{
    // Encapsulates a Group's rule: either "InOrder" (no ChoiceCount) or
    // "Choice" (must have a valid ChoiceCount, checked against child count later
    // once the Group's children are known).
    public sealed class GroupRule : Common.ValueObject
    {
        public GroupRuleType RuleType { get; }
        public int? ChoiceCount { get; }

        private GroupRule(GroupRuleType ruleType, int? choiceCount)
        {
            RuleType = ruleType;
            ChoiceCount = choiceCount;
        }

        public static GroupRule InOrder() => new GroupRule(GroupRuleType.InOrder, null);

        public static GroupRule Choice(int choiceCount)
        {
            if (choiceCount <= 0)
                throw new DomainException("Choice count must be a positive number.");

            return new GroupRule(GroupRuleType.Choice, choiceCount);
        }

        // Called by Group whenever its children collection changes, to keep
        // "pick N of M" meaningful (N must never exceed the current child count).
        public void EnsureConsistentWith(int childrenCount)
        {
            if (RuleType == GroupRuleType.Choice && ChoiceCount.HasValue && ChoiceCount.Value > childrenCount)
            {
                throw new DomainException(
                    $"A Choice group cannot require picking {ChoiceCount.Value} item(s) when it only has {childrenCount} child/children.");
            }
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return RuleType;
            yield return ChoiceCount;
        }
    }
}
