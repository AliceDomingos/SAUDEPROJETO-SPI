using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Groups;

public sealed class CreateGroupRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; init; } = string.Empty;

    public Guid? GestorId { get; init; }
}



