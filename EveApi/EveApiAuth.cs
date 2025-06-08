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
        var missingAccessToken = this.AccessToken == null;
        var expired = this.RefreshAt == null || this.RefreshAt.Value < DateTime.Now;
        if (loginRequired) {
            await LogIn();
        } else if (missingAccessToken || expired) {
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

        var collectTokenTask = CollectLoginToken();

        var state = Guid.NewGuid().ToString(); // TODO: Not Crypto Secure
        var baseUrl = this.Config["RefreshTokenUrl"];
        var callbackUrl = this.Config["CallbackUrl"];
        var clientId = this.Config["ClientId"];

        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString["response_type"] = "code";
        queryString["redirect_uri"] = callbackUrl;
        queryString["state"] = state;
        queryString["client_id"] = clientId;

        var urlBuilder = new UriBuilder(baseUrl);
        urlBuilder.Query = queryString.ToString();
        var url = urlBuilder.ToString();
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

        this.LoginToken = await collectTokenTask;

    }

    private async Task<string> CollectLoginToken() {
        var callbackUrl = this.Config["CallbackUrl"];
        using var listener = new HttpListener();
        listener.Prefixes.Add(callbackUrl + "/");
        listener.Start();
        var context = await listener.GetContextAsync();
        var queryString = HttpUtility.ParseQueryString(context.Request.Url.Query);
        var token = queryString["code"];
        using var response = context.Response;
        var responseString = "<html><body><h1>Authorization Complete</h1></body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        // TODO: Validate State
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

}