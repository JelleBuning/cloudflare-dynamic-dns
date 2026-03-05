using CloudflareDynamicDns.Core.Fetchers.Interfaces;

namespace CloudflareDynamicDns.Core.Fetchers;

public class CloudflareDiagnosticsFetcher(HttpClient httpClient) : IPublicIpAddressFetcher
{
    private const string TraceUrl = "https://1.1.1.1/cdn-cgi/trace";
    private const string IpKey = "ip=";

    public async Task<string> FetchIpAddressAsync()
    {
        try
        {
            var response = await httpClient.GetStringAsync(TraceUrl);
            return ExtractIpAddress(response);
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
    }

    private static string ExtractIpAddress(string content)
    {
        var lines = content.Split('\n');
        var ipLine = lines.FirstOrDefault(l => l.StartsWith(IpKey));

        return ipLine?.Replace(IpKey, string.Empty).Trim() ?? string.Empty;
    }
}