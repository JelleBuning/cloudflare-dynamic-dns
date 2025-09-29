using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CloudflareDynamicDns.Workers;

public class DnsRecordWorker(IPublicIpAddressFetcher publicIpAddressFetcher, ICloudflareService cloudflareService, IOptions<CloudflareOptions> cloudflareOptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ip = await publicIpAddressFetcher.FetchIpAddressAsync();
            var dnsRecords = await cloudflareService.GetDnsRecordsAsync();
            foreach (var cloudflareDnsRecord in dnsRecords.Where(dnsRecord => dnsRecord.Content != ip))
            {
                await cloudflareService.UpdateIpAddressAsync(cloudflareDnsRecord, ip);
            }

            await Task.Delay(TimeSpan.FromMinutes(cloudflareOptions.Value.IntervalMinutes), stoppingToken);
        }
    }
}