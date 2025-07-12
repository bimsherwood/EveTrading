using EveTrading.SDE;
using Newtonsoft.Json;

namespace EveTrading.EveApi;

public class EveApi {

    private readonly SDE.SDE SDE;
    private readonly HttpClient HttpClient;
    private readonly EveApiAuth Auth;

    public EveApi(SDE.SDE sde, HttpClient client, EveApiAuth auth) {
        this.SDE = sde;
        this.HttpClient = client;
        this.Auth = auth;
    }

    public async Task<CommoditySummarySeries> GetPriceHistory(int regionId, string commodityName) {
        await this.Auth.RefreshLoginIfRequired();
        var commodity = this.SDE.Commodities[commodityName];
        var request = this.Auth.AuthenticatedRequest($"/latest/markets/{regionId}/history/?type_id={commodity.Id}");
        var response = await this.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<CommoditySummaryDay>>(responseString);
        return new CommoditySummarySeries {
            Name = commodityName,
            Series = result
        };
    }

    public async Task<List<Asset>> GetAssets() {
        await this.Auth.RefreshLoginIfRequired();
        var character = this.Auth.GetCharacter();
        var request = this.Auth.AuthenticatedRequest($"/characters/{character}/assets/");
        var response = await this.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<List<Asset>>(responseString);
        return result;
    }
    
}