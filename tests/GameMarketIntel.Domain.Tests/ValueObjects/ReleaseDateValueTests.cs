
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.ValueObjects;


public sealed class ReleaseDateValueTests
{

    [Fact]
    public void ForDay_ShouldCreateDayPrecisionValue()
    {
        // Act
        var value = ReleaseDateValue.ForDay( new DateOnly(2026, 8, 18));

        // Assert
        value.Kind.ShouldBe(ReleaseDateKind.Day);
        value.Year.ShouldBe(2026);
        value.Month.ShouldBe(8);
        value.Day.ShouldBe(18);
        value.Quarter.ShouldBeNull();
    }

     [Fact]
    public void ForMonth_ShouldCreateMonthPrecisionValue()
    {
        // Act
        var value = ReleaseDateValue.ForMonth(
            year: 2026,
            month: 8);

        // Assert
        value.Kind.ShouldBe(ReleaseDateKind.Month);
        value.Year.ShouldBe(2026);
        value.Month.ShouldBe(8);
        value.Day.ShouldBeNull();
        value.Quarter.ShouldBeNull();
    }


    [Fact]
    public void ForYear_ShouldCreateYearPrecisionValue()
    {
        // Act
        var value = ReleaseDateValue.ForYear(2026);

        // Assert
        value.Kind.ShouldBe(ReleaseDateKind.Year);
        value.Year.ShouldBe(2026);
        value.Month.ShouldBeNull();
        value.Day.ShouldBeNull();
        value.Quarter.ShouldBeNull();
    }
    [Fact]
    public void ForQuarter_ShouldCreateQuarterPrecisionValue()
    {
        // Act
        var value = ReleaseDateValue.ForQuarter(
            year: 2026,
            quarter: 3);

        // Assert
        value.Kind.ShouldBe(ReleaseDateKind.Quarter);
        value.Year.ShouldBe(2026);
        value.Month.ShouldBeNull();
        value.Day.ShouldBeNull();
        value.Quarter.ShouldBe(3);
    }


    [Fact]
    public void ToBeDetermined_ShouldCreateValueWithoutDateComponents()
    {
        // Act
        var value = ReleaseDateValue.ToBeDetermined();

        // Assert
        value.Kind.ShouldBe(
            ReleaseDateKind.ToBeDetermined);
        value.Year.ShouldBeNull();
        value.Month.ShouldBeNull();
        value.Day.ShouldBeNull();
        value.Quarter.ShouldBeNull();
    }

     [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    public void ForYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(
        int invalidYear)
    {
        // Act
        var exception =
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                _ = ReleaseDateValue.ForYear(invalidYear);
            });

        // Assert
        exception.ParamName.ShouldBe("year");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void ForMonth_ShouldThrowArgumentOutOfRangeException_WhenMonthIsInvalid(
        int invalidMonth)
    {
        // Act
        var exception =
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                _ = ReleaseDateValue.ForMonth(
                    2026,
                    invalidMonth);
            });

        // Assert
        exception.ParamName.ShouldBe("month");
    }


    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void ForQuarter_ShouldThrowArgumentOutOfRangeException_WhenQuarterIsInvalid(
        int invalidQuarter)
    {
        // Act
        var exception =
            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                _ = ReleaseDateValue.ForQuarter(
                    2026,
                    invalidQuarter);
            });

        // Assert
        exception.ParamName.ShouldBe("quarter");
    }
    [Fact]
public void Values_ShouldBeEqual_WhenComponentsAreEqual()
{
    // Arrange
    var first = ReleaseDateValue.ForMonth(
        year: 2026,
        month: 8);

    var second = ReleaseDateValue.ForMonth(
        year: 2026,
        month: 8);

    // Assert
    first.ShouldBe(second);
}


}

