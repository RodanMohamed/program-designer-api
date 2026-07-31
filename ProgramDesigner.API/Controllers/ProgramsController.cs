using Microsoft.AspNetCore.Mvc;
using ProgramDesigner.Application.Common;
using ProgramDesigner.Application.DTOs.Requests;
using ProgramDesigner.Application.DTOs.Responses;
using ProgramDesigner.Application.Services;

namespace ProgramDesigner.API.Controllers
{
    [ApiController]
    [Route("programs")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;

        public ProgramsController(IProgramService programService)
        {
            _programService = programService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            GeneralResult<ProgramDto> result = await _programService.CreateProgramAsync(dto);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return CreatedAtAction(nameof(GetProgramById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgramById(int id)
        {
            GeneralResult<ProgramDto> result = await _programService.GetProgramByIdAsync(id);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateProgram(int id)
        {
            GeneralResult<ValidateProgramResponseDto> result = await _programService.ValidateProgramAsync(id);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost("{id}/simulate")]
        public async Task<IActionResult> SimulateProgram(int id, [FromBody] SimulateProgramDto? dto)
        {
            SimulateProgramDto requestDto = dto ?? new SimulateProgramDto();

            GeneralResult<SimulateProgramResponseDto> result = await _programService.SimulateProgramAsync(id, requestDto);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }

        private IActionResult MapErrorsToActionResult(List<Error> errors)
        {

            if (errors.Count == 0)
            {
                return BadRequest(errors);
            }

            return errors[0].Type switch
            {
                "NotFound" => NotFound(errors),
                "Conflict" => Conflict(errors),
                _ => BadRequest(errors)
            };
        }
    }
}