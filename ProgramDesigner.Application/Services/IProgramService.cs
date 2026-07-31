using ProgramDesigner.Application.Common;
using ProgramDesigner.Application.DTOs.Requests;
using ProgramDesigner.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.Services
{
    public interface IProgramService
    {
        Task<GeneralResult<ProgramDto>> CreateProgramAsync(CreateProgramDto dto);
        Task<GeneralResult<ProgramDto>> GetProgramByIdAsync(int id);
        Task<GeneralResult<ValidateProgramResponseDto>> ValidateProgramAsync(int id);
        Task<GeneralResult<SimulateProgramResponseDto>> SimulateProgramAsync(int id, SimulateProgramDto dto);
    }
}
