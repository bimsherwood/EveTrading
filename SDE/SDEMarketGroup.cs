using EveTrading.SDE;

public class SDEMarketGroup {

    public int Id { get; set; }
    public int ParentGroup { get; set; }
    public string Name { get; set; }

    public static SDEMarketGroup Load(KeyValuePair<int, SDEMarketGroupDataModel> entry) {
        var type = new SDEMarketGroup();
        type.Id = entry.Key;
        type.ParentGroup = entry.Value.ParentGroupId;
        type.Name = entry.Value.NameId.En;
        return type;
    }

}