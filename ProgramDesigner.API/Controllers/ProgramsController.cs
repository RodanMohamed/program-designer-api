using Microsoft.AspNetCore.Mvc;
using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Manager.ProgramManagers;
using ProgramDesigner.Common.GeneralResult;

namespace ProgramDesigner.API.Controllers
{
    [ApiController]
    [Route("programs")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramManager _programManager;

        public ProgramsController(IProgramManager programManager)
        {
            _programManager = programManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            GeneralResult<ProgramDto> result = await _programManager.CreateProgramAsync(dto);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return CreatedAtAction(nameof(GetProgramById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgramById(int id)
        {
            GeneralResult<ProgramDto> result = await _programManager.GetProgramByIdAsync(id);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateProgram(int id)
        {
            GeneralResult<ValidateProgramResponseDto> result = await _programManager.ValidateProgramAsync(id);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }

        private IActionResult MapErrorsToActionResult(List<Error> errors)
        {
            bool isNotFoundError = errors.Count > 0 && errors[0].Type == "NotFound";

            if (isNotFoundError)
            {
                return NotFound(errors);
            }

            return BadRequest(errors);
        }
        [HttpPost("{id}/simulate")]
        public async Task<IActionResult> SimulateProgram(int id, [FromBody] SimulateProgramDto? dto)
        {
            SimulateProgramDto requestDto = dto ?? new SimulateProgramDto();

            GeneralResult<SimulateProgramResponseDto> result = await _programManager.SimulateProgramAsync(id, requestDto);

            if (!result.Success)
            {
                return MapErrorsToActionResult(result.Errors);
            }

            return Ok(result.Data);
        }
    }
}
