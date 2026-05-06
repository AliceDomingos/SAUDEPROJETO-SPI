namespace SPI.Application.DTOs.Evaluations;

public sealed class ExportFileResultDto
{
    public byte[] Content { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";
    public string FileName { get; init; } = "arquivo.bin";
}



