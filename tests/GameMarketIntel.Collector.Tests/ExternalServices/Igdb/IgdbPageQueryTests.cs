using GameMarketIntel.Collector.ExternalServices.Igdb;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbPageQueryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(int.MaxValue)]
    public void Build_ShouldIncludeFieldsOrderingAndPagination(int offset)
    {
        // Arrange
        var query = new IgdbPageQuery("id,name", 100);

        // Act
        var result = query.Build(offset);

        // Assert
        result.ShouldBe(FormattableString.Invariant($"fields id,name; sort id asc; limit 100; offset {offset};"));
    }

    [Fact]
    public void Build_ShouldPreserveFilterAndNormalizeFieldSpacing()
    {
        // Arrange
        var query = new IgdbPageQuery(" id, name, cover.image_id ", 50, " id > 10 & platforms = (6,48) ");

        // Act
        var result = query.Build(100);

        // Assert
        result.ShouldBe("fields id,name,cover.image_id; where id > 10 & platforms = (6,48); sort id asc; limit 50; offset 100;");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_ShouldOmitEmptyFilter(string? filter)
    {
        // Arrange
        var query = new IgdbPageQuery("id", 100, filter);

        // Act
        var result = query.Build(0);

        // Assert
        result.ShouldBe("fields id; sort id asc; limit 100; offset 0;");
    }

    [Fact]
    public void Build_ShouldNotRetainThePreviousOffset()
    {
        // Arrange
        var query = new IgdbPageQuery("id", 100);

        // Act
        var first = query.Build(0);
        var second = query.Build(100);
        var repeated = query.Build(0);

        // Assert
        first.ShouldBe("fields id; sort id asc; limit 100; offset 0;");
        second.ShouldBe("fields id; sort id asc; limit 100; offset 100;");
        repeated.ShouldBe(first);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    public void Constructor_ShouldAcceptPageSizeBoundaries(int pageSize)
    {
        // Arrange
        const string fields = "id";

        // Act
        var query = new IgdbPageQuery(fields, pageSize);

        // Assert
        query.PageSize.ShouldBe(pageSize);
        query.Build(0).ShouldBe(FormattableString.Invariant($"fields id; sort id asc; limit {pageSize}; offset 0;"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public void Constructor_ShouldRejectInvalidPageSize(int pageSize)
    {
        // Arrange
        const string fields = "id";

        // Act
        Action action = () => new IgdbPageQuery(fields, pageSize);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("pageSize");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("*")]
    [InlineData("cover.*")]
    [InlineData("id,")]
    [InlineData("id,,name")]
    [InlineData("cover..image_id")]
    [InlineData(".name")]
    [InlineData("name.")]
    [InlineData("1name")]
    [InlineData("fields id")]
    [InlineData("id; limit 1")]
    public void Constructor_ShouldRejectInvalidFields(string? fields)
    {
        // Arrange
        const int pageSize = 100;

        // Act
        Action action = () => new IgdbPageQuery(fields!, pageSize);

        // Assert
        Should.Throw<ArgumentException>(action).ParamName.ShouldBe("fields");
    }

    [Theory]
    [InlineData("id > 10;")]
    [InlineData("id > 10; offset 500;")]
    [InlineData("id > 10; sort name desc;")]
    [InlineData("name = \"A;B\"")]
    public void Constructor_ShouldRejectSemicolonsInFilter(string filter)
    {
        // Arrange
        const int pageSize = 100;

        // Act
        Action action = () => new IgdbPageQuery("id", pageSize, filter);

        // Assert
        Should.Throw<ArgumentException>(action).ParamName.ShouldBe("filter");
    }

    [Fact]
    public void Build_ShouldRejectNegativeOffset()
    {
        // Arrange
        var query = new IgdbPageQuery("id", 100);

        // Act
        Action action = () => query.Build(-1);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("offset");
    }
}
