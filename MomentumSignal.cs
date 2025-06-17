
using EveTrading.EveApi;

public class MomentumSignal {

    public static MomentumSignal Analyse(CommoditySummarySeries summary) {

        var smallWindowSize = 5;
        var largeWindowSize = 20;

        var smallMovingAvg = MovingAverage(summary, 5);
        var largeMovingAvg = MovingAverage(summary, 20);
        var differences = Enumerable.Zip(smallMovingAvg, largeMovingAvg, (f, t) => (f - t) / t).ToList();

        // Construct the moving averages
        var series = new List<MomentumDay>();
        for (var i = 0; i < summary.Series.Count; i++) {
            var day = new MomentumDay();
            day.Day = summary.Series[i];
            day.LargeWindowAverage = largeMovingAvg[i];
            day.SmallWindowAverage = smallMovingAvg[i];
            day.DifferencePercent = differences[i];
            day.Signal = Signal.Hold;
            series.Add(day);
        }

        // Annotate signals
        for (var i = 1; i < series.Count; i++) {

            var yesterday = series[i - 1];
            var today = series[i];

            // When the small average overtakes the large average, positive momentum
            var invertUp =
                yesterday.SmallWindowAverage < yesterday.LargeWindowAverage &&
                today.SmallWindowAverage >= today.LargeWindowAverage;
            if (invertUp) {
                today.Signal = Signal.Buy;
            }
            
            // When the small average overtakes the large average, negative momentum
            var invertDown =
                yesterday.SmallWindowAverage >= yesterday.LargeWindowAverage &&
                today.SmallWindowAverage < today.LargeWindowAverage;
            if (invertDown) {
                today.Signal = Signal.Sell;
            }

        }

        var signal = new MomentumSignal();
        signal.Name = summary.Name;
        signal.SmallWindow = smallWindowSize;
        signal.LargeWindow = largeWindowSize;
        signal.Series = series;

        return signal;

    }

    private static List<decimal> MovingAverage(CommoditySummarySeries summary, int window) {
        var movingAverages = new List<decimal>();
        for (var i = 0; i < summary.Series.Count; i++) {
            var truncatedWindow = Math.Min(i + 1, window);
            var start = i - truncatedWindow + 1;
            var avg = summary.Series.Skip(start).Take(truncatedWindow).Average(o => o.Average);
            movingAverages.Add(avg);
        }
        return movingAverages;
    }

    public string Name { get; private set; }
    public int SmallWindow { get; private set; }
    public int LargeWindow { get; private set; }
    public List<MomentumDay> Series { get; private set; }

    private MomentumSignal() { }

}