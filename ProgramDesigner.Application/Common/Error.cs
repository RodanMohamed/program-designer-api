using System;
using System.Collections.Generic;
using System.Text;


namespace ProgramDesigner.Application.Common
{
    public sealed record Error(string Type, string Code, string Message);
}

