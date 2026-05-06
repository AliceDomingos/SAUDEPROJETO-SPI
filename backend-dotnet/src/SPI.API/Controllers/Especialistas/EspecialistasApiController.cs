using SPI.Api.Extensions;
using SPI.Application.DTOs.Specialists;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/specialists")]
[Authorize(Policy = "SpecialistAccess")]
public sealed class SpecialistsController : ControllerBase
{
    private readonly ISpecialistsAppService _specialistsAppService;

    public SpecialistsController(ISpecialistsAppService specialistsAppService)
    {
        _specialistsAppService = specialistsAppService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool activeOnly, CancellationToken cancellationToken)
    {
        var result = await _specialistsAppService.ListAsync(User.GetUserId(), activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "SpecialistManagement")]
    public async Task<IActionResult> Create([FromBody] CreateSpecialistRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _specialistsAppService.CreateAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SpecialistManagement")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialistRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _specialistsAppService.UpdateAsync(id, request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SpecialistManagement")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _specialistsAppService.DeactivateAsync(id, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Especialista desativado" });
    }
}
