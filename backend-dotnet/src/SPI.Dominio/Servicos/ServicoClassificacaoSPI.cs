namespace SPI.Domain.Services;

public static class SPIClassificationService
{
    public static decimal CalculateScore(IReadOnlyDictionary<string, int> respostas)
    {
        if (respostas.Count == 0)
        {
            throw new InvalidOperationException("A avaliacao precisa conter respostas.");
        }

        return respostas.Values.Sum(x => (decimal)x);
    }

    public static string Classify(decimal score)
    {
        if (score <= 29.5m)
        {
            return "Sem indicativo de TEA";
        }

        if (score < 37m)
        {
            return "TEA Leve a Moderado";
        }

        return "TEA Grave";
    }

    public static string ClassifyWithRanges(decimal score, IReadOnlyCollection<FormClassificationRange> ranges)
    {
        if (ranges.Count == 0)
            return Classify(score);

        var match = ranges.FirstOrDefault(r => score >= r.ScoreMin && score <= r.ScoreMax);
        return match?.Rotulo ?? Classify(score);
    }
}



