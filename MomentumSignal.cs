
using EveTrading.EveApi;

public class MomentumSignal {

    public static MomentumSignal Analyse(CommoditySummarySeries summary) {

        var shortWindow = 5;
        var longWindow = 20;
        var period = 20;

        var fiveDay = MovingAverage(summary, 20, 5);
        var twentyDay = MovingAverage(summary, 20, 20);
        var differences = Enumerable.Zip(fiveDay, twentyDay, (a, b) => a - b );
        var ratios = Enumerable.Zip(twentyDay, differences, (baseline, diff) => diff / baseline);
        var meanChange = ratios.Average();

        var signal = new MomentumSignal();
        signal.Name = summary.Name;
        signal.ShortWindow = shortWindow;
        signal.LongWindow = longWindow;
        signal.Period = period;
        signal.MeanChange = meanChange;

        return signal;
    }

    private static List<decimal> MovingAverage(CommoditySummarySeries summary, int period, int window) {
        var movingAverages = new List<decimal>();
        for (var i = 0; i < period; i++) {
            var start = summary.Series.Count - period - window + i;
            var avg = summary.Series.Skip(start).Take(window).Average(o => o.Average);
            movingAverages.Add(avg);
        }
        return movingAverages;
    }

    public string Name { get; private set; }
    public int Period { get; private set; }
    public int ShortWindow { get; private set; }
    public int LongWindow { get; private set; }
    public decimal MeanChange { get; private set; }

    private MomentumSignal() { }

    public override string ToString() {
        return $"{this.Name} {this.ShortWindow} / {this.LongWindow} day: {this.MeanChange:P}";
    }

}