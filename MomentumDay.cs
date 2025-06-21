
using EveTrading.EveApi;

public class MomentumDay {

    public CommoditySummaryDay Day { get; set; }
    public decimal SmallWindowAverage { get; set; }
    public decimal LargeWindowAverage { get; set; }
    public decimal DifferencePercent { get; set; }
    public Signal Signal { get; set; }

}