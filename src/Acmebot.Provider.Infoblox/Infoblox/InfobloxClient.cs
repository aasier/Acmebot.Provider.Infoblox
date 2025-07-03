using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        /// <summary>
        /// List all authoritative DNS zones.
        /// </summary>
        public async Task<List<string>> GetZonesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/zone_auth");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = new List<string>();
            using var doc = JsonDocument.Parse(json);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("fqdn", out var fqdn))
                    result.Add(fqdn.GetString());
            }
            return result;
        }

        /// <summary>
        /// Add a TXT record. Each value = one record (multiple values = multiple calls).
        /// </summary>
        public async Task AddTxtRecordAsync(string name, string value, int? ttl = null)
        {
            var record = new Dictionary<string, object>
            {
                ["name"] = name,
                ["text"] = value
            };
            if (ttl.HasValue)
                record["ttl"] = ttl.Value;

            var content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
            var url = $"{_baseUrl}/record:txt?_return_fields+=name,text&_return_as_object=1";
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Delete all TXT records for a given FQDN (regardless of value).
        /// </summary>
        public async Task DeleteTxtRecordsAsync(string name)
        {
            // 1. Find all TXT records for that name
            var searchUrl = $"{_baseUrl}/record:txt?name={Uri.EscapeDataString(name)}";
            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("_ref", out var recordRef))
                {
                    var refValue = recordRef.GetString();
                    // 2. Delete each record by _ref
                    var delUrl = $"{_baseUrl}/{refValue}?_return_as_object=1";
                    var delResponse = await _httpClient.DeleteAsync(delUrl);
                    delResponse.EnsureSuccessStatusCode();
                }
            }
        }
    }
}