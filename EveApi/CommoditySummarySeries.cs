
namespace EveTrading.EveApi;

public class CommoditySummarySeries {

    public string Name { get; set; }
    public List<CommoditySummaryDay> Series { get; set; }

    public CommoditySummarySeries Slice(int start, int count) {
        var sliced = new CommoditySummarySeries();
        sliced.Name = Name;
        sliced.Series = Series.Skip(start).Take(count).ToList();
        return sliced;
    }

}