using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Common.GeneralResult
{
    // Base result: represents success/failure of an operation with no return data
    public sealed class GeneralResult
    {
        public bool Success { get; init; }

        public List<Error> Errors { get; init; } = [];

        public static GeneralResult Ok()
        {
            return new GeneralResult
            {
                Success = true,
                Errors = []
            };
        }

        public static GeneralResult Fail(params Error[] errors)
        {
            return new GeneralResult
            {
                Success = false,
                Errors = [.. errors]
            };
        }

        public static GeneralResult Fail(IEnumerable<Error> errors)
        {
            return new GeneralResult
            {
                Success = false,
                Errors = [.. errors]
            };
        }
    }
}
