using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SPI.Application.DTOs.Evaluations;

public sealed class CreateEvaluationRequestDto
{
    [JsonPropertyName("patientId")]
    public Guid? PatientIdCamelCase { get; init; }

    [JsonPropertyName("patient_id")]
    public Guid? PatientIdSnakeCase { get; init; }

    [JsonPropertyName("formId")]
    public Guid? FormId { get; init; }

    [JsonPropertyName("groupId")]
    public Guid? GroupId { get; init; }

    [MaxLength(2000)]
    [JsonPropertyName("observacoes")]
    public string? Observacoes { get; init; }

    [Required]
    public Dictionary<string, int> Respostas { get; init; } = [];

    public Guid ResolvePatientId()
    {
        var patientId = PatientIdSnakeCase ?? PatientIdCamelCase;
        if (patientId is null || patientId == Guid.Empty)
        {
            throw new InvalidOperationException("patient_id ou patientId precisa ser informado.");
        }

        return patientId.Value;
    }
}



