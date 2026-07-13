using GameMarketIntel.Domain.Enums;


namespace GameMarketIntel.Domain.ValueObjects;

public sealed class SourceReliability
{

    public ReliabilityLevel Level { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Limitations { get; private set; }

    public SourceReliability(ReliabilityLevel level, string reason, string? limitations = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A justificativa do nível de confiabilidade é obrigatória.", nameof(reason));

        Level = level;
        Reason = reason.Trim();
        Limitations = string.IsNullOrEmpty(limitations) ? null : limitations.Trim();
    }


}
