
using System.Drawing;
using EveTrading.EveApi;

public class CommodityPlot {

    private CommoditySummarySeries Commodity;
    private IOrderSequence Orders;

    public CommodityPlot(CommoditySummarySeries commodity, IOrderSequence orders) {
        this.Commodity = commodity;
        this.Orders = orders;
    }

    public void Render(string fileName) {

        ScottPlot.Plot plot = new();

        // Lines
        var dailyAverage = this.Commodity.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.Average })
            .ToArray();
        var priceLine = plot.Add.ScatterLine(dailyAverage, ScottPlot.Color.FromColor(Color.Gray));
        priceLine.Axes.YAxis = plot.Axes.Right;

        // Markers
        var buys = this.Orders.Orders
            .Select((o, i) => new { X = i, Y = (double)o.BuyPrice, Signal = o.Signal })
            .Where(o => o.Signal == Signal.Buy)
            .Select(o => new ScottPlot.Coordinates { X = o.X, Y = o.Y })
            .ToArray();
        var sells = this.Orders.Orders
            .Select((o, i) => new { X = i, Y = (double)o.SellPrice, Signal = o.Signal })
            .Where(o => o.Signal == Signal.Sell)
            .Select(o => new ScottPlot.Coordinates { X = o.X, Y = o.Y })
            .ToArray();
        foreach (var buy in buys) {
            var marker = plot.Add.Marker(
                buy,
                ScottPlot.MarkerShape.FilledSquare,
                size: 20,
                color: ScottPlot.Color.FromColor(Color.Blue));
            marker.Axes.YAxis = plot.Axes.Right;
        }
        foreach (var sell in sells) {
            var marker = plot.Add.Marker(
                sell,
                ScottPlot.MarkerShape.FilledSquare,
                size: 20,
                color: ScottPlot.Color.FromColor(Color.Red));
            marker.Axes.YAxis = plot.Axes.Right;
        }

        // Axis
        //plot.Axes.Left.IsVisible = false;

        plot.SavePng(fileName, 3200, 1200);
    }

}
