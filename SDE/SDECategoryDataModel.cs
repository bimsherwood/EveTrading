using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class SDECategoryDataModel {

    [YamlMember(Alias = "name")]
    public L10nString Name { get; set; }

}