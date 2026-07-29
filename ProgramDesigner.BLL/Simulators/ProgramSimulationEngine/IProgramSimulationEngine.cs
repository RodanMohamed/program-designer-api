using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Simulators.ProgramSimulationEngine
{
    public interface IProgramSimulationEngine
    {
        SimulateProgramResponseDto Simulate(ProgramTreeBuildResult tree, SimulateProgramDto request);
    }
}
