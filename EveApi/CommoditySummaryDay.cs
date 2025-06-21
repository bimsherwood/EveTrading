
namespace EveTrading.EveApi;

public class CommoditySummaryDay {
    public DateTime Date { get; set; }
    public decimal Average { get; set; }
    public decimal Highest { get; set; }
    public decimal Lowest { get; set; }
    public long OrderCount { get; set; }
    public long Volume { get; set; }
}