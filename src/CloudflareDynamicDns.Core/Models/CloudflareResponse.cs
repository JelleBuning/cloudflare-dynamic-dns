namespace CloudflareDynamicDns.Core.Models;

public class CloudflareResponse
{
    public bool Success { get; set; }
    public List<object> Errors { get; set; }
    public List<object> Messages { get; set; }
    public List<CloudflareDnsRecord> Result { get; set; }
}