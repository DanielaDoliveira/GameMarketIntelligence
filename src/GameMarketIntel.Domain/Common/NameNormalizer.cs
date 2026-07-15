using System.Text;

namespace GameMarketIntel.Domain.Common;

internal static class NameNormalizer
{
    public static string Normalize(string value)
    {
        return value .Trim()  .Normalize(NormalizationForm.FormC) .ToUpperInvariant();
    }
}