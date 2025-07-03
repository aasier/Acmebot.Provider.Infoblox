
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Acmebot.Provider.Infoblox.Infoblox
{
    public class InfobloxClient
    {
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly HttpClient _httpClient;

        public InfobloxClient(HttpClient httpClient, string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _username = username;
            _password = password;
            _httpClient = httpClient;
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<string[]> GetZonesAsync()
        {
            var resp = await _httpClient.GetAsync($"{_baseUrl}/zone_auth");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray().Select(x => x.GetProperty("fqdn").GetString()!).ToArray();
        }

        public async Task<string?> AddTxtRecordAsync(string zone, string name, string value, int ttl = 300)
        {
            var payload = new
            {
                name = name,
                text = value,
                ttl = ttl,
                view = "default",
                zone = zone
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync($"{_baseUrl}/record:txt", content);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task<bool> RemoveTxtRecordAsync(string zone, string name, string value)
        {
            var searchResp = await _httpClient.GetAsync($"{_baseUrl}/record:txt?name={name}&text={value}&zone={zone}");
            if (!searchResp.IsSuccessStatusCode) return false;
            var json = await searchResp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                var refId = elem.GetProperty("_ref").GetString();
                if (!string.IsNullOrEmpty(refId))
                {
                    var delResp = await _httpClient.DeleteAsync($"{_baseUrl}/{refId}");
                    if (delResp.IsSuccessStatusCode) return true;
                }
            }
            return false;
        }
    }
}