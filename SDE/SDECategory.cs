using EveTrading.SDE;

public class SDECategory {

    public int Id { get; set; }
    public string Name { get; set; }

    public static SDECategory Load(KeyValuePair<int, SDECategoryDataModel> entry) {
        var type = new SDECategory();
        type.Id = entry.Key;
        type.Name = entry.Value.Name.En;
        return type;
    }

}