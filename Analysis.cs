
using System.Text;
using EveTrading.EveApi;

public class Analysis {

    public static Analysis Analyse(CommoditySummarySeries summary) {

        var today = summary.Series.Last();
        var fiveDay = summary.Series.Skip(summary.Series.Count - 5).ToList();
        var tenDay = summary.Series.Skip(summary.Series.Count - 10).ToList();
        var twentyDay = summary.Series.Skip(summary.Series.Count - 20).ToList();
        var ninetyDay = summary.Series.Skip(summary.Series.Count - 90).ToList();

        var analysis = new Analysis();
        analysis.Name = summary.Name;

        analysis.NinetyDayHigh = ninetyDay.Select(o => o.Highest).Max();
        analysis.NinetyDayAverage = ninetyDay.Select(o => o.Average).Average();
        analysis.NinetyDayLow = ninetyDay.Select(o => o.Lowest).Min();
        analysis.TwentyDayAverage = twentyDay.Select(o => o.Average).Average();
        analysis.TenDayAverage = tenDay.Select(o => o.Average).Average();
        analysis.FiveDayAverage = fiveDay.Select(o => o.Average).Average();
        analysis.LastAverage = today.Average;

        analysis.AverageDailyMonetaryVolume = fiveDay.Select(o => o.Volume * o.Average).Average();
        analysis.AverageDailyRangeToMedian = fiveDay.Select(o => (o.Highest - o.Lowest) / o.Average).Average();

        return analysis;
    }

    public string Name { get; private set; }
    public decimal NinetyDayHigh { get; private set; }
    public decimal NinetyDayAverage { get; private set; }
    public decimal NinetyDayLow { get; private set; }
    public decimal TwentyDayAverage { get; set; }
    public decimal TenDayAverage { get; set; }
    public decimal FiveDayAverage { get; set; }
    public decimal LastAverage { get; set; }
    public decimal AverageDailyMonetaryVolume { get; private set; }
    public decimal AverageDailyRangeToMedian { get; private set; }

    private Analysis() { }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.AppendLine(this.Name);
        builder.AppendLine($"High: {this.NinetyDayHigh}");
        builder.AppendLine($"Low: {this.NinetyDayLow}");
        builder.AppendLine($"5 day Avg Daily Volume: {this.AverageDailyMonetaryVolume:C0}");
        builder.AppendLine($"5 day Avg Daily Range-to-Median: {this.AverageDailyRangeToMedian:P}");
        builder.AppendLine($"90 Day: {this.NinetyDayAverage:C2}");
        builder.AppendLine($"20 Day: {this.TwentyDayAverage:C2} ({(this.TwentyDayAverage - this.NinetyDayAverage)/this.NinetyDayAverage:P})");
        builder.AppendLine($"10 Day: {this.TenDayAverage:C2} ({(this.TenDayAverage - this.TwentyDayAverage)/this.TwentyDayAverage:P})");
        builder.AppendLine($" 5 Day: {this.FiveDayAverage:C2} ({(this.FiveDayAverage - this.TenDayAverage)/this.TenDayAverage:P})");
        builder.AppendLine($" 1 Day: {this.LastAverage:C2} ({(this.LastAverage - this.FiveDayAverage)/this.FiveDayAverage:P})");
        return builder.ToString();
    }

}