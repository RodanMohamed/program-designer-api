using ProgramDesigner.Application.Common;
using ProgramDesigner.Application.DTOs.Requests;
using ProgramDesigner.Application.DTOs.Responses;
using ProgramDesigner.Application.Mapping;
using ProgramDesigner.Domain;
using ProgramDesigner.Domain.Exceptions;
using ProgramDesigner.Domain.Repositories;
using ProgramDesigner.Domain.Services;
using ProgramDesigner.Domain.Results;
using ProgramDesigner.Domain.Requests;
using ProgramDesigner.Domain.ValueObjects;

namespace ProgramDesigner.Application.Services
{
    public class ProgramService : IProgramService
    {
        private readonly ILearningProgramRepository _repository;
        private readonly IPrerequisiteValidationService _validationService;
        private readonly IProgramSimulationService _simulationService;

        public ProgramService(
            ILearningProgramRepository repository,
            IPrerequisiteValidationService validationService,
            IProgramSimulationService simulationService)
        {
            _repository = repository;
            _validationService = validationService;
            _simulationService = simulationService;
        }

        public async Task<GeneralResult<ProgramDto>> CreateProgramAsync(CreateProgramDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Validation", "Name", "Program Name is required."));
            }
            if (await _repository.ExistsByNameAsync(dto.Name))
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Conflict", "DuplicateProgramName", $"A program named '{dto.Name}' already exists."));
            }

            if (dto.RootGroup == null || dto.RootGroup.ItemType != "Group")
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Validation", "RootGroup", "RootGroup is required and must have ItemType 'Group'."));
            }

            Dictionary<string, ProgramItem> keyToEntity = new();
            List<CreateProgramItemDto> flatDtos = new();
            List<Error> buildErrors = new();

            ProgramItem rootEntity;
            try
            {
                rootEntity = BuildItemEntity(dto.RootGroup, keyToEntity, flatDtos, buildErrors);
            }
            catch (DomainException ex)
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Validation", "InvalidItem", ex.Message));
            }

            if (buildErrors.Count > 0)
            {
                return GeneralResult<ProgramDto>.Fail(buildErrors);
            }

            ValidatePrerequisiteReferences(flatDtos, keyToEntity, buildErrors);
            if (buildErrors.Count > 0)
            {
                return GeneralResult<ProgramDto>.Fail(buildErrors);
            }

            if (rootEntity is not Group rootGroup)
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Validation", "InvalidRoot", "The root item must be a Group."));
            }

            try
            {
                ApplyPrerequisiteLinks(flatDtos, keyToEntity);

                LearningProgram program = LearningProgram.Create(dto.Name, rootGroup);
                // Reject anything with an impossible prerequisite (cycle, self-reference,
                // nested-inside, appears-later) right here — these can never become valid
                // later, so there's no reason to let them into the database.
                // Reachability warnings are NOT blocking (per the spec: warn, don't reject) —
                // callers can check them any time via POST /programs/:id/validate.
                ValidationResult validationResult = _validationService.Validate(program);
                if (!validationResult.IsValid)
                {
                    List<Error> impossibleErrors = validationResult.ImpossiblePrerequisites
                        .Select(issue => new Error("Validation", "ImpossiblePrerequisite", issue.Description))
                        .ToList();

                    return GeneralResult<ProgramDto>.Fail(impossibleErrors);
                }


                await _repository.AddAsync(program);
                await _repository.SaveChangesAsync();

                return GeneralResult<ProgramDto>.Ok(ProgramMapper.ToProgramDto(program));
            }
            catch (DomainException ex)
            {
                return GeneralResult<ProgramDto>.Fail(new Error("Validation", "InvalidStructure", ex.Message));
            }
        }

        public async Task<GeneralResult<ProgramDto>> GetProgramByIdAsync(int id)
        {
            LearningProgram? program = await _repository.GetByIdAsync(id);

            if (program == null)
            {
                return GeneralResult<ProgramDto>.Fail(NotFound(id));
            }

            return GeneralResult<ProgramDto>.Ok(ProgramMapper.ToProgramDto(program));
        }

        public async Task<GeneralResult<ValidateProgramResponseDto>> ValidateProgramAsync(int id)
        {
            LearningProgram? program = await _repository.GetByIdAsync(id);

            if (program == null)
            {
                return GeneralResult<ValidateProgramResponseDto>.Fail(NotFound(id));
            }

            ValidationResult result = _validationService.Validate(program);

            return GeneralResult<ValidateProgramResponseDto>.Ok(ProgramMapper.ToValidateResponseDto(result));
        }

        public async Task<GeneralResult<SimulateProgramResponseDto>> SimulateProgramAsync(int id, SimulateProgramDto dto)
        {
            LearningProgram? program = await _repository.GetByIdAsync(id);

            if (program == null)
            {
                return GeneralResult<SimulateProgramResponseDto>.Fail(NotFound(id));
            }

            Dictionary<int, IReadOnlyList<int>> choices = dto.Choices.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<int>)pair.Value);

            SimulationInput input = new SimulationInput(choices, dto.CompletedItemIds.ToHashSet());

            SimulationResult result = _simulationService.Simulate(program, input);

            return GeneralResult<SimulateProgramResponseDto>.Ok(ProgramMapper.ToSimulateResponseDto(result));
        }

        private static Error NotFound(int id) =>
            new("NotFound", "ProgramNotFound", $"No program was found with Id {id}.");

        private ProgramItem BuildItemEntity(
            CreateProgramItemDto dto,
            Dictionary<string, ProgramItem> keyToEntity,
            List<CreateProgramItemDto> flatDtos,
            List<Error> errors)
        {
            ProgramItem entity = dto.ItemType switch
            {
                "Step" => Step.Create(dto.Name, dto.StepType ?? string.Empty),
                "Group" => Group.Create(dto.Name, BuildRule(dto)),
                _ => throw new DomainException($"Item '{dto.Key}' has an unknown ItemType '{dto.ItemType}'. Expected 'Step' or 'Group'.")
            };

            if (string.IsNullOrWhiteSpace(dto.Key) || keyToEntity.ContainsKey(dto.Key))
            {
                errors.Add(new Error("Validation", "DuplicateOrMissingKey", $"Key '{dto.Key}' is missing or duplicated. Keys must be unique across the whole tree."));
            }
            else
            {
                keyToEntity[dto.Key] = entity;
            }

            flatDtos.Add(dto);

            if (dto.ItemType == "Group" && entity is Group groupEntity)
            {
                foreach (CreateProgramItemDto childDto in dto.Children)
                {
                    ProgramItem childEntity = BuildItemEntity(childDto, keyToEntity, flatDtos, errors);
                    groupEntity.AddChild(childEntity);
                }
            }

            return entity;
        }
        private static GroupRule BuildRule(CreateProgramItemDto dto)
        {
            return dto.RuleType switch
            {
                "InOrder" => GroupRule.InOrder(),
                "Choice" => GroupRule.Choice(dto.ChoiceCount ?? 0),
                _ => throw new DomainException($"Item '{dto.Key}' has an unknown RuleType '{dto.RuleType}'. Expected 'InOrder' or 'Choice'.")
            };
        }

        private void ValidatePrerequisiteReferences(
            List<CreateProgramItemDto> flatDtos,
            Dictionary<string, ProgramItem> keyToEntity,
            List<Error> errors)
        {
            foreach (CreateProgramItemDto itemDto in flatDtos)
            {
                foreach (string prerequisiteKey in itemDto.PrerequisiteKeys)
                {
                    if (prerequisiteKey == itemDto.Key)
                    {
                        errors.Add(new Error("Validation", "SelfPrerequisite", $"Item '{itemDto.Key}' cannot have a prerequisite pointing at itself."));
                        continue;
                    }

                    if (!keyToEntity.ContainsKey(prerequisiteKey))
                    {
                        errors.Add(new Error("Validation", "InvalidReference", $"PrerequisiteKey '{prerequisiteKey}' referenced by '{itemDto.Key}' does not exist in the request."));
                    }
                }
            }
        }

        private void ApplyPrerequisiteLinks(List<CreateProgramItemDto> flatDtos, Dictionary<string, ProgramItem> keyToEntity)
        {
            foreach (CreateProgramItemDto itemDto in flatDtos)
            {
                ProgramItem sourceEntity = keyToEntity[itemDto.Key];

                foreach (string prerequisiteKey in itemDto.PrerequisiteKeys)
                {
                    sourceEntity.AddPrerequisite(keyToEntity[prerequisiteKey]);
                }
            }
        }
    }
}
