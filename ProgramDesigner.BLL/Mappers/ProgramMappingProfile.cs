using AutoMapper;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Mappers
{
    public class ProgramMappingProfile : Profile
    {
        public ProgramMappingProfile()
        {
            // Base map: handles fields common to both Step and Group.
            // Include<T, TDto> tells AutoMapper to dispatch to the derived map
            // below when the runtime type of the source object is Step or Group.
            CreateMap<ProgramItem, ProgramItemDto>()
                .Include<Step, ProgramItemDto>()
                .Include<Group, ProgramItemDto>()
                .ForMember(dest => dest.ItemType, opt =>
                {
                    opt.MapFrom(src => src is Step ? "Step" : "Group");
                })
                .ForMember(dest => dest.PrerequisiteItemName, opt =>
                {
                    opt.MapFrom(src => src.PrerequisiteItem == null ? null : src.PrerequisiteItem.Name);
                })
                .ForMember(dest => dest.StepType, opt => opt.Ignore())
                .ForMember(dest => dest.RuleType, opt => opt.Ignore())
                .ForMember(dest => dest.ChoiceCount, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            // Derived map: fills Step-only fields. Common fields are inherited from the base map above.
            CreateMap<Step, ProgramItemDto>()
                .IncludeBase<ProgramItem, ProgramItemDto>()
                .ForMember(dest => dest.StepType, opt =>
                {
                    opt.MapFrom(src => src.StepType);
                });

            // Derived map: fills Group-only fields, including the recursive Children mapping.
            // Children is ordered by "Order" so the tree reflects the designer's intended sequence.
            CreateMap<Group, ProgramItemDto>()
                .IncludeBase<ProgramItem, ProgramItemDto>()
                .ForMember(dest => dest.RuleType, opt =>
                {
                    opt.MapFrom(src => src.RuleType.ToString());
                })
                .ForMember(dest => dest.ChoiceCount, opt =>
                {
                    opt.MapFrom(src => src.ChoiceCount);
                })
                .ForMember(dest => dest.Children, opt =>
                {
                    opt.MapFrom(src => src.Children.OrderBy(c => c.Order));
                });

            // Top-level program mapping: wraps the root group tree with program metadata.
            CreateMap<LearningProgram, ProgramDto>()
                .ForMember(dest => dest.RootGroup, opt =>
                {
                    opt.MapFrom(src => src.RootGroup);
                });
        }
    }
}