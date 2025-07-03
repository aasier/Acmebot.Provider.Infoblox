using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Acmebot.Provider.Infoblox.Infoblox;
using Acmebot.Provider.Infoblox.Models;

namespace Acmebot.Provider.Infoblox
{
    public class FunctionApi
    {
        private readonly InfobloxService _service;
        private readonly ILogger _logger;

        public FunctionApi(InfobloxService service, ILoggerFactory loggerFactory)
        {
            _service = service;
            _logger = loggerFactory.CreateLogger<FunctionApi>();
        }

        [Function("GetZones")]
        public async Task<HttpResponseData> GetZones(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zones")] HttpRequestData req)
        {
            var zones = await _service.ListZonesAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(zones);
            return response;
        }

        [Function("UpsertRecord")]
        public async Task<HttpResponseData> UpsertRecord(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "zones/{zoneId}/records/{recordName}")] HttpRequestData req,
            string zoneId, string recordName)
        {
            var body = await JsonSerializer.DeserializeAsync<RecordRequest>(req.Body);
            await _service.UpsertTxtRecordsAsync(zoneId, recordName, body);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }

        [Function("DeleteRecord")]
        public async Task<HttpResponseData> DeleteRecord(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "zones/{zoneId}/records/{recordName}")] HttpRequestData req,
            string zoneId, string recordName)
        {
            await _service.DeleteTxtRecordsAsync(zoneId, recordName);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
    }
}