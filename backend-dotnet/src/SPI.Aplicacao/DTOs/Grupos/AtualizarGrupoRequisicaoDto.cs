using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Groups;

public sealed class UpdateGroupRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; init; } = string.Empty;

    public int? GestorId { get; init; }
}



