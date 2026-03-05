using CloudflareDynamicDns.Core.Fetchers.Interfaces;

namespace CloudflareDynamicDns.Core.Fetchers;

public class IpifyFetcher(HttpClient httpClient) : IPublicIpAddressFetcher
{
    public async Task<string> FetchIpAddressAsync()
    {
        const string url = "https://api.ipify.org";
        try
        {
            var publicIp = await httpClient.GetStringAsync(url);
            return publicIp.Trim(); 
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error retrieving public IP: {ex.Message}");
            return "Error: Could not resolve public IP.";
        }
    }
}