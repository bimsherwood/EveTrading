
using System.Drawing;
using EveTrading.EveApi;

public class CommodityPlot {

    private MomentumSignal Momentum;

    public CommodityPlot(MomentumSignal momentum) {
        this.Momentum = momentum;
    }

    public void Render(string fileName) {

        ScottPlot.Plot myPlot = new();

        // Lines
        var dailyAverage = this.Momentum.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.Day.Average })
            .ToArray();
        var largeWindowMovingAverage = this.Momentum.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.LargeWindowAverage })
            .ToArray();
        var smallWindowMovingAverage = this.Momentum.Series
            .Select((o, i) => new ScottPlot.Coordinates { X = i, Y = (double)o.SmallWindowAverage })
            .ToArray();
        myPlot.Add.ScatterLine(dailyAverage, ScottPlot.Color.FromColor(Color.Gray));
        myPlot.Add.ScatterLine(largeWindowMovingAverage, ScottPlot.Color.FromColor(Color.Blue));
        myPlot.Add.ScatterLine(smallWindowMovingAverage, ScottPlot.Color.FromColor(Color.Red));

        // Markers
        var buys = this.Momentum.Series
            .Select((o, i) => new { X = i, Y = (double)o.Day.Average, Signal = o.Signal })
            .Where(o => o.Signal == Signal.Buy)
            .Select(o => new ScottPlot.Coordinates { X = o.X, Y = o.Y })
            .ToArray();
        var sells = this.Momentum.Series
            .Select((o, i) => new { X = i, Y = (double)o.Day.Average, Signal = o.Signal })
            .Where(o => o.Signal == Signal.Sell)
            .Select(o => new ScottPlot.Coordinates { X = o.X, Y = o.Y })
            .ToArray();
        foreach (var buy in buys) {
            myPlot.Add.Marker(buy, ScottPlot.MarkerShape.FilledSquare, size: 20, color: ScottPlot.Color.FromColor(Color.Blue));
        }
        foreach (var sell in sells) {
            myPlot.Add.Marker(sell, ScottPlot.MarkerShape.Cross, size: 20, color: ScottPlot.Color.FromColor(Color.Red));
        }

        myPlot.SavePng(fileName, 3200, 1200);
    }

}
