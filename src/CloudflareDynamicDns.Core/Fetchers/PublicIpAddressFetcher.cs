using CloudflareDynamicDns.Core.Fetchers.Interfaces;

namespace CloudflareDynamicDns.Core.Fetchers;

public class PublicIpAddressFetcher : IPublicIpAddressFetcher
{
    public async Task<string> FetchIpAddressAsync()
    {
        const string url = "https://api.ipify.org";
        using var client = new HttpClient();
        try
        {
            var publicIp = await client.GetStringAsync(url);
            return publicIp.Trim(); 
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error retrieving public IP: {ex.Message}");
            return "Error: Could not resolve public IP.";
        }
    }
}