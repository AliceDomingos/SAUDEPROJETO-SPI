namespace SPI.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Email e obrigatorio.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
        {
            throw new InvalidOperationException("Email invalido.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}



