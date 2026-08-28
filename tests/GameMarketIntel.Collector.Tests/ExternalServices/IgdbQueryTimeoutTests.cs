using GameMarketIntel.Collector.ExternalServices.Igdb;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices;

public class IgdbQueryTimeoutTests
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());

        // Act
        var result = await timeout.ExecuteAsync(_ => Task.FromResult("[]"), CancellationToken.None);

        // Assert
        result.ShouldBe("[]");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCancelOperationAtDeadline()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var timeout = CreateTimeout(timeProvider);

        // Act
        var task = timeout.ExecuteAsync(WaitForCancellationAsync, CancellationToken.None);
        timeProvider.Advance(RequestTimeout.Subtract(TimeSpan.FromSeconds(1)));
        var completedBeforeDeadline = task.IsCompleted;
        timeProvider.Advance(TimeSpan.FromSeconds(1));

        // Assert
        completedBeforeDeadline.ShouldBeFalse();
        var exception = await Should.ThrowAsync<TimeoutException>(() => task);
        exception.Message.ShouldBe("IGDB query exceeded the configured timeout.");
        exception.InnerException.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldShareOneDeadlineAcrossOperationSteps()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var timeout = CreateTimeout(timeProvider);
        var secondStepStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<string> RunStepsAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(20), timeProvider, cancellationToken);
            secondStepStarted.SetResult();
            await Task.Delay(TimeSpan.FromSeconds(20), timeProvider, cancellationToken);
            return "[]";
        }

        // Act
        var task = timeout.ExecuteAsync(RunStepsAsync, CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(20));
        await secondStepStarted.Task;
        timeProvider.Advance(TimeSpan.FromSeconds(10));

        // Assert
        await Should.ThrowAsync<TimeoutException>(() => task);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveCallerCancellation()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());
        using var cancellationSource = new CancellationTokenSource();

        // Act
        var task = timeout.ExecuteAsync(WaitForCancellationAsync, cancellationSource.Token);
        await cancellationSource.CancelAsync();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotStartOperationWhenCallerAlreadyCancelled()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var operationStarted = false;

        // Act
        var task = timeout.ExecuteAsync(_ =>
        {
            operationStarted = true;
            return Task.FromResult("[]");
        }, cancellationSource.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
        operationStarted.ShouldBeFalse();
    }
    [Fact]
    public async Task ExecuteAsync_ShouldPreserveUnrelatedCancellation()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());
        var expectedException = new OperationCanceledException("Cancellation from another component.");

        OperationCanceledException? actualException = null;

        // Act
        try
        {
            await timeout.ExecuteAsync(_ => Task.FromException<string>(expectedException), CancellationToken.None);
        }
        catch (OperationCanceledException exception)
        {
            actualException = exception;
        }

        // Assert
        actualException.ShouldBeSameAs(expectedException);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveOtherFailures()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());
        var expectedException = new InvalidOperationException("Operation failed.");

        // Act
        var task = timeout.ExecuteAsync(_ => Task.FromException<string>(expectedException), CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => task);
        exception.ShouldBeSameAs(expectedException);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectResultReturnedAfterDeadline()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var timeout = CreateTimeout(timeProvider);

        // Act
        var task = timeout.ExecuteAsync(_ =>
        {
            timeProvider.Advance(RequestTimeout);
            return Task.FromResult("[]");
        }, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<TimeoutException>(() => task);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreferCallerCancellationWhenBothSignalsAreCancelled()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var timeout = CreateTimeout(timeProvider);
        using var cancellationSource = new CancellationTokenSource();

        // Act
        var task = timeout.ExecuteAsync(_ =>
        {
            cancellationSource.Cancel();
            timeProvider.Advance(RequestTimeout);
            return Task.FromResult("[]");
        }, cancellationSource.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectNullOperation()
    {
        // Arrange
        var timeout = CreateTimeout(new FakeTimeProvider());

        // Act
        var task = timeout.ExecuteAsync(null!, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(() => task);
        exception.ParamName.ShouldBe("operation");
    }

    private static IgdbQueryTimeout CreateTimeout(TimeProvider timeProvider) =>
        new(Options.Create(new IgdbOptions { RequestTimeout = RequestTimeout }), timeProvider);

    private static async Task<string> WaitForCancellationAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return string.Empty;
    }
}