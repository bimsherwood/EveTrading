using YamlDotNet.Serialization;

namespace EveTrading.SDE;

public class L10nString {

    [YamlMember(Alias = "en")]
    public string En { get; set; }

}