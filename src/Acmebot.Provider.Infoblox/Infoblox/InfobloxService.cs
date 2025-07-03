using System.Collections.Generic;
using System.Threading.Tasks;
using Acmebot.Provider.Infoblox.Models;

namespace Acmebot.Provider.Infoblox.Infoblox
{
    public class InfobloxService
    {
        private readonly InfobloxClient _client;

        public InfobloxService(InfobloxClient client)
        {
            _client = client;
        }

        public Task<IEnumerable<string>> GetZonesAsync()
        {
            return _client.GetZonesAsync();
        }

        public Task AddTxtRecordAsync(string zone, string name, RecordRequest req)
        {
            // Assume only a single value for TXT
            return _client.AddTxtRecordAsync(zone, name, req.Values[0], req.Ttl);
        }

        public Task DeleteTxtRecordAsync(string zone, string name, RecordRequest req)
        {
            // Assume only a single value for TXT
            return _client.DeleteTxtRecordAsync(zone, name, req.Values[0]);
        }
    }
}