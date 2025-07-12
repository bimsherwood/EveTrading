using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace EveTrading.EveApi;

public class EveApiAuth {

    private readonly HttpClient HttpClient;
    private readonly IConfiguration Config;
    private readonly EveTokenStore TokenStore;
    private string? LoginToken;
    private string? RefreshToken;
    private string? AccessToken;
    private DateTime? RefreshAt;

    public EveApiAuth(HttpClient httpClient, IConfiguration config, EveTokenStore tokenStore) {
        this.HttpClient = httpClient;
        this.Config = config;
        this.TokenStore = tokenStore;
    }

    public async Task RefreshLoginIfRequired() {
        await Load();
        var loginRequired = this.RefreshToken == null;
        if (loginRequired) {
            await LogIn();
        }
        var missingAccessToken = this.AccessToken == null;
        var expired = this.RefreshAt == null || this.RefreshAt.Value < DateTime.Now;
        if (missingAccessToken || expired) {
            await RefreshAccessToken();
            await Save();
        }
    }

    public async Task LogIn() {
        await LoadLoginToken();
        await LoadRefreshAccessTokens();
        await Save();
    }

    public HttpRequestMessage AuthenticatedRequest(string path) {
        var apiUrl = this.Config["ApiUrl"];
        var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);
        return request;
    }

    public int? GetCharacter() {
        if (this.AccessToken == null) {
            return null;
        }
        var claims = ParseJwtClaims(this.AccessToken);
        var subject = claims["sub"];
        var character = subject.Split(":")[2];
        return int.Parse(character);
    }

    private async Task RefreshAccessToken() {

        var tokenUrl = this.Config["AccessTokenUrl"];
        var clientId = this.Config["ClientId"];
        var clientSecret = this.Config["ClientSecret"];

        var requestBody = new Dictionary<string, string>();
        requestBody["grant_type"] = "refresh_token";
        requestBody["refresh_token"] = this.RefreshToken;
        var formEncodedBody = new FormUrlEncodedContent(requestBody);

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = formEncodedBody;

        var response = await this.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var responseContent = JsonConvert.DeserializeObject<EveTokenResponse>(responseString);

        this.AccessToken = responseContent.AccessToken;
        this.RefreshToken = responseContent.RefreshToken;
        this.RefreshAt = DateTime.Now.AddSeconds(responseContent.ExpiresIn);

    }

    private async Task Save() {
        this.TokenStore.AccessToken = this.AccessToken;
        this.TokenStore.RefreshToken = this.RefreshToken;
        this.TokenStore.RefreshAt = this.RefreshAt;
        await this.TokenStore.Save();
    }

    private async Task Load() {
        await this.TokenStore.Load();
        this.AccessToken = this.TokenStore.AccessToken;
        this.RefreshToken = this.TokenStore.RefreshToken;
        this.RefreshAt = this.TokenStore.RefreshAt;
    }

    private async Task LoadLoginToken() {

        var state = Guid.NewGuid().ToString();
        var collectTokenTask = CollectLoginToken(state);

        var baseUrl = this.Config["RefreshTokenUrl"];
        var callbackUrl = this.Config["CallbackUrl"];
        var clientId = this.Config["ClientId"];
        var scopes = this.Config["Scopes"];

        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString["response_type"] = "code";
        queryString["redirect_uri"] = callbackUrl;
        queryString["state"] = state;
        queryString["client_id"] = clientId;
        queryString["scope"] = scopes;

        var urlBuilder = new UriBuilder(baseUrl);
        urlBuilder.Query = queryString.ToString();
        var url = urlBuilder.ToString();
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

        this.LoginToken = await collectTokenTask;

    }

    private async Task<string> CollectLoginToken(string stateNonce) {
        var callbackUrl = this.Config["CallbackUrl"];
        using var listener = new HttpListener();
        listener.Prefixes.Add(callbackUrl + "/");
        listener.Start();
        var context = await listener.GetContextAsync();
        ; // TODO: Validate State Nonce
        var queryString = HttpUtility.ParseQueryString(context.Request.Url.Query);
        var token = queryString["code"];
        using var response = context.Response;
        var responseString = "<html><body><h1>Authorization Complete</h1></body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        return token;
    }

    private async Task LoadRefreshAccessTokens() {

        var tokenUrl = this.Config["AccessTokenUrl"];
        var clientId = this.Config["ClientId"];
        var clientSecret = this.Config["ClientSecret"];

        var requestBody = new Dictionary<string, string>();
        requestBody["grant_type"] = "authorization_code";
        requestBody["code"] = this.LoginToken;
        var formEncodedBody = new FormUrlEncodedContent(requestBody);

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = formEncodedBody;

        var response = await this.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var responseContent = JsonConvert.DeserializeObject<EveTokenResponse>(responseString);

        this.AccessToken = responseContent.AccessToken;
        this.RefreshToken = responseContent.RefreshToken;
        this.RefreshAt = DateTime.Now.AddSeconds(responseContent.ExpiresIn);

    }

    private Dictionary<string, dynamic> ParseJwtClaims(string jwt) {
        var parts = jwt.Split('.');
        var payload = parts[1];
        // Fix Base64 padding
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4) {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(payload);
        var str = Encoding.UTF8.GetString(bytes);
        var claims = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(str);
        return claims;
    }

}