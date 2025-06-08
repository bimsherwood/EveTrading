using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDETypeDataModel {

    [YamlMember(Alias = "name")]
    public L10nString Name { get; set; }

    [YamlMember(Alias = "marketGroupID")]
    public int? MarketGroupId { get; set; }

}