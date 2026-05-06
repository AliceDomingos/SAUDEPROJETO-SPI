using System.Security.Claims;

namespace SPI.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId) || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuario autenticado invalido.");
        }

        return userId;
    }
}



