
using System.Diagnostics;
using EveTrading.EveApi;
using EveTrading.SDE;
using Microsoft.Extensions.Configuration;

public class Program {

    public static async Task Main(string[] args) {

        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("config.json")
            .AddUserSecrets<Program>();
        var config = configBuilder.Build();
        using var httpClient = new HttpClient();
        var tokenStore = new EveTokenStore("token.json");
        var auth = new EveApiAuth(httpClient, config, tokenStore);
        var sde = await SDE.Load(config);
        var api = new EveApi(sde, httpClient, auth);

        var program = new Program(sde, api, config);
        await program.Run(args);

    }

    private readonly SDE Sde;
    private readonly PriceCache PriceCache;
    private readonly IConfiguration Config;
    private readonly string[] Commodities;

    public Program(SDE sde, EveApi api, IConfiguration config) {
        this.Sde = sde;
        this.Config = config;
        this.PriceCache = new PriceCache(api, config);
        this.Commodities = new[]{
            "Chiral Structures",
            "Silicon",
            "Precious Metals",
            "Reactive Metals",
            "Toxic Metals",
            "Mechanical Parts",
            "Consumer Electronics",
            "Construction Blocks"
        };

    }

    private async Task Run(string[] args) {
        var instruction = args.FirstOrDefault()?.ToLower();
        switch (instruction) {
            case "recent":
                await Recents();
                break;
            case "swingtest":
                await SwingTest(args.Skip(1).ToArray());
                break;
            case "plot":
                await DrawPlot(args.Skip(1).ToArray());
                break;
            default:
                await ShowHelp();
                break;
        }
    }

    private async Task ShowHelp() {
        Console.WriteLine("Usage:");
        Console.WriteLine("EveTrading.exe recent");
        Console.WriteLine("EveTrading.exe swingtest <Commodity>");
        Console.WriteLine("EveTrading.exe plot <Commodity>");
    }

    private async Task Recents() {
        foreach (var commodity in this.Commodities) {
            Console.WriteLine($"=== {commodity} ===");
            Console.WriteLine();
            var localPrices = await this.PriceCache.LoadCommodity(this.Sde.SinqLaisonId, commodity);
            var localAnalysis = RecentStatistics.Analyse(localPrices);
            Console.WriteLine("Local:");
            Console.WriteLine(localAnalysis);
            Console.WriteLine();
            var centralPrices = await this.PriceCache.LoadCommodity(this.Sde.TheForgeId, commodity);
            var centralAnalysis = RecentStatistics.Analyse(centralPrices);
            Console.WriteLine("Central:");
            Console.WriteLine(centralAnalysis);
            Console.WriteLine();
        }
    }

    private async Task SwingTest(string[] args) {

        var commodity = args.FirstOrDefault() ?? "(none)";
        if (!this.Sde.Commodities.TryGetValue(commodity, out var _)) {
            Console.WriteLine($"Commodity {commodity} not known.");
            return;
        }

        var centralPrices = await this.PriceCache.LoadCommodity(this.Sde.TheForgeId, commodity);
        var centralOrders = SwingSignal.Analyse(centralPrices, 90);

        var startingCash = 1000 * 1000 * 1000;
        var taxRate = 0.03m;
        var backTest = new BackTest(centralOrders);
        var finalCash = backTest.TradeWith(startingCash, taxRate);
        Console.WriteLine($"Back testing {commodity} with {startingCash:C} tax rate {taxRate:P}: {finalCash:C}");

    }

    private async Task DrawPlot(string[] args) {

        var commodity = args.FirstOrDefault() ?? "(none)";
        if (!this.Sde.Commodities.TryGetValue(commodity, out var _)) {
            Console.WriteLine($"Commodity {commodity} not known.");
            return;
        }

        var centralPrices = await this.PriceCache.LoadCommodity(this.Sde.TheForgeId, commodity);
        var centralOrders = SwingSignal.Analyse(centralPrices, 90);

        var graphOutputFolder = this.Config["GraphOutputFolder"]!;
        var graphOutputFile = Path.Combine(graphOutputFolder, $"{commodity}.png");
        var plot = new CommodityPlot(centralPrices, centralOrders);
        plot.Render(graphOutputFile);

        var recentSignals = centralOrders
            .Orders
            .TakeLast(5)
            .Where(o => o.Signal != Signal.Hold)
            .ToArray();
        if (recentSignals.Any(o => o.Signal != Signal.Hold)) {
            Console.WriteLine($"Attention: {commodity}");
        }

    }

}