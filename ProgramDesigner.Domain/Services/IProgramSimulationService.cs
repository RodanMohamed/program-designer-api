using ProgramDesigner.Domain.Requests;
using ProgramDesigner.Domain.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Services
{
    public interface IProgramSimulationService
    {
        SimulationResult Simulate(LearningProgram program, SimulationInput input);
    }
}
