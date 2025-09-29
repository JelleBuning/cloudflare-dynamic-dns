using CloudflareDynamicDns.Core.Fetchers;
using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services;
using CloudflareDynamicDns.Core.Services.Interfaces;
using CloudflareDynamicDns.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CloudflareOptions>(options => {
            options.ApiToken = hostContext.Configuration.GetValue<string>("CF_API_TOKEN") ?? throw new Exception("CF_API_TOKEN is missing");
            options.DomainNames = hostContext.Configuration.GetValue<string>("CF_DOMAIN_NAMES")?.Split(',').ToList() ?? throw new Exception("CF_DOMAIN_NAMES is missing");
            options.IntervalMinutes = hostContext.Configuration.GetValue("INTERVAL_MINUTES", 15);
        });
        services.AddTransient<IPublicIpAddressFetcher, PublicIpAddressFetcher>();
        services.AddHttpClient<ICloudflareService, CloudflareService>();
        services.AddHostedService<DnsRecordWorker>();
    })
    .Build()
    .Run();
