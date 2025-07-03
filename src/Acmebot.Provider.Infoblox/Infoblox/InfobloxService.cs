using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<ZoneResponse>> ListZonesAsync()
        {
            // El cliente Infoblox devuelve solo nombres, adaptamos
            var zones = await _client.GetZonesAsync();
            return zones.Select(z => new ZoneResponse
            {
                Id = z.Replace('.', '_'), // id estilo "example_com"
                Name = z
                // NameServers: podrías pedir más info a Infoblox si lo necesitas
            }).ToList();
        }

        public async Task UpsertTxtRecordsAsync(string zoneId, string recordName, RecordRequest req)
        {
            // Elimina todos los TXT previos de ese nombre en la zona
            await _client.DeleteTxtRecordsAsync(recordName);

            // Crea cada valor TXT
            foreach (var value in req.Values)
            {
                await _client.AddTxtRecordAsync(recordName, value, req.Ttl);
            }
        }

        public Task DeleteTxtRecordsAsync(string zoneId, string recordName)
        {
            // Borra todos los TXT para ese nombre
            return _client.DeleteTxtRecordsAsync(recordName);
        }
    }
}