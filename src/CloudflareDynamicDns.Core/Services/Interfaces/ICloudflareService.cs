using CloudflareDynamicDns.Core.Models;

namespace CloudflareDynamicDns.Core.Services.Interfaces;

public interface ICloudflareService
{
    Task<List<CloudflareDnsRecord>> GetDnsRecordsAsync();
    Task UpdateIpAddressAsync(string domainName, string ip);
}