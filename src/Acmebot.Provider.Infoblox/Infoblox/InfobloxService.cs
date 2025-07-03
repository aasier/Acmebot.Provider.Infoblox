namespace Acmebot.Provider.Infoblox.Infoblox
{
    public class InfobloxService
    {
        private readonly InfobloxClient _client;

        public InfobloxService(HttpClient httpClient, string baseUrl, string username, string password)
        {
            _client = new InfobloxClient(httpClient, baseUrl, username, password);
        }

        public async Task<IEnumerable<string>> GetZonesAsync() => await _client.GetZonesAsync();

        public async Task AddTxtRecordAsync(string zone, string name, string value)
        {
            await _client.AddTxtRecordAsync(zone, name, value);
        }

        public async Task RemoveTxtRecordAsync(string zone, string name, string value)
        {
            await _client.RemoveTxtRecordAsync(zone, name, value);
        }
    }
}