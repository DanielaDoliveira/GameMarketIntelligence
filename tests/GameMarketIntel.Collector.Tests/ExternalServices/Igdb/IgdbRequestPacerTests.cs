using GameMarketIntel.Collector.ExternalServices.Igdb;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public class IgdbRequestPacerTests
{
    private static readonly DateTimeOffset StartTime =
        new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task WaitAsync_ShouldAllowFirstRequestImmediately()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider);

        // Act
        var waitTask = pacer.WaitAsync(CancellationToken.None);

        // Assert
        waitTask.IsCompletedSuccessfully.ShouldBeTrue();
        await waitTask;
    }

    [Fact]
    public async Task WaitAsync_ShouldWaitUntilConfiguredIntervalHasElapsed()
    {
        // Arrange
        var requestInterval = TimeSpan.FromMilliseconds(300);
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider, requestInterval);
        await pacer.WaitAsync(CancellationToken.None);

        // Act
        var secondWaitTask = pacer.WaitAsync(CancellationToken.None);
        timeProvider.Advance(requestInterval.Subtract(TimeSpan.FromMilliseconds(1)));

        // Assert
        secondWaitTask.IsCompleted.ShouldBeFalse();

        timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        await secondWaitTask;
    }

    [Fact]
    public async Task WaitAsync_ShouldSerializeConcurrentRequests()
    {
        // Arrange
        var requestInterval = TimeSpan.FromMilliseconds(300);
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider, requestInterval);

        // Act
        var firstWaitTask = pacer.WaitAsync(CancellationToken.None);
        var secondWaitTask = pacer.WaitAsync(CancellationToken.None);
        var thirdWaitTask = pacer.WaitAsync(CancellationToken.None);

        // Assert
        await firstWaitTask;
        secondWaitTask.IsCompleted.ShouldBeFalse();
        thirdWaitTask.IsCompleted.ShouldBeFalse();

        timeProvider.Advance(requestInterval);
        var completedWaitTask = await Task.WhenAny(secondWaitTask, thirdWaitTask);
        await completedWaitTask;
        new[] { secondWaitTask, thirdWaitTask }
            .Count(task => task.IsCompletedSuccessfully)
            .ShouldBe(1);

        timeProvider.Advance(requestInterval);
        await Task.WhenAll(secondWaitTask, thirdWaitTask);
    }

    [Fact]
    public async Task WaitAsync_ShouldPropagateCancellation()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider);
        await pacer.WaitAsync(CancellationToken.None);
        using var cancellationSource = new CancellationTokenSource();

        // Act
        var waitTask = pacer.WaitAsync(cancellationSource.Token);
        await cancellationSource.CancelAsync();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(waitTask);
    }

    [Theory]
    [InlineData(3600)]
    [InlineData(-3600)]
    public async Task WaitAsync_ShouldIgnoreWallClockChanges(int clockShiftSeconds)
    {
        // Arrange
        var timeProvider = new IndependentClockTimeProvider();
        var pacer = CreatePacer(timeProvider);
        await pacer.WaitAsync(CancellationToken.None);

        // Act
        timeProvider.ShiftUtc(TimeSpan.FromSeconds(clockShiftSeconds));
        var secondWait = pacer.WaitAsync(CancellationToken.None);

        // Assert
        secondWait.IsCompleted.ShouldBeFalse();
        timeProvider.AdvanceElapsed(TimeSpan.FromMilliseconds(299));
        secondWait.IsCompleted.ShouldBeFalse();
        timeProvider.AdvanceElapsed(TimeSpan.FromMilliseconds(1));
        await secondWait.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task WaitAsync_ShouldWaitOnlyForRemainingInterval()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider);
        await pacer.WaitAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMilliseconds(200));

        // Act
        var secondWait = pacer.WaitAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMilliseconds(99));

        // Assert
        secondWait.IsCompleted.ShouldBeFalse();
        timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        await secondWait.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task WaitAsync_ShouldAllowRequestImmediatelyAfterIdleInterval()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider);
        await pacer.WaitAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(1));

        // Act
        var secondWait = pacer.WaitAsync(CancellationToken.None);

        // Assert
        secondWait.IsCompletedSuccessfully.ShouldBeTrue();
        await secondWait;
    }

    [Fact]
    public async Task WaitAsync_ShouldReleaseLockAndNotReserveSlotAfterCancellation()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(StartTime);
        var pacer = CreatePacer(timeProvider);
        await pacer.WaitAsync(CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        var canceledWait = pacer.WaitAsync(cancellation.Token);

        // Act
        await cancellation.CancelAsync();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => canceledWait);
        var nextWait = pacer.WaitAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMilliseconds(300));
        await nextWait.WaitAsync(TimeSpan.FromSeconds(5));
    }

    // UTC can move independently from the timestamps and timers used for elapsed time.
    private sealed class IndependentClockTimeProvider : TimeProvider
    {
        private readonly FakeTimeProvider _elapsedClock = new(StartTime);
        private DateTimeOffset _utcNow = StartTime;

        public override long TimestampFrequency => _elapsedClock.TimestampFrequency;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public override long GetTimestamp() => _elapsedClock.GetTimestamp();

        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period) =>
            _elapsedClock.CreateTimer(callback, state, dueTime, period);

        public void ShiftUtc(TimeSpan difference) => _utcNow = _utcNow.Add(difference);

        public void AdvanceElapsed(TimeSpan elapsed) => _elapsedClock.Advance(elapsed);
    }

    private static IgdbRequestPacer CreatePacer(TimeProvider timeProvider, TimeSpan? requestInterval = null)
    {
        var options = Options.Create(new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            RequestInterval = requestInterval ?? TimeSpan.FromMilliseconds(300)
        });

        return new IgdbRequestPacer(options, timeProvider);
    }
}
