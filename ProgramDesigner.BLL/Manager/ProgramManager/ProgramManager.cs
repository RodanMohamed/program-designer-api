using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using ProgramDesigner.BLL.Validators.ProgramValidationEngine;
using ProgramDesigner.Common.Enums;
using ProgramDesigner.Common.GeneralResult;
using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.DAL.UnitOfWork;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ProgramDesigner.BLL.Simulators.ProgramSimulationEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProgramDesigner.BLL.Manager.ProgramManagers
{
    public class ProgramManager : IProgramManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateProgramDto> _createValidator;
        private readonly IProgramValidationEngine _validationEngine;
        private readonly IProgramSimulationEngine _simulationEngine;

        public ProgramManager(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateProgramDto> createValidator,
            IProgramValidationEngine validationEngine,
            IProgramSimulationEngine simulationEngine)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _validationEngine = validationEngine;
            _simulationEngine = simulationEngine;
            _simulationEngine = simulationEngine;
        }

        public async Task<GeneralResult<ProgramDto>> CreateProgramAsync(CreateProgramDto dto)
        {
            ValidationResult shapeValidationResult = await _createValidator.ValidateAsync(dto);
            if (!shapeValidationResult.IsValid)
            {
                List<Error> shapeErrors = new List<Error>();
                foreach (ValidationFailure failure in shapeValidationResult.Errors)
                {
                    shapeErrors.Add(new Error("Validation", failure.PropertyName, failure.ErrorMessage));
                }

                return GeneralResult<ProgramDto>.Fail(shapeErrors);
            }

            Dictionary<string, ProgramItem> keyToEntity = new Dictionary<string, ProgramItem>();
            List<CreateProgramItemDto> flatDtos = new List<CreateProgramItemDto>();
            List<string> buildErrorMessages = new List<string>();

            ProgramItem rootEntity = BuildItemEntity(dto.RootGroup, 0, keyToEntity, flatDtos, buildErrorMessages);

            ValidatePrerequisiteReferences(flatDtos, keyToEntity, buildErrorMessages);
            if (buildErrorMessages.Count > 0)
            {
                List<Error> referenceErrors = new List<Error>();
                foreach (string message in buildErrorMessages)
                {
                    referenceErrors.Add(new Error("Validation", "InvalidReference", message));
                }

                return GeneralResult<ProgramDto>.Fail(referenceErrors);
            }

            if (rootEntity is not Group rootGroup)
            {
                Error rootError = new Error("Validation", "InvalidRoot", "The root item must be a Group.");
                return GeneralResult<ProgramDto>.Fail(rootError);
            }

            LearningProgram program = new LearningProgram();
            program.Name = dto.Name;
            program.RootGroup = rootGroup;

            await _unitOfWork.LearningPrograms.AddAsync(program);
            await _unitOfWork.SaveChangesAsync();
            ApplyPrerequisiteLinks(flatDtos, keyToEntity);
            await _unitOfWork.SaveChangesAsync();

            ProgramDto resultDto = _mapper.Map<ProgramDto>(program);

            return GeneralResult<ProgramDto>.Ok(resultDto);

        }

        public async Task<GeneralResult<ProgramDto>> GetProgramByIdAsync(int id)
        {
            LearningProgram? program = await _unitOfWork.LearningPrograms.GetByIdWithRootGroupAsync(id);

            if (program == null)
            {
                Error notFoundError = new Error("NotFound", "ProgramNotFound", "No program was found with Id " + id + ".");
                return GeneralResult<ProgramDto>.Fail(notFoundError);
            }

            IReadOnlyList<ProgramItem> flatItems = await _unitOfWork.ProgramItems.GetAllFlatNoTrackingAsync();
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, program.RootGroupId);

            program.RootGroup = (Group)tree.Root;

            ProgramDto resultDto = _mapper.Map<ProgramDto>(program);

            return GeneralResult<ProgramDto>.Ok(resultDto);
        }

        public async Task<GeneralResult<ValidateProgramResponseDto>> ValidateProgramAsync(int id)
        {
            LearningProgram? program = await _unitOfWork.LearningPrograms.GetByIdWithRootGroupAsync(id);

            if (program == null)
            {
                Error notFoundError = new Error("NotFound", "ProgramNotFound", "No program was found with Id " + id + ".");
                return GeneralResult<ValidateProgramResponseDto>.Fail(notFoundError);
            }

            IReadOnlyList<ProgramItem> flatItems = await _unitOfWork.ProgramItems.GetAllFlatNoTrackingAsync();
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, program.RootGroupId);

            ValidateProgramResponseDto validationResponse = _validationEngine.Validate(tree);

            return GeneralResult<ValidateProgramResponseDto>.Ok(validationResponse);
        }
        public async Task<GeneralResult<SimulateProgramResponseDto>> SimulateProgramAsync(int id, SimulateProgramDto request)
        {
            LearningProgram? program = await _unitOfWork.LearningPrograms.GetByIdWithRootGroupAsync(id);

            if (program == null)
            {
                Error notFoundError = new Error("NotFound", "ProgramNotFound", "No program was found with Id " + id + ".");
                return GeneralResult<SimulateProgramResponseDto>.Fail(notFoundError);
            }

            IReadOnlyList<ProgramItem> flatItems = await _unitOfWork.ProgramItems.GetAllFlatNoTrackingAsync();
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, program.RootGroupId);

            SimulateProgramResponseDto simulationResponse = _simulationEngine.Simulate(tree, request);

            return GeneralResult<SimulateProgramResponseDto>.Ok(simulationResponse);
        }

        private ProgramItem BuildItemEntity(
            CreateProgramItemDto dto,
            int order,
            Dictionary<string, ProgramItem> keyToEntity,
            List<CreateProgramItemDto> flatDtos,
            List<string> errorMessages)
        {
            ProgramItem entity;

            if (dto.ItemType == "Step")
            {
                Step step = new Step();
                step.StepType = dto.StepType ?? string.Empty;
                entity = step;
            }
            else
            {
                Group group = new Group();
                group.RuleType = Enum.Parse<GroupRuleType>(dto.RuleType!);
                group.ChoiceCount = dto.ChoiceCount;
                group.Children = new List<ProgramItem>();
                entity = group;
            }

            entity.Name = dto.Name;
            entity.Order = order;

            if (keyToEntity.ContainsKey(dto.Key))
            {
                errorMessages.Add("Duplicate Key '" + dto.Key + "' found in the request. Keys must be unique across the whole tree.");
            }
            else
            {
                keyToEntity[dto.Key] = entity;
            }

            flatDtos.Add(dto);

            if (dto.ItemType == "Group")
            {
                Group groupEntity = (Group)entity;
                int childOrder = 0;

                foreach (CreateProgramItemDto childDto in dto.Children)
                {
                    ProgramItem childEntity = BuildItemEntity(childDto, childOrder, keyToEntity, flatDtos, errorMessages);
                    groupEntity.Children.Add(childEntity);
                    childOrder++;
                }
            }

            return entity;
        }

        private void ValidatePrerequisiteReferences(
    List<CreateProgramItemDto> flatDtos,
    Dictionary<string, ProgramItem> keyToEntity,
    List<string> errorMessages)
        {
            foreach (CreateProgramItemDto itemDto in flatDtos)
            {
                if (itemDto.PrerequisiteKey == null)
                {
                    continue;
                }

                if (itemDto.PrerequisiteKey == itemDto.Key)
                {
                    errorMessages.Add("Item '" + itemDto.Key + "' cannot have a prerequisite pointing at itself.");
                    continue;
                }

                if (!keyToEntity.ContainsKey(itemDto.PrerequisiteKey))
                {
                    errorMessages.Add("PrerequisiteKey '" + itemDto.PrerequisiteKey + "' referenced by '" + itemDto.Key + "' does not exist in the request.");
                }
            }
        }

        private void ApplyPrerequisiteLinks(
            List<CreateProgramItemDto> flatDtos,
            Dictionary<string, ProgramItem> keyToEntity)
        {
            foreach (CreateProgramItemDto itemDto in flatDtos)
            {
                if (itemDto.PrerequisiteKey == null)
                {
                    continue;
                }

                ProgramItem sourceEntity = keyToEntity[itemDto.Key];
                sourceEntity.PrerequisiteItem = keyToEntity[itemDto.PrerequisiteKey];
            }
        }
    }
 }

