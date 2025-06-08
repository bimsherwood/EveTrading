using Newtonsoft.Json;

public class EveTokenStore {

    public string FilePath { get; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshAt { get; set; }

    public EveTokenStore(string filePath) {
        FilePath = filePath;
    }

    public async Task Save() {
        var data = JsonConvert.SerializeObject(this);
        File.WriteAllText(this.FilePath, data);
    }

    public async Task Load() {
        if (File.Exists(this.FilePath)) {
            var data = File.ReadAllText(this.FilePath);
            var store = JsonConvert.DeserializeObject<EveTokenStore>(data);
            this.AccessToken = store.AccessToken;
            this.RefreshToken = store.RefreshToken;
            this.RefreshAt = store.RefreshAt;
        } else {
            this.AccessToken = null;
            this.RefreshToken = null;
            this.RefreshAt = null;
        }
    }

}