using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDEMarketGroupDataModel {

    [YamlMember(Alias = "nameID")]
    public L10nString NameId { get; set; }

    [YamlMember(Alias = "parentGroupID")]
    public int ParentGroupId { get; set; }

}