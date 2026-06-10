using Microsoft.Extensions.Caching.Memory;

namespace MoodPlaylistApi.Services
{
    public interface ICacheService
    {
        Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl);
        void Remove(string key);
    }

    public sealed class CacheService(IMemoryCache cache) : ICacheService
    {
        public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
        {
            if (cache.TryGetValue(key, out T cached))
                return cached;

            var result = await factory();
            cache.Set(key, result, ttl);
            return result;
        }

        public void Remove(string key) => cache.Remove(key);
    }

}
