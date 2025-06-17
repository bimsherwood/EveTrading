
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDE {

    private static IDeserializer Yaml;

    static SDE() {
        Yaml = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    }

    public static async Task<SDE> Load(IConfiguration config) {

        var sdeRoot = config["SDERootPath"] ?? throw new InvalidOperationException("SDERootPath configuration is required.");

        // Load types and market groups
        var groupFile = Path.Combine(sdeRoot, "fsd/marketGroups.yaml");
        var typeFile = Path.Combine(sdeRoot, "fsd/types.yaml");
        var marketGroupDataModels = await LoadIndexedListFile<SDEMarketGroupDataModel>(groupFile);
        var typeDataModels = await LoadIndexedListFile<SDETypeDataModel>(typeFile);
        var marketGroups = marketGroupDataModels.Select(SDEMarketGroup.Load).ToDictionary(o => o.Id);
        var commodities = typeDataModels
            .Where(o => o.Value.MarketGroupId is not null)
            .Select(o => SDEType.Load(marketGroups, o));

        // Load regions
        var theForge = await LoadEveRegion(sdeRoot, "TheForge");
        var sinqLaison = await LoadEveRegion(sdeRoot, "SinqLaison");

        var sde = new SDE();
        sde.TheForgeId = theForge.RegionId;
        sde.SinqLaisonId = sinqLaison.RegionId;
        sde.Commodities = commodities
            .DistinctBy(o => o.Name) // TODO: This elides some types that appear twice with the same name.
            .ToDictionary(o => o.Name);

        return sde;

    }

    private static async Task<Dictionary<int, T>> LoadIndexedListFile<T>(string filePath) {
        var content = await File.ReadAllTextAsync(filePath);
        var types = Yaml.Deserialize<Dictionary<int, T>>(content);
        return types;
    }

    private static async Task<SDERegionDataModel> LoadEveRegion(string sdeRoot, string regionName) {
        var regionFilePath = Path.Combine(sdeRoot, $"universe/eve/{regionName}/region.yaml");
        var content = await File.ReadAllTextAsync(regionFilePath);
        var region = Yaml.Deserialize<SDERegionDataModel>(content);
        return region;
    }

    public int TheForgeId { get; private set; }
    public int SinqLaisonId { get; private set; }
    public Dictionary<string, SDEType> Commodities { get; private set; }

    private SDE() { }

}