namespace CloudflareDynamicDns.Core.Models;

public class CloudflareDnsRecord
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public int Ttl { get; set; }
    public bool Proxied { get; set; }
    public string ZoneId { get; set; }
}