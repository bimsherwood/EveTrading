
using EveTrading.EveApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class PriceCache {

    private readonly EveApi Api;
    private readonly IConfiguration Config;

    private Dictionary<string, CommoditySummarySeries>? Cache;

    public PriceCache(EveApi api, IConfiguration config) {
        this.Api = api;
        this.Config = config;
        this.Cache = null;
    }

    public async Task<CommoditySummarySeries> LoadCommodity(int region, string commodity) {

        var key = $"{DateTime.Today:yyyy-MM-dd}|{region}|{commodity}";
        var cacheFile = this.Config["CacheFile"];

        // Load cache from disk if required and if available
        if (this.Cache == null) {
            if (File.Exists(cacheFile)) {
                var existingCachFileContent = await File.ReadAllTextAsync(cacheFile);
                this.Cache = JsonConvert.DeserializeObject<Dictionary<string, CommoditySummarySeries>>(existingCachFileContent);
            }
        }

        // Check the cache
        if (this.Cache == null) {
            this.Cache = new Dictionary<string, CommoditySummarySeries>();
        }
        if (this.Cache.TryGetValue(key, out var cachedSeries)) {
            return cachedSeries;
        }

        // Load the commodity from the API
        var series = await this.Api.GetPriceHistory(region, commodity);

        // Cache and save the series
        this.Cache[key] = series;
        var newCacheFileContent = JsonConvert.SerializeObject(this.Cache);
        await File.WriteAllTextAsync(cacheFile, newCacheFileContent);

        return series;

    }

}