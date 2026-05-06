using SPI.Domain.Entities;

namespace SPI.Application.Interfaces.Seguranca;

public interface IPasswordInviteTokenService
{
    string Generate(User user);
    PasswordInviteTokenPayload Validate(string token);
}

public sealed record PasswordInviteTokenPayload(Guid UserId, string PasswordHashSnapshot);



