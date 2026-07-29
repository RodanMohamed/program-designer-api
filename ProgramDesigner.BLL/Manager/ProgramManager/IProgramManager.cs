using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.Common.GeneralResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Manager.ProgramManagers
{
    public interface IProgramManager
    {
        Task<GeneralResult<ProgramDto>> CreateProgramAsync(CreateProgramDto dto);

        Task<GeneralResult<ProgramDto>> GetProgramByIdAsync(int id);

        Task<GeneralResult<ValidateProgramResponseDto>> ValidateProgramAsync(int id);
        Task<GeneralResult<SimulateProgramResponseDto>> SimulateProgramAsync(int id, SimulateProgramDto request);
    }
}
