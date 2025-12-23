using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CloudflareDynamicDns.Core.Services;

public class CloudflareService : ICloudflareService
{
    private readonly CloudflareOptions _config;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public CloudflareService(HttpClient httpClient, IOptions<CloudflareOptions> cloudflareOptions)
    {
        _config = cloudflareOptions.Value;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiToken);
    }

    private async Task<string> GetZoneIdAsync(string domain)
    {
        var baseDomain = GetBaseDomain(domain);
        var url = $"zones?name={baseDomain}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<CloudflareResponse>(json, _jsonSerializerOptions);

        var zone = result?.Result.SingleOrDefault(x => x.Name.Equals(baseDomain, StringComparison.OrdinalIgnoreCase));
        return zone?.Id ?? throw new Exception("Zone ID not found!");
    }
    
    public async Task<List<CloudflareDnsRecord>> SyncDnsRecordsAsync(string ip)
    {
        var cloudflareDnsRecords = new List<CloudflareDnsRecord>();
        foreach (var domainName in _config.DomainNames)
        {
            var zoneId = await GetZoneIdAsync(domainName);
            var url = $"zones/{zoneId}/dns_records?name={domainName}";
            var responseMessage = await _httpClient.GetAsync(url);
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CloudflareResponse>(json, _jsonSerializerOptions);
            if (result == null) continue;

            if (result.Result.Count == 0 && result.Errors.Count == 0)
            {
                var status = await CreateRecordAsync(domainName, ip, zoneId);
                if(status != HttpStatusCode.OK) continue;
            }
            
            result.Result.ForEach(x => x.ZoneId = zoneId);
            cloudflareDnsRecords.AddRange(result.Result);
        }
        return cloudflareDnsRecords;
    }

    private async Task<HttpStatusCode> CreateRecordAsync(string domainName, string ip, string zoneId)
    {
        var url =  $"zones/{zoneId}/dns_records";
        var createPayload = new
        {
            name = domainName,
            ttl = 1, // auto
            type = "A",
            content = ip,
            proxied = false
        };
        var jsonPayload = JsonSerializer.Serialize(createPayload);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        return response.StatusCode;
    }

    public async Task UpdateIpAddressAsync(CloudflareDnsRecord dnsRecord, string ip)
    {
        var encodedName = WebUtility.UrlEncode(dnsRecord.Name);
        var listUrl = $"zones/{dnsRecord.ZoneId}/dns_records?name={encodedName}&type=A";
    
        var listResponse = await _httpClient.GetAsync(listUrl);
        listResponse.EnsureSuccessStatusCode();

        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<CloudflareResponse>(listJson, _jsonSerializerOptions);

        var existingRecord = listResult?.Result.FirstOrDefault() 
                             ?? throw new Exception($"No existing A record found for {dnsRecord}. Cannot update.");
        
        if (existingRecord.Content == ip)
        {
            return;
        }

        var recordId = existingRecord.Id;
        var updateUrl = $"zones/{dnsRecord.ZoneId}/dns_records/{recordId}";

        var updatePayload = new
        {
            type = "A",
            name = dnsRecord.Name,
            content = ip,
            ttl = existingRecord.Ttl,
            proxied = existingRecord.Proxied
        };
    
        var updateResponse = await _httpClient.PatchAsJsonAsync(updateUrl, updatePayload);
        updateResponse.EnsureSuccessStatusCode();
    }
    
    private static string GetBaseDomain(string domainName)
    {
        if (!domainName.Contains("://"))
        {
            domainName = "https://" + domainName;
        }
        Uri.TryCreate(domainName, UriKind.Absolute, out var uri);
        if(uri == null) throw new Exception("Invalid domain name!");
        var host = uri.Host;
        var parts = host.Split('.');
        return parts.Length >= 2 ? string.Join(".", parts.Skip(parts.Length - 2).Take(2)) : host;
    }
}