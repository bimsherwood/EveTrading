
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

        var program = new Program(sde, api);
        await program.Run(args);

    }

    private readonly SDE Sde;
    private readonly EveApi Api;
    private readonly string[] Commodities;

    public Program(SDE sde, EveApi api) {
        this.Sde = sde;
        this.Api = api;

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
            case "summary":
                await Summaries();
                break;
            case "momentum":
                await Momentum();
                break;
            default:
                await ShowHelp();
                break;
        }
    }

    private async Task ShowHelp() {
        Console.WriteLine("Usage:");
        Console.WriteLine("EveTrading.exe summary");
        Console.WriteLine("EveTrading.exe momentum");
    }

    private async Task Summaries() {
        foreach (var commodity in this.Commodities) {
            Console.WriteLine($"=== {commodity} ===");
            Console.WriteLine();
            var localPrices = await this.Api.GetPriceHistory(this.Sde.SinqLaisonId, commodity);
            var localAnalysis = Analysis.Analyse(localPrices);
            Console.WriteLine("Local:");
            Console.WriteLine(localAnalysis);
            Console.WriteLine();
            var centralPrices = await this.Api.GetPriceHistory(this.Sde.TheForgeId, commodity);
            var centralAnalysis = Analysis.Analyse(centralPrices);
            Console.WriteLine("Central:");
            Console.WriteLine(centralAnalysis);
            Console.WriteLine();
        }
    }

    private async Task Momentum() {
        var localSignals = new List<MomentumSignal>();
        var centralSignals = new List<MomentumSignal>();
        foreach (var commodity in this.Commodities) {
            var localPrices = await this.Api.GetPriceHistory(this.Sde.SinqLaisonId, commodity);
            var localMomentum = MomentumSignal.Analyse(localPrices);
            localSignals.Add(localMomentum);
            var centralPrices = await this.Api.GetPriceHistory(this.Sde.TheForgeId, commodity);
            var centralMomentum = MomentumSignal.Analyse(centralPrices);
            centralSignals.Add(centralMomentum);
        }
        var bestLocalUp = localSignals.MaxBy(o => o.MeanChange);
        Console.WriteLine($"Best Local Momentum Up: {bestLocalUp}");
        var bestLocalDown = localSignals.MinBy(o => o.MeanChange);
        Console.WriteLine($"Best Local Momentum Down: {bestLocalDown}");
        var bestCentralUp = centralSignals.MaxBy(o => o.MeanChange);
        Console.WriteLine($"Best Central Momentum Up: {bestCentralUp}");
        var bestCentralDown = centralSignals.MinBy(o => o.MeanChange);
        Console.WriteLine($"Best Central Momentum Down: {bestCentralDown}");
    }

}