namespace CloudflareDynamicDns.Core.Fetchers.Interfaces;

public interface IPublicIpAddressFetcher
{
    Task<string> FetchIpAddressAsync();
}