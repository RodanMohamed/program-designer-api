using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Validators.ProgramValidationEngine
{
    public interface IProgramValidationEngine
    {
        // Pure, DB-independent logic: takes an already-built in-memory tree
        // and returns the validation outcome. This makes it directly unit-testable
        // without touching the database (see Part 3 of the challenge).
        ValidateProgramResponseDto Validate(ProgramTreeBuildResult tree);
    }
}
