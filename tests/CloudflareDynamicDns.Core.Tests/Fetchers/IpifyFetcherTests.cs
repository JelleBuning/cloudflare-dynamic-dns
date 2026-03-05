using System.Net;
using CloudflareDynamicDns.Core.Fetchers;
using Moq;
using Moq.Protected;
using Xunit;

namespace CloudflareDynamicDns.Core.Tests.Fetchers;

public class IpifyFetcherTests
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
        var handlerMock = SetupHandler(HttpStatusCode.OK, "  1.2.3.4  ");
        var fetcher = new IpifyFetcher(CreateHttpClient(handlerMock.Object));

        var result = await fetcher.FetchIpAddressAsync();

        Assert.Equal("1.2.3.4", result);
    }

    [Fact]
    public async Task FetchIpAddressAsync_ReturnsErrorMessage_OnHttpFailure()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        var fetcher = new IpifyFetcher(CreateHttpClient(handlerMock.Object));

        var result = await fetcher.FetchIpAddressAsync();

        Assert.StartsWith("Error:", result);
    }
}
