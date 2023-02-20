using Microsoft.Extensions.Caching.Memory;

namespace TCP_TCP.Server;

public class MyMemoryCache
{
    public MemoryCache Cache { get; } = new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = 1024
        });

    
}