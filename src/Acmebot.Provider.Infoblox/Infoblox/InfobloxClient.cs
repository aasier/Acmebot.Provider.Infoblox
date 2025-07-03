using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Acmebot.Provider.Infoblox.Infoblox
{
    public class InfobloxClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public InfobloxClient(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Infoblox__BaseUrl"] ?? throw new ArgumentNullException("Infoblox__BaseUrl");
            var username = configuration["Infoblox__Username"];
            var password = configuration["Infoblox__Password"];
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<IEnumerable<string>> GetZonesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/zone_auth");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var result = new List<string>();
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("fqdn", out var fqdn))
                    result.Add(fqdn.GetString());
            }
            return result;
        }

        public async Task AddTxtRecordAsync(string zone, string name, string value, int ttl)
        {
            var record = new
            {
                name = name,
                ipv4addr = value,
                ttl = ttl,
                type = "TXT",
                text = value
            };
            var content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/record:txt", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteTxtRecordAsync(string zone, string name, string value)
        {
            // Lookup the record Ref
            var searchUrl = $"{_baseUrl}/record:txt?zone={zone}&name={name}";
            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("_ref", out var recordRef))
                {
                    var refValue = recordRef.GetString();
                    var delResponse = await _httpClient.DeleteAsync($"{_baseUrl}/{refValue}");
                    delResponse.EnsureSuccessStatusCode();
                }
            }
        }
    }
}