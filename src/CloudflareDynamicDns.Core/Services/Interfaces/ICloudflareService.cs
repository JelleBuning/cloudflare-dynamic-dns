using CloudflareDynamicDns.Core.Models;

namespace CloudflareDynamicDns.Core.Services.Interfaces;

public interface ICloudflareService
{
    Task<List<CloudflareDnsRecord>> SyncDnsRecordsAsync(string ip);
    Task UpdateIpAddressAsync(CloudflareDnsRecord dnsRecord, string ip);
}