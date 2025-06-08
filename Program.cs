
using EveTrading.SDE;
using Microsoft.Extensions.Configuration;

public class Program {

    public static async Task Main(string[] args) {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("config.json")
            .AddUserSecrets<Program>();
        var config = configBuilder.Build();
        var tokenStore = new EveTokenStore("token.json");
        var api = new EveApiAuth(config, tokenStore);

        var sde = await SDE.Load(config);
        var veldspar = sde.Commodities["Veldspar"];
        Console.WriteLine(veldspar.Id);

    }

    private static async Task GetTypes(EveApiAuth api) {
        var request = api.AuthenticatedRequest("/universe/types/");
        using var client = new HttpClient();
        var resposne = await client.SendAsync(request);
        var responseContent = await resposne.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
    }

}