using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.Common
{
    public sealed class GeneralResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public List<Error> Errors { get; init; } = new();

        public static GeneralResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static GeneralResult<T> Fail(params Error[] errors) =>
            new() { Success = false, Errors = errors.ToList() };

        public static GeneralResult<T> Fail(IEnumerable<Error> errors) =>
            new() { Success = false, Errors = errors.ToList() };
    }
}
