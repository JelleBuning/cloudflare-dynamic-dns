using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services.Interfaces;
using Microsoft.Extensions.Options;
using CloudflareDnsRecord = CloudflareDynamicDns.Core.Models.CloudflareDnsRecord;

namespace CloudflareDynamicDns.Core.Services;

public class CloudflareService : ICloudflareService
{
    private readonly CloudflareOptions _config;
    private readonly HttpClient _httpClient;
    
    public CloudflareService(HttpClient httpClient, IOptions<CloudflareOptions> cloudflareOptions)
    {
        _config = cloudflareOptions.Value;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiToken);
    }

    public async Task<List<CloudflareDnsRecord>> GetDnsRecordsAsync()
    {
        var url = $"zones/{_config.ZoneId}/dns_records";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CloudflareResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Result.Where(x => _config.DomainName.Split(',').Contains(x.Name)).ToList() ?? throw new Exception("Domain name not found!");
    }
    
    public async Task UpdateIpAddressAsync(string domainName, string ip)
    {
        var encodedName = System.Net.WebUtility.UrlEncode(domainName);
        var listUrl = $"zones/{_config.ZoneId}/dns_records?name={encodedName}&type=A";
    
        var listResponse = await _httpClient.GetAsync(listUrl);
        listResponse.EnsureSuccessStatusCode();

        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<CloudflareResponse>(listJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var existingRecord = listResult?.Result?.FirstOrDefault();

        if (existingRecord == null)
        {
            throw new Exception($"No existing A record found for {domainName}. Cannot update.");
        }

        if (existingRecord.Content == ip)
        {
            return;
        }

        var recordId = existingRecord.Id;
        var updateUrl = $"zones/{_config.ZoneId}/dns_records/{recordId}";

        var updatePayload = new
        {
            type = "A",
            name = domainName,
            content = ip,
            ttl = existingRecord.Ttl,
            proxied = existingRecord.Proxied
        };
    
        var updateResponse = await _httpClient.PatchAsJsonAsync(updateUrl, updatePayload);
        updateResponse.EnsureSuccessStatusCode();
    }
}