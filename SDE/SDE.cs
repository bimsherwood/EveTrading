
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDE {

    public Dictionary<string, SDEType> Commodities { get; private set; }

    public static async Task<SDE> Load(IConfiguration config) {
        
        var marketGroupDataModels = await LoadGroups(config["SDEMarketGroupFile"]);
        var typeDataModels = await LoadTypes(config["SDETypeFile"]);

        var marketGroups = marketGroupDataModels.Select(SDEMarketGroup.Load).ToDictionary(o => o.Id);

        var sde = new SDE();
        sde.Commodities = new Dictionary<string, SDEType>();
        var marketTypeDataModels = typeDataModels.Where(o => o.Value.MarketGroupId != null);
        foreach(var kv in marketTypeDataModels){
            var id = kv.Key;
            var typeDataModel = kv.Value;
            var marketGroup = marketGroups[typeDataModel.MarketGroupId.Value];
            sde.Commodities[typeDataModel.Name.En] = new SDEType(){
                Id = id,
                Name = typeDataModel.Name.En,
                Group = marketGroup.Name
            };
        }
        
        return sde;

    }

    private static async Task<Dictionary<int, SDETypeDataModel>> LoadTypes(string filePath) {
        var content = await File.ReadAllTextAsync(filePath);
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var types = deserializer.Deserialize<Dictionary<int, SDETypeDataModel>>(content);
        return types;
    }

    private static async Task<Dictionary<int, SDEMarketGroupDataModel>> LoadGroups(string filePath) {
        var content = await File.ReadAllTextAsync(filePath);
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var types = deserializer.Deserialize<Dictionary<int, SDEMarketGroupDataModel>>(content);
        return types;
    }

    private static async Task<Dictionary<int, SDECategoryDataModel>> LoadCategories(string filePath) {
        var content = await File.ReadAllTextAsync(filePath);
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var types = deserializer.Deserialize<Dictionary<int, SDECategoryDataModel>>(content);
        return types;
    }

    private SDE() { }

}