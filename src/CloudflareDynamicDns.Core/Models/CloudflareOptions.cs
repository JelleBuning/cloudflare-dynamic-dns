namespace CloudflareDynamicDns.Core.Models;

public class CloudflareOptions
{
    public required string ApiToken { get; set; }
    public required List<string> DomainNames { get; set; }
    public int IntervalMinutes { get; set; }
}