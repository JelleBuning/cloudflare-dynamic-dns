using CloudflareDynamicDns.Core.Fetchers;
using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services;
using CloudflareDynamicDns.Core.Services.Interfaces;
using CloudflareDynamicDns.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CloudflareOptions>(options => {
            options.ZoneId = hostContext.Configuration["CF_ZONE_ID"]!;
            options.ApiToken = hostContext.Configuration["CF_API_TOKEN"]!;
            options.DomainName = hostContext.Configuration["CF_DOMAIN_NAMES"]!;
        });
        services.AddTransient<IPublicIpAddressFetcher, PublicIpAddressFetcher>();
        services.AddHttpClient<ICloudflareService, CloudflareService>();
        services.AddHostedService<DnsRecordWorker>();
    })
    .Build()
    .Run();
