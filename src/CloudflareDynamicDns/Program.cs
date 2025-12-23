using Polly;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CloudflareDynamicDns.Core.Fetchers;
using CloudflareDynamicDns.Core.Fetchers.Interfaces;
using CloudflareDynamicDns.Core.Models;
using CloudflareDynamicDns.Core.Services;
using CloudflareDynamicDns.Core.Services.Interfaces;
using CloudflareDynamicDns.Workers;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CloudflareOptions>(options => {
            options.ApiToken = hostContext.Configuration.GetValue<string>("CF_API_TOKEN") ?? throw new Exception("CF_API_TOKEN is missing");
            options.DomainNames = hostContext.Configuration.GetValue<string>("CF_DOMAIN_NAMES")?
                .Replace(" ", "").Split(',')
                .ToList() ?? throw new Exception("CF_DOMAIN_NAMES is missing");
            options.IntervalMinutes = hostContext.Configuration.GetValue("INTERVAL_MINUTES", 15);
        });

        services.AddTransient<IPublicIpAddressFetcher, PublicIpAddressFetcher>();
        services.AddHttpClient<ICloudflareService, CloudflareService>()
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.UseJitter = true;
                options.Retry.ShouldRetryAfterHeader = true;
                options.Retry.MaxRetryAttempts = 10;
                options.Retry.Delay = TimeSpan.FromSeconds(10);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
            });

        services.AddHostedService<DnsRecordWorker>();
    })
    .Build()
    .Run();