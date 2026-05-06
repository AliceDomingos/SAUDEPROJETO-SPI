namespace SPI.Application.Interfaces.Seguranca;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}



