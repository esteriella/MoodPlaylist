using Microsoft.Extensions.Caching.Memory;
using MoodPlaylistApi.Services;

namespace MoodPlaylistApi.Tests.Services;

public sealed class CacheServiceTests
{
    [Fact(DisplayName = "Cache returns the stored value without invoking the factory again")]
    public async Task GetOrCreateAsync_ValueAlreadyCached_ReturnsCachedValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CacheService(memoryCache);
        var factoryCalls = 0;

        var first = await service.GetOrCreateAsync("key", () =>
        {
            factoryCalls++;
            return Task.FromResult("created");
        }, TimeSpan.FromMinutes(1));
        var second = await service.GetOrCreateAsync("key", () =>
        {
            factoryCalls++;
            return Task.FromResult("replacement");
        }, TimeSpan.FromMinutes(1));

        Assert.Equal("created", first);
        Assert.Equal("created", second);
        Assert.Equal(1, factoryCalls);
    }

    [Fact(DisplayName = "Cache removal causes the next request to invoke the factory")]
    public async Task Remove_ValueAlreadyCached_RemovesValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CacheService(memoryCache);
        await service.GetOrCreateAsync("key", () => Task.FromResult("first"), TimeSpan.FromMinutes(1));

        service.Remove("key");
        var result = await service.GetOrCreateAsync(
            "key", () => Task.FromResult("second"), TimeSpan.FromMinutes(1));

        Assert.Equal("second", result);
    }

    [Fact(DisplayName = "Cache does not treat a cached null as a reusable value")]
    public async Task GetOrCreateAsync_FactoryReturnsNull_InvokesFactoryOnNextRequest()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CacheService(memoryCache);
        var factoryCalls = 0;

        await service.GetOrCreateAsync<string?>("key", () =>
        {
            factoryCalls++;
            return Task.FromResult<string?>(null);
        }, TimeSpan.FromMinutes(1));
        await service.GetOrCreateAsync<string?>("key", () =>
        {
            factoryCalls++;
            return Task.FromResult<string?>(null);
        }, TimeSpan.FromMinutes(1));

        Assert.Equal(2, factoryCalls);
    }
}
