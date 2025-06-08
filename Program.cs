
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
        var veldsparPrices = await api.GetPriceHistory(sde.TheForgeId, "Veldspar");
        foreach (var price in veldsparPrices) {
            Console.WriteLine("{0:yyyy-MM-dd} - {1}", price.Date, price.Average);
        }
    }

}