using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CloudflareDynamicDns.Workers;

public class DnsRecordWorker(IPublicIpAddressFetcher publicIpAddressFetcher, ICloudflareService cloudflareService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ip = await publicIpAddressFetcher.FetchIpAddressAsync();
            var dnsRecords = await cloudflareService.GetDnsRecordsAsync();
            foreach (var cloudflareDnsRecord in dnsRecords.Where(dnsRecord => dnsRecord.Content != ip))
            {
                await cloudflareService.UpdateIpAddressAsync(cloudflareDnsRecord.Name, ip);
            }
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); 
        }
    }
}