namespace CloudflareDynamicDns.Core.Models;

public class CloudflareDnsRecord
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Content { get; init; }
    public string? ZoneId { get; set; }
    public int? Ttl { get; init; }
    public bool? Proxied { get; init; }
}