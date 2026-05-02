using SPI.Api.Extensions;
using SPI.Api.Services;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "DashboardAccess")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardAppService _dashboardAppService;
    private readonly SpiMockSusDashboardService _spiMockSusDashboardService;

    public DashboardController(
        IDashboardAppService dashboardAppService,
        SpiMockSusDashboardService spiMockSusDashboardService)
    {
        _dashboardAppService = dashboardAppService;
        _spiMockSusDashboardService = spiMockSusDashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _dashboardAppService.GetAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("spi-mock")]
    public async Task<IActionResult> GetSpiMock(
        [FromQuery] string? risco,
        [FromQuery] string? especialista,
        [FromQuery] string? mes,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        var result = await _spiMockSusDashboardService.GetAsync(risco, especialista, mes, dataInicio, dataFim, cancellationToken);
        return Ok(result);
    }
}



