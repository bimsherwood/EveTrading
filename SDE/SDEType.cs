using EveTrading.SDE;

public class SDEType {

    public int Id { get; set; }
    public string Name { get; set; }
    public string? MarketGroup { get; set; }

    public static SDEType Load(Dictionary<int, SDEMarketGroup> marketGroups, KeyValuePair<int, SDETypeDataModel> entry) {
        var type = new SDEType();
        type.Id = entry.Key;
        type.Name = entry.Value.Name.En;
        if (entry.Value.MarketGroupId is int groupId && marketGroups.TryGetValue(groupId, out var marketGroup)) {
            type.MarketGroup = marketGroup.Name;
        } else {
            type.MarketGroup = null;
        }
        return type;
    }

}