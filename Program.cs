
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
        await auth.RefreshLoginIfRequired();
        var sde = await SDE.Load(config);
        var api = new EveApi(sde, httpClient, auth);

        var program = new Program(sde, api, config);
        await program.Run(args);

    }

    private readonly SDE Sde;
    private readonly EveApi Api;
    private readonly IConfiguration Config;
    private readonly string[] Commodities;

    public Program(SDE sde, EveApi api, IConfiguration config) {
        this.Sde = sde;
        this.Api = api;
        this.Config = config;
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
            case "plot":
                await DrawPlot();
                break;
            default:
                await ShowHelp();
                break;
        }
    }

    private async Task ShowHelp() {
        Console.WriteLine("Usage:");
        Console.WriteLine("EveTrading.exe recent");
        Console.WriteLine("EveTrading.exe plot");
    }

    private async Task Recents() {
        foreach (var commodity in this.Commodities) {
            Console.WriteLine($"=== {commodity} ===");
            Console.WriteLine();
            var localPrices = await this.Api.GetPriceHistory(this.Sde.SinqLaisonId, commodity);
            var localAnalysis = RecentStatistics.Analyse(localPrices);
            Console.WriteLine("Local:");
            Console.WriteLine(localAnalysis);
            Console.WriteLine();
            var centralPrices = await this.Api.GetPriceHistory(this.Sde.TheForgeId, commodity);
            var centralAnalysis = RecentStatistics.Analyse(centralPrices);
            Console.WriteLine("Central:");
            Console.WriteLine(centralAnalysis);
            Console.WriteLine();
        }
    }

    private async Task DrawPlot() {

        var commodity = "Construction Blocks";
        var centralPrices = await this.Api.GetPriceHistory(this.Sde.TheForgeId, commodity);
        var centralPriceMomentum = MomentumSignal.Analyse(centralPrices);

        var backTest = new BackTest(centralPriceMomentum);
        var startingCash = 1000 * 1000 * 1000;
        var taxRate = 0.03m;
        var finalCash = backTest.TradeWith(startingCash, taxRate);
        Console.WriteLine($"Trading with {startingCash:C} tax rate {taxRate:P}: {finalCash:C}");

        var graphOutputFolder = this.Config["GraphOutputFolder"];
        var graphOutputFile = Path.Combine(graphOutputFolder, "EveTrading.png");
        var plot = new CommodityPlot(centralPriceMomentum);
        plot.Render(graphOutputFile);

        Process.Start(new ProcessStartInfo {
            UseShellExecute = true,
            FileName = graphOutputFile
        });

    }

}