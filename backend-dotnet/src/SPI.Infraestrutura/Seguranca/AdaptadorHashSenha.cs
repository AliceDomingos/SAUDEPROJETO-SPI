using SPI.Application.Interfaces.Seguranca;
using SPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace SPI.Infrastructure.Data.Security;

public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    private static User CreatePasswordHasherUser() =>
        new("placeholder", new("placeholder@local"), "seed", Domain.Enums.UserRole.Admin);

    public string Hash(string password)
    {
        var user = CreatePasswordHasherUser();
        return _passwordHasher.HashPassword(user, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var user = CreatePasswordHasherUser();
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}




