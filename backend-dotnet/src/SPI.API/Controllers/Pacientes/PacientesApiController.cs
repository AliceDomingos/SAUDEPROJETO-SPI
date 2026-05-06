using SPI.Api.Extensions;
using SPI.Application.DTOs.Patients;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize(Policy = "PatientAccess")]
public sealed class PatientsController : ControllerBase
{
    private readonly IPatientsAppService _patientsAppService;

    public PatientsController(IPatientsAppService patientsAppService)
    {
        _patientsAppService = patientsAppService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _patientsAppService.ListAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reusable")]
    public async Task<IActionResult> ListReusable(CancellationToken cancellationToken)
    {
        var result = await _patientsAppService.ListReusableAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _patientsAppService.CreateAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _patientsAppService.UpdateAsync(id, request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _patientsAppService.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Paciente excluido" });
    }
}



