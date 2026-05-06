using SPI.Domain.Entities;

namespace SPI.Application.Interfaces.Seguranca;

public interface ITokenService
{
    string Generate(User user);
}



