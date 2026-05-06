namespace SPI.Application.DTOs.Groups;

public sealed class GroupResponseDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public Guid GestorId { get; init; }
    public string GestorNome { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public int QuantidadeMembros { get; init; }
    public DateTime CriadoEm { get; init; }
}



