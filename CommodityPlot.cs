
using EveTrading.EveApi;

public class CommodityPlot {

    private CommoditySummarySeries Commodity;

    public CommodityPlot(CommoditySummarySeries commodity) {
        this.Commodity = commodity;
    }

    public void Render(string fileName) {
        var lowCoords = this.Commodity.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.Lowest })
            .ToArray();
        var highCoords = this.Commodity.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.Highest })
            .ToArray();
        ScottPlot.Plot myPlot = new();
        myPlot.Add.ScatterLine(lowCoords);
        myPlot.Add.ScatterLine(highCoords);
        myPlot.SavePng(fileName, 3200, 1200);
    }

}
