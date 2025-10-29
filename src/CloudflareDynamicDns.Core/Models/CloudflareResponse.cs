namespace CloudflareDynamicDns.Core.Models;

public class CloudflareResponse
{
    public bool Success { get; init; }
    public List<object> Errors { get; init; } = [];
    public List<object> Messages { get; init; } = [];
    public List<CloudflareDnsRecord> Result { get; init; } = [];
}