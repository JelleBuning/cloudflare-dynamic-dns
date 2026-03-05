using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CloudflareDynamicDns.Core.Tests.Services;

public class CloudflareServiceTests
{
    private readonly CloudflareOptions _options = new()
    {
        ApiToken = "test-token",
        DomainNames = ["sub.example.com"],
        IntervalMinutes = 5
    };

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler);

    private static void SetupResponse(Mock<HttpMessageHandler> handlerMock, HttpResponseMessage response) =>
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

    private static void SetupSequencedResponses(Mock<HttpMessageHandler> handlerMock, params HttpResponseMessage[] responses)
    {
        var setup = handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var response in responses)
            setup = setup.ReturnsAsync(response);
    }

    private static HttpResponseMessage JsonResponse(object content) =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(content)
        };

    private CloudflareResponse ZoneResponse(string zoneId = "zone-1") => new()
    {
        Success = true,
        Result =
        [
            new CloudflareDnsRecord { Id = zoneId, Name = "example.com", Content = "1.2.3.4" }
        ]
    };

    private CloudflareResponse DnsRecordsResponse(string id = "record-1", string ip = "1.2.3.4") => new()
    {
        Success = true,
        Result =
        [
            new CloudflareDnsRecord { Id = id, Name = "sub.example.com", Content = ip, ZoneId = "zone-1", Ttl = 1, Proxied = false }
        ]
    };

    private static CloudflareResponse EmptyResponse() => new() { Success = true, Result = [] };

    [Fact]
    public async Task SyncDnsRecordsAsync_ReturnsExistingRecords_WhenRecordsExist()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupSequencedResponses(handlerMock,
            JsonResponse(ZoneResponse()),
            JsonResponse(DnsRecordsResponse()));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        var result = await service.SyncDnsRecordsAsync("1.2.3.4");

        Assert.Single(result);
        Assert.Equal("sub.example.com", result[0].Name);
        Assert.Equal("zone-1", result[0].ZoneId);
    }

    [Fact]
    public async Task SyncDnsRecordsAsync_CreatesRecord_WhenNoRecordsExist()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupSequencedResponses(handlerMock,
            JsonResponse(ZoneResponse()),
            JsonResponse(EmptyResponse()),
            new HttpResponseMessage(HttpStatusCode.OK));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        var result = await service.SyncDnsRecordsAsync("1.2.3.4");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SyncDnsRecordsAsync_SkipsDomain_WhenCreateRecordFails()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupSequencedResponses(handlerMock,
            JsonResponse(ZoneResponse()),
            JsonResponse(EmptyResponse()),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        var result = await service.SyncDnsRecordsAsync("1.2.3.4");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SyncDnsRecordsAsync_ThrowsException_WhenZoneNotFound()
    {
        var emptyZoneResponse = new CloudflareResponse { Success = true, Result = [] };
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupResponse(handlerMock, JsonResponse(emptyZoneResponse));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        await Assert.ThrowsAsync<Exception>(() => service.SyncDnsRecordsAsync("1.2.3.4"));
    }

    [Fact]
    public async Task UpdateIpAddressAsync_DoesNotUpdate_WhenIpIsUnchanged()
    {
        var dnsRecord = new CloudflareDnsRecord { Id = "record-1", Name = "sub.example.com", ZoneId = "zone-1" };
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupResponse(handlerMock, JsonResponse(DnsRecordsResponse(ip: "1.2.3.4")));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        await service.UpdateIpAddressAsync(dnsRecord, "1.2.3.4");

        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateIpAddressAsync_UpdatesRecord_WhenIpHasChanged()
    {
        var dnsRecord = new CloudflareDnsRecord { Id = "record-1", Name = "sub.example.com", ZoneId = "zone-1" };
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupSequencedResponses(handlerMock,
            JsonResponse(DnsRecordsResponse(ip: "1.2.3.4")),
            new HttpResponseMessage(HttpStatusCode.OK));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        await service.UpdateIpAddressAsync(dnsRecord, "5.6.7.8");

        handlerMock.Protected().Verify("SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateIpAddressAsync_ThrowsException_WhenNoRecordFound()
    {
        var dnsRecord = new CloudflareDnsRecord { Id = "record-1", Name = "sub.example.com", ZoneId = "zone-1" };
        var handlerMock = new Mock<HttpMessageHandler>();
        SetupResponse(handlerMock, JsonResponse(EmptyResponse()));

        var service = new CloudflareService(CreateHttpClient(handlerMock.Object), Options.Create(_options));

        await Assert.ThrowsAsync<Exception>(() => service.UpdateIpAddressAsync(dnsRecord, "5.6.7.8"));
    }
}
