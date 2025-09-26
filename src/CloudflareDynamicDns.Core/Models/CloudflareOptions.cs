namespace CloudflareDynamicDns.Core.Models;

public class CloudflareOptions
{
    public required string ApiToken { get; set; }
    public required string ZoneId { get; set; }
    public required string DomainName { get; set; }
}