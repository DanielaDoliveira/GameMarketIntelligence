using GameMarketIntel.Collector.Jobs;
using GameMarketIntel.Collector.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.Workers;

public sealed class IgdbImportWorkerTests
{
    [Fact]
    public async Task StartAsync_ShouldExecuteImportOnceAndStopApplication()
    {
        // Arrange
        var importJob = Substitute.For<IIgdbImportJob>();
        var applicationLifetime = Substitute.For<IHostApplicationLifetime>();
        importJob.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var worker = CreateWorker(importJob, applicationLifetime);

        // Act
        await worker.StartAsync(CancellationToken.None);
        await worker.ExecuteTask!;

        // Assert
        await importJob.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
        applicationLifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StartAsync_ShouldTreatRequestedCancellationAsExpectedShutdown()
    {
        // Arrange
        using var cancellationSource = new CancellationTokenSource();
        var importStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var importJob = Substitute.For<IIgdbImportJob>();
        var applicationLifetime = Substitute.For<IHostApplicationLifetime>();
        importJob.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(callInfo => WaitForCancellationAsync(
                callInfo.Arg<CancellationToken>(),
                importStarted));
        var worker = CreateWorker(importJob, applicationLifetime);

        // Act
        await worker.StartAsync(cancellationSource.Token);
        await importStarted.Task;
        await cancellationSource.CancelAsync();
        await worker.ExecuteTask!;

        // Assert
        await importJob.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
        applicationLifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StartAsync_ShouldPropagateUnexpectedFailureAndStopApplication()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Import failed");
        var importJob = Substitute.For<IIgdbImportJob>();
        var applicationLifetime = Substitute.For<IHostApplicationLifetime>();
        importJob.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(expectedException));
        var worker = CreateWorker(importJob, applicationLifetime);

        // Act
        await worker.StartAsync(CancellationToken.None);
        var actualException = await Should.ThrowAsync<InvalidOperationException>(
            () => worker.ExecuteTask!);

        // Assert
        actualException.ShouldBeSameAs(expectedException);
        applicationLifetime.Received(1).StopApplication();
    }

    private static IgdbImportWorker CreateWorker(IIgdbImportJob importJob, IHostApplicationLifetime applicationLifetime) =>
        new(importJob, applicationLifetime, NullLogger<IgdbImportWorker>.Instance);

    private static async Task WaitForCancellationAsync(CancellationToken cancellationToken, TaskCompletionSource importStarted)
    {
        importStarted.SetResult();
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
