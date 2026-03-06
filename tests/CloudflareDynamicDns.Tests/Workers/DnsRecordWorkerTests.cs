using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services.Interfaces;
using CloudflareDynamicDns.Workers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CloudflareDynamicDns.Tests.Workers;

public class DnsRecordWorkerTests
{
    private static IOptions<CloudflareOptions> CreateOptions(int intervalMinutes = 0) =>
        Options.Create(new CloudflareOptions
        {
            ApiToken = "test-token",
            DomainNames = ["sub.example.com"],
            IntervalMinutes = intervalMinutes
        });

    private static CloudflareDnsRecord CreateRecord(string ip, string name = "sub.example.com") =>
        new() { Id = "record-1", Name = name, Content = ip, ZoneId = "zone-1" };

    /// <summary>
    /// Cancels the stoppingToken after the first call to FetchIpAddressAsync so the
    /// worker completes after exactly one iteration (Task.Delay with a cancelled token exits).
    /// </summary>
    private static (Mock<IPublicIpAddressFetcher>, CancellationTokenSource) SetupFetcherWithOneIteration(
        string ip = "5.6.7.8")
    {
        var cts = new CancellationTokenSource();
        var fetcherMock = new Mock<IPublicIpAddressFetcher>();
        fetcherMock
            .Setup(f => f.FetchIpAddressAsync())
            .ReturnsAsync(() =>
            {
                cts.Cancel();
                return ip;
            });
        return (fetcherMock, cts);
    }

    [Fact]
    public async Task ExecuteAsync_CallsUpdateIpAddress_ForRecordsWithOutdatedIp()
    {
        const string currentIp = "5.6.7.8";
        const string outdatedIp = "1.2.3.4";

        var (fetcherMock, cts) = SetupFetcherWithOneIteration(currentIp);

        var serviceMock = new Mock<ICloudflareService>();
        serviceMock
            .Setup(s => s.SyncDnsRecordsAsync(currentIp))
            .ReturnsAsync([CreateRecord(outdatedIp)]);

        var worker = new DnsRecordWorker(fetcherMock.Object, serviceMock.Object, CreateOptions());
        await worker.StartAsync(cts.Token);
        try { await worker.ExecuteTask!; } catch (OperationCanceledException) { }

        serviceMock.Verify(s => s.UpdateIpAddressAsync(
            It.Is<CloudflareDnsRecord>(r => r.Content == outdatedIp), currentIp), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotCallUpdateIpAddress_WhenIpIsUnchanged()
    {
        const string currentIp = "1.2.3.4";

        var (fetcherMock, cts) = SetupFetcherWithOneIteration(currentIp);

        var serviceMock = new Mock<ICloudflareService>();
        serviceMock
            .Setup(s => s.SyncDnsRecordsAsync(currentIp))
            .ReturnsAsync([CreateRecord(currentIp)]);

        var worker = new DnsRecordWorker(fetcherMock.Object, serviceMock.Object, CreateOptions());
        await worker.StartAsync(cts.Token);
        try { await worker.ExecuteTask!; } catch (OperationCanceledException) { }

        serviceMock.Verify(s => s.UpdateIpAddressAsync(
            It.IsAny<CloudflareDnsRecord>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotCallUpdateIpAddress_WhenNoRecordsReturned()
    {
        const string currentIp = "1.2.3.4";

        var (fetcherMock, cts) = SetupFetcherWithOneIteration(currentIp);

        var serviceMock = new Mock<ICloudflareService>();
        serviceMock
            .Setup(s => s.SyncDnsRecordsAsync(currentIp))
            .ReturnsAsync([]);

        var worker = new DnsRecordWorker(fetcherMock.Object, serviceMock.Object, CreateOptions());
        await worker.StartAsync(cts.Token);
        try { await worker.ExecuteTask!; } catch (OperationCanceledException) { }

        serviceMock.Verify(s => s.UpdateIpAddressAsync(
            It.IsAny<CloudflareDnsRecord>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_StopsImmediately_WhenAlreadyCancelled()
    {
        var fetcherMock = new Mock<IPublicIpAddressFetcher>();
        var serviceMock = new Mock<ICloudflareService>();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var worker = new DnsRecordWorker(fetcherMock.Object, serviceMock.Object, CreateOptions());
        await worker.StartAsync(cts.Token);
        try { await worker.ExecuteTask!; } catch (OperationCanceledException) { }

        fetcherMock.Verify(f => f.FetchIpAddressAsync(), Times.Never);
        serviceMock.Verify(s => s.SyncDnsRecordsAsync(It.IsAny<string>()), Times.Never);
    }
}
