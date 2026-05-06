using System.Text.Json.Serialization;

namespace SPI.Application.DTOs.Patients;

public sealed class PatientResponseDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;

    [JsonPropertyName("data_nascimento")]
    public DateTime DataNascimento { get; init; }

    public string Sexo { get; init; } = string.Empty;
    public int? Idade { get; init; }

    [JsonPropertyName("avaliador_id")]
    public Guid? AvaliadorId { get; init; }

    [JsonPropertyName("nome_responsavel")]
    public string? NomeResponsavel { get; init; }

    public string? Telefone { get; init; }
    public string? Email { get; init; }

    [JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [JsonPropertyName("estado")]
    public string? Estado { get; init; }

    [JsonPropertyName("cidade")]
    public string? Cidade { get; init; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [JsonPropertyName("rua")]
    public string? Rua { get; init; }

    [JsonPropertyName("numero")]
    public string? Numero { get; init; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    public string? Observacoes { get; init; }

    [JsonPropertyName("group_id")]
    public Guid GroupId { get; init; }

    [JsonPropertyName("group_nome")]
    public string GroupNome { get; init; } = string.Empty;

    [JsonPropertyName("criado_em")]
    public DateTime CriadoEm { get; init; }
}
