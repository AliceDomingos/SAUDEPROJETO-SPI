using SPI.Api.Extensions;
using SPI.Application.DTOs.Profile;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileAppService _profileAppService;

    public ProfileController(IProfileAppService profileAppService)
    {
        _profileAppService = profileAppService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _profileAppService.GetAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _profileAppService.UpdateAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }
}



