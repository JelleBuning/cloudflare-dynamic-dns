using System.Net;
using CloudflareDynamicDns.Core.Fetchers;
using Moq;
using Moq.Protected;
using Xunit;

namespace CloudflareDynamicDns.Core.Tests.Fetchers;

public class CloudflareDiagnosticsFetcherTests
{
    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler);

    private static Mock<HttpMessageHandler> SetupHandler(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
        return handlerMock;
    }

    [Fact]
    public async Task FetchIpAddressAsync_ReturnsIpAddress_OnSuccess()
    {
        var traceResponse = "fl=1abc\nip=1.2.3.4\nts=1234567890\n";
        var handlerMock = SetupHandler(HttpStatusCode.OK, traceResponse);
        var fetcher = new CloudflareDiagnosticsFetcher(CreateHttpClient(handlerMock.Object));

        var result = await fetcher.FetchIpAddressAsync();

        Assert.Equal("1.2.3.4", result);
    }

    [Fact]
    public async Task FetchIpAddressAsync_ReturnsEmpty_OnHttpFailure()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        var fetcher = new CloudflareDiagnosticsFetcher(CreateHttpClient(handlerMock.Object));

        var result = await fetcher.FetchIpAddressAsync();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task FetchIpAddressAsync_ReturnsEmpty_WhenIpLineIsMissing()
    {
        var traceResponse = "fl=1abc\nts=1234567890\n";
        var handlerMock = SetupHandler(HttpStatusCode.OK, traceResponse);
        var fetcher = new CloudflareDiagnosticsFetcher(CreateHttpClient(handlerMock.Object));

        var result = await fetcher.FetchIpAddressAsync();

        Assert.Equal(string.Empty, result);
    }
}
