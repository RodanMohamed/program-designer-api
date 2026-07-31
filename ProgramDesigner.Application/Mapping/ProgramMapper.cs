using ProgramDesigner.Application.DTOs.Responses;
using ProgramDesigner.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using ProgramDesigner.Domain.Results;
using System.Linq;

namespace ProgramDesigner.Application.Mapping
{
    public static class ProgramMapper
    {
        public static ProgramDto ToProgramDto(LearningProgram program)
        {
            return new ProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                CreatedAt = program.CreatedAt,
                RootGroup = ToItemDto(program.RootGroup)
            };
        }

        public static ProgramItemDto ToItemDto(ProgramItem item)
        {
            ProgramItemDto dto = new ProgramItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Prerequisites = item.Prerequisites
                    .Select(p => new PrerequisiteRefDto { ItemId = p.Id, ItemName = p.Name })
                    .ToList()
            };

            if (item is Step step)
            {
                dto.ItemType = "Step";
                dto.StepType = step.StepType;
            }
            else if (item is Group group)
            {
                dto.ItemType = "Group";
                dto.RuleType = group.Rule.RuleType.ToString();
                dto.ChoiceCount = group.Rule.ChoiceCount;
                dto.Children = group.Children.Select(ToItemDto).ToList();
            }

            return dto;
        }

        public static ValidateProgramResponseDto ToValidateResponseDto(ValidationResult result)
        {
            return new ValidateProgramResponseDto
            {
                IsValid = result.IsValid,
                ImpossiblePrerequisites = result.ImpossiblePrerequisites.Select(ToIssueDto).ToList(),
                ReachabilityWarnings = result.ReachabilityWarnings.Select(ToIssueDto).ToList()
            };
        }

        private static PrerequisiteIssueDto ToIssueDto(PrerequisiteIssue issue)
        {
            return new PrerequisiteIssueDto
            {
                ItemId = issue.Item.Id,
                ItemName = issue.Item.Name,
                PrerequisiteItemId = issue.Prerequisite.Id,
                PrerequisiteItemName = issue.Prerequisite.Name,
                Description = issue.Description
            };
        }

        public static SimulateProgramResponseDto ToSimulateResponseDto(SimulationResult result)
        {
            return new SimulateProgramResponseDto
            {
                Items = result.Items.Select(state => new ProgramItemStateDto
                {
                    ItemId = state.Item.Id,
                    ItemName = state.Item.Name,
                    ItemType = state.Item is Step ? "Step" : "Group",
                    Status = state.Status.ToString(),
                    Reason = state.Reason
                }).ToList()
            };
        }

    }
}