using FluentValidation;
using ProgramDesigner.BLL.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Validators
{
    public class CreateProgramDtoValidator : AbstractValidator<CreateProgramDto>
    {
        public CreateProgramDtoValidator()
        {
            RuleFor(program => program.Name)
                .NotEmpty()
                .WithMessage("Program Name is required.");

            RuleFor(program => program.RootGroup)
                .NotNull()
                .WithMessage("RootGroup is required.");

            When(program => program.RootGroup != null, () =>
            {
                RuleFor(program => program.RootGroup.ItemType)
                    .Equal("Group")
                    .WithMessage("The top-level RootGroup must have ItemType = 'Group'.");

                RuleFor(program => program.RootGroup)
                    .SetValidator(new CreateProgramItemDtoValidator());
            });
        }
    }
}
