using FluentValidation;
using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Validators
{
    public class CreateProgramItemDtoValidator : AbstractValidator<CreateProgramItemDto>
    {
        public CreateProgramItemDtoValidator()
        {
            RuleFor(item => item.Key)
                .NotEmpty()
                .WithMessage("Every item must have a non-empty Key.");

            RuleFor(item => item.Name)
                .NotEmpty()
                .WithMessage("Every item must have a Name.");

            RuleFor(item => item.ItemType)
                .Must(BeAValidItemType)
                .WithMessage("ItemType must be either 'Step' or 'Group'.");

            // Step-specific rules
            When(item => item.ItemType == "Step", () =>
            {
                RuleFor(item => item.StepType)
                    .NotEmpty()
                    .WithMessage("A Step must have a StepType.");

                RuleFor(item => item.RuleType)
                    .Must(ruleType => ruleType == null)
                    .WithMessage("A Step must not have a RuleType (that belongs to Group only).");

                RuleFor(item => item.ChoiceCount)
                    .Must(choiceCount => choiceCount == null)
                    .WithMessage("A Step must not have a ChoiceCount (that belongs to Group only).");

                RuleFor(item => item.Children)
                    .Must(children => children == null || children.Count == 0)
                    .WithMessage("A Step must not have Children.");
            });

            // Group-specific rules
            When(item => item.ItemType == "Group", () =>
            {
                RuleFor(item => item.RuleType)
                    .Must(BeAValidRuleType)
                    .WithMessage("RuleType must be either 'InOrder' or 'Choice'.");

                RuleFor(item => item.Children)
                    .NotEmpty()
                    .WithMessage("A Group must contain at least one child (Step or Group).");

                When(item => item.RuleType == "Choice", () =>
                {
                    RuleFor(item => item.ChoiceCount)
                        .NotNull()
                        .WithMessage("A Choice group must specify ChoiceCount (N in 'pick N of M').");

                    RuleFor(item => item.ChoiceCount)
                        .GreaterThan(0)
                        .When(item => item.ChoiceCount.HasValue)
                        .WithMessage("ChoiceCount must be greater than zero.");

                    RuleFor(item => item)
                        .Must(HaveChoiceCountWithinChildrenRange)
                        .WithMessage("ChoiceCount cannot be greater than the number of children.");
                });

                // Recursive validation: each child is validated by this same validator,
                // so nesting to any depth is checked automatically.
                RuleForEach(item => item.Children)
                    .SetValidator(this);
            });
        }

        private bool BeAValidItemType(string itemType)
        {
            return itemType == "Step" || itemType == "Group";
        }

        private bool BeAValidRuleType(string? ruleType)
        {
            if (ruleType == null)
            {
                return false;
            }

            return Enum.TryParse<GroupRuleType>(ruleType, out _);
        }

        private bool HaveChoiceCountWithinChildrenRange(CreateProgramItemDto item)
        {
            if (item.ChoiceCount == null)
            {
                return true;
            }

            return item.ChoiceCount <= item.Children.Count;
        }
    }
}
