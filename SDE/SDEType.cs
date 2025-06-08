using EveTrading.SDE;

public class SDEType {

    public int Id { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }

    public static SDEType Load(KeyValuePair<int, SDETypeDataModel> entry) {
        var type = new SDEType();
        type.Id = entry.Key;
        type.Name = entry.Value.Name.En;
        return type;
    }

}