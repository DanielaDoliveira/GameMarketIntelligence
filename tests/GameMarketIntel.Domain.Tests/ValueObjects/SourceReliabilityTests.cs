using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.ValueObjects;

public sealed class SourceReliabilityTests
{
    [Fact]
    public void Constructor_ShouldCreateSourceReliability_WhenDataIsValid()
    {
        // Arrange
        const ReliabilityLevel level = ReliabilityLevel.PublicDirect;

        const string reason = "Dados obtidos diretamente de uma API pública.";

        const string limitations = "Os dados não representam vendas.";

        // Act
        var reliability = new SourceReliability( level, reason, limitations);

        // Assert
        reliability.Level.ShouldBe(level);
        reliability.Reason.ShouldBe(reason);
        reliability.Limitations.ShouldBe(limitations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenReasonIsInvalid(
    string invalidReason)
    {
        // Act
        var exception = Should.Throw<ArgumentException>(() =>new SourceReliability(ReliabilityLevel.PublicDirect,invalidReason));

        // Assert
        exception.ParamName.ShouldBe("reason");
    }
}