using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class CompanyTests
{
    [Fact]
    public void Constructor_ShouldCreateCompany()
    {
        // Arrange
        const string name = "Nintendo EPD";

        // Act
        var company = new Company(name);

        // Assert
        company.Id.ShouldNotBe(Guid.Empty);
        company.Name.ShouldBe(name);
        company.NormalizedName.ShouldBe("NINTENDO EPD");
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange & Act
        var company = new Company("  Nintendo EPD  ");

        // Assert
        company.Name.ShouldBe("Nintendo EPD");
        company.NormalizedName.ShouldBe("NINTENDO EPD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        // Arrange & Act
        var action = () => new Company(name!);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAllowNameAtMaximumLength()
    {
        // Arrange
        var name = new string('A', Company.MaxNameLength);

        // Act
        var company = new Company(name);

        // Assert
        company.Name.Length.ShouldBe(Company.MaxNameLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var name = new string('A', Company.MaxNameLength + 1);

        // Act
        var action = () => new Company(name);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}