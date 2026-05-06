using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SPI.Application.DTOs.Patients;

public sealed class UpdatePatientRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\D*(\d\D*){11}$", ErrorMessage = "Informe um CPF valido com 11 digitos.")]
    public string Cpf { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("data_nascimento")]
    public DateTime? DataNascimento { get; init; }

    [Required]
    [RegularExpression("^(masculino|feminino|outro)$", ErrorMessage = "Informe um sexo valido.")]
    public string Sexo { get; init; } = string.Empty;

    [JsonPropertyName("nome_responsavel")]
    [MaxLength(200)]
    public string? NomeResponsavel { get; init; }

    [MaxLength(30)]
    public string? Telefone { get; init; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; init; }

    [RegularExpression(@"^\D*(\d\D*){8}$", ErrorMessage = "Informe um CEP valido com 8 digitos.")]
    [MaxLength(9)]
    public string? Cep { get; init; }

    [RegularExpression("^(AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO)$", ErrorMessage = "Informe uma UF valida.")]
    [MaxLength(2)]
    public string? Estado { get; init; }

    [MaxLength(120)]
    public string? Cidade { get; init; }

    [MaxLength(120)]
    public string? Bairro { get; init; }

    [MaxLength(200)]
    public string? Rua { get; init; }

    [MaxLength(30)]
    public string? Numero { get; init; }

    [MaxLength(200)]
    public string? Complemento { get; init; }

    [MaxLength(2000)]
    public string? Observacoes { get; init; }

    public Guid? GroupId { get; init; }
}
