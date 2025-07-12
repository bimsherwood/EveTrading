
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
        var priceCache = new PriceCache(api, config);

        var program = new Program(sde, api, priceCache, config);
        await program.Run(args);

    }

    private readonly SDE Sde;
    private readonly EveApi Api;
    private readonly PriceCache PriceCache;
    private readonly IConfiguration Config;

    public Program(SDE sde, EveApi api, PriceCache prices, IConfiguration config) {
        this.Sde = sde;
        this.PriceCache = prices;
        this.Api = api;
        this.Config = config;
    }

    private async Task Run(string[] args) {
        var instruction = args.FirstOrDefault()?.ToLower();
        var commodities = args.Length > 1
            ? args.Skip(1).ToArray()
            : this.Config.GetSection("Commodities").Get<string[]>();
        switch (instruction) {
            case "swingtest":
                await SwingTest(commodities);
                break;
            case "plot":
                await DrawPlot(commodities);
                break;
            case "assets":
                await EvaluateAssets(commodities);
                break;
            default:
                await ShowHelp();
                break;
        }
    }

    private async Task ShowHelp() {
        Console.WriteLine("Usage:");
        Console.WriteLine("EveTrading.exe assets [<commodity>]");
        Console.WriteLine("EveTrading.exe swingtest [<commodity>]");
        Console.WriteLine("EveTrading.exe plot [<commodity>]");
    }

    private async Task SwingTest(string[] commodities) {
        foreach (var commodity in commodities) {

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
    }

    private async Task DrawPlot(string[] commodities) {
        foreach (var commodity in commodities) {

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

    public async Task EvaluateAssets(string[] commodities) {

        // Commodity Totals
        var targetCommodities = new List<SDEType>();
        foreach (var commodity in commodities) {
            if (this.Sde.Commodities.TryGetValue(commodity, out var commodityModel)) {
                targetCommodities.Add(commodityModel);
            }
            else {
                Console.WriteLine($"Commodity {commodity} not known.");
            }
        }

        // Sum up total quantities
        var assets = await this.Api.GetAssets();
        var assetQuantities = assets
            .Join(targetCommodities, o => o.TypeId, o => o.Id, (i, j) => new KeyValuePair<string, long>(j.Name, i.Quantity))
            .GroupBy(o => o.Key, o => o.Value)
            .ToDictionary(o => o.Key, o => o.Sum());

        // Price the commodities
        var totalValue = 0m;
        foreach (var kv in assetQuantities) {
            var commodityName = kv.Key;
            var totalQuantity = kv.Value;
            var prices = await this.PriceCache.LoadCommodity(this.Sde.TheForgeId, commodityName);
            var price = prices.Series.LastOrDefault()?.Average;
            var value = price * totalQuantity;
            Console.WriteLine($"{commodityName} x {totalQuantity} @ {price:C} = {value:C}");
            totalValue += value ?? 0;
        }

        Console.WriteLine($"Total: {totalValue:C}");

    }

}