
using EveTrading.EveApi;

public class SwingSignal : IOrderSequence {

    public static SwingSignal Analyse(CommoditySummarySeries summary, int windowSize) {

        var minimums = MovingMinimum(summary, windowSize);
        var maximums = MovingMaximum(summary, windowSize);

        // Construct the moving averages
        var series = new List<SwingDay>();
        for (var i = 0; i < summary.Series.Count; i++) {
            var day = new SwingDay();
            day.Day = summary.Series[i];
            day.WindowMin = minimums[i];
            day.WindowMax = maximums[i];
            day.Signal = Signal.Hold;
            series.Add(day);
        }

        // Annotate signals
        for (var i = windowSize; i < series.Count; i++) {

            var today = series[i];

            // When the small average overtakes the large average, positive momentum
            var historicLow = today.Day.Average <= today.WindowMin;
            if (historicLow) {
                today.Signal = Signal.Buy;
            }

            // When the small average overtakes the large average, negative momentum
            var historicHigh = today.Day.Average >= today.WindowMax;
            if (historicHigh) {
                today.Signal = Signal.Sell;
            }

        }

        var signal = new SwingSignal();
        signal.Name = summary.Name;
        signal.Window = windowSize;
        signal.Series = series;

        return signal;

    }

    private static List<decimal> MovingMinimum(CommoditySummarySeries summary, int window) {
        var minimums = new List<decimal>();
        for (var i = 0; i < summary.Series.Count; i++) {
            var truncatedWindow = Math.Min(i + 1, window);
            var start = i - truncatedWindow + 1;
            var min = summary.Series.Skip(start).Take(truncatedWindow).Min(o => o.Average);
            minimums.Add(min);
        }
        return minimums;
    }

    private static List<decimal> MovingMaximum(CommoditySummarySeries summary, int window) {
        var maxmimums = new List<decimal>();
        for (var i = 0; i < summary.Series.Count; i++) {
            var truncatedWindow = Math.Min(i + 1, window);
            var start = i - truncatedWindow + 1;
            var max = summary.Series.Skip(start).Take(truncatedWindow).Max(o => o.Average);
            maxmimums.Add(max);
        }
        return maxmimums;
    }

    public string Name { get; private set; }
    public int Window { get; private set; }
    public List<SwingDay> Series { get; private set; }
    
    public List<Order> Orders => this.Series.Select(o => new Order(o)).ToList();
    public decimal FinalSalePrice => this.Series.Last().Day.Lowest;

    private SwingSignal() { }

}