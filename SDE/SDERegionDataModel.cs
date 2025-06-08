using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDERegionDataModel {

    [YamlMember(Alias = "regionID")]
    public int RegionId { get; set; }

}