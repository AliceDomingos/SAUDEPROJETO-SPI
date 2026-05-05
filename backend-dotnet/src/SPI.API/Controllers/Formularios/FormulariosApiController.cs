using SPI.Api.Extensions;
using SPI.Application.DTOs.Forms;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/forms")]
[Authorize(Policy = "FormAccess")]
public sealed class FormsController : ControllerBase
{
    private readonly IFormsAppService _formsAppService;

    public FormsController(IFormsAppService formsAppService)
    {
        _formsAppService = formsAppService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _formsAppService.ListAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{formId:int}")]
    public async Task<IActionResult> GetById(int formId, CancellationToken cancellationToken)
    {
        var result = await _formsAppService.GetByIdAsync(formId, User.GetUserId(), cancellationToken);
        if (result is null)
        {
            return NotFound(new { detail = "Formulario nao encontrado" });
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "FormManagement")]
    public async Task<IActionResult> Create([FromBody] CreateFormRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _formsAppService.CreateAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{formId:int}")]
    [Authorize(Policy = "FormManagement")]
    public async Task<IActionResult> Update(int formId, [FromBody] UpdateFormRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _formsAppService.UpdateAsync(formId, request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{formId:int}")]
    [Authorize(Policy = "FormManagement")]
    public async Task<IActionResult> Deactivate(int formId, CancellationToken cancellationToken)
    {
        await _formsAppService.DeactivateAsync(formId, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{formId:int}/activate")]
    [Authorize(Policy = "FormManagement")]
    public async Task<IActionResult> Activate(int formId, CancellationToken cancellationToken)
    {
        await _formsAppService.ActivateAsync(formId, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}



