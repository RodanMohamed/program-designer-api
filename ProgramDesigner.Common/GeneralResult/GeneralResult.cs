using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Common.GeneralResult
{
    public sealed class GeneralResult<T>
    {
        public bool Success { get; init; }

        public T? Data { get; init; }

        public List<Error> Errors { get; init; } = [];

        public static GeneralResult<T> Ok(T data)
        {
            return new GeneralResult<T>
            {
                Success = true,
                Data = data,
                Errors = []
            };
        }

        public static GeneralResult<T> Fail(params Error[] errors)
        {
            return new GeneralResult<T>
            {
                Success = false,
                Data = default,
                Errors = [.. errors]
            };
        }

        public static GeneralResult<T> Fail(IEnumerable<Error> errors)
        {
            return new GeneralResult<T>
            {
                Success = false,
                Data = default,
                Errors = [.. errors]
            };
        }
    }
}