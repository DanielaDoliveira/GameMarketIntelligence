using System.Net.Http;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbPaginatorTests
{
    [Fact]
    public async Task ReadPagesAsync_ShouldAdvanceOffsetAndStopAtPartialPage()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        var query = new IgdbPageQuery("id", 2, "id > 0");
        client.QueryAsync("games", query.Build(0), Arg.Any<CancellationToken>()).Returns("[{\"id\":1},{\"id\":2}]");
        client.QueryAsync("games", query.Build(2), Arg.Any<CancellationToken>()).Returns("[{\"id\":3}]");
        var paginator = new IgdbPaginator(client);

        // Act
        var pages = await ReadAllAsync(paginator.ReadPagesAsync("games", query));

        // Assert
        pages.ShouldBe(new[] { "[{\"id\":1},{\"id\":2}]", "[{\"id\":3}]" });
        await client.Received(2).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await client.Received(1).QueryAsync("games", query.Build(0), Arg.Any<CancellationToken>());
        await client.Received(1).QueryAsync("games", query.Build(2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldRequestEmptyPageAfterExactMultiple()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        var query = new IgdbPageQuery("id", 1);
        client.QueryAsync("games", query.Build(0), Arg.Any<CancellationToken>()).Returns("[{\"id\":1}]");
        client.QueryAsync("games", query.Build(1), Arg.Any<CancellationToken>()).Returns("[]");
        var paginator = new IgdbPaginator(client);

        // Act
        var pages = await ReadAllAsync(paginator.ReadPagesAsync("games", query));

        // Assert
        pages.ShouldBe(new[] { "[{\"id\":1}]" });
        await client.Received(2).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldReturnNoPagesWhenFirstPageIsEmpty()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        client.QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("[]");
        var paginator = new IgdbPaginator(client);

        // Act
        var pages = await ReadAllAsync(paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 2)));

        // Assert
        pages.ShouldBeEmpty();
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldFetchOnlyWhenConsumerRequestsAnotherPage()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        client.QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("[{\"id\":1}]");
        var paginator = new IgdbPaginator(client);
        var pages = paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 1));

        // Act
        var callsBeforeReading = client.ReceivedCalls().Count();
        await using (var enumerator = pages.GetAsyncEnumerator())
        {
            (await enumerator.MoveNextAsync()).ShouldBeTrue();
            enumerator.Current.ShouldBe("[{\"id\":1}]");
        }

        // Assert
        callsBeforeReading.ShouldBe(0);
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-json-sensitive-body")]
    [InlineData("{\"sensitive-body\":true}")]
    [InlineData("null")]
    [InlineData("[{\"id\":1},{\"id\":2},{\"id\":3}]")]
    public async Task ReadPagesAsync_ShouldRejectInvalidPageWithoutExposingBody(string body)
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        client.QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(body);
        var paginator = new IgdbPaginator(client);

        // Act
        var task = ReadAllAsync(paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 2)));

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => task);
        exception.InnerException.ShouldBeNull();
        exception.ToString().ShouldNotContain("sensitive-body");
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldStopBeforeCallingClientWhenAlreadyCanceled()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        var paginator = new IgdbPaginator(client);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var task = ReadAllAsync(paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 1), cancellation.Token));

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
        await client.DidNotReceive().QueryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldHonorEnumeratorCancellationBetweenPages()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        using var cancellation = new CancellationTokenSource();
        client.QueryAsync("games", Arg.Any<string>(), cancellation.Token).Returns("[{\"id\":1}]");
        var paginator = new IgdbPaginator(client);
        await using var enumerator = paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 1))
            .GetAsyncEnumerator(cancellation.Token);
        (await enumerator.MoveNextAsync()).ShouldBeTrue();

        // Act
        cancellation.Cancel();
        var task = enumerator.MoveNextAsync().AsTask();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), cancellation.Token);
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldPropagateClientFailureWithoutRetrying()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        var expected = new HttpRequestException("Controlled failure.");
        client.QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(expected));
        var paginator = new IgdbPaginator(client);

        // Act
        var task = ReadAllAsync(paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 1)));

        // Assert
        var exception = await Should.ThrowAsync<HttpRequestException>(() => task);
        exception.ShouldBeSameAs(expected);
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadPagesAsync_ShouldNotYieldResponseAfterCancellation()
    {
        // Arrange
        var client = Substitute.For<IIgdbClient>();
        using var cancellation = new CancellationTokenSource();
        client.QueryAsync("games", Arg.Any<string>(), cancellation.Token).Returns(_ =>
        {
            cancellation.Cancel();
            return Task.FromResult("[{\"id\":1}]");
        });
        var paginator = new IgdbPaginator(client);
        await using var enumerator = paginator.ReadPagesAsync("games", new IgdbPageQuery("id", 2), cancellation.Token)
            .GetAsyncEnumerator();

        // Act
        var task = enumerator.MoveNextAsync().AsTask();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
        await client.Received(1).QueryAsync("games", Arg.Any<string>(), cancellation.Token);
    }

    private static async Task<List<string>> ReadAllAsync(IAsyncEnumerable<string> pages)
    {
        var result = new List<string>();

        await foreach (var page in pages)
            result.Add(page);

        return result;
    }
}
