using apbd_cw12.DTOs;
using apbd_cw12.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientsService _patientsService;

    public PatientsController(IPatientsService patientsService)
    {
        _patientsService = patientsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var result = await _patientsService.GetPatientsAsync(search);
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] CreateBedAssignmentRequest request)
    {
        try
        {
            await _patientsService.AssignBedAsync(pesel, request);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message); 
        }
    }
}