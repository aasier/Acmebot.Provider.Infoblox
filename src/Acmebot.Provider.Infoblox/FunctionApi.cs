using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
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
            var response = req.CreateResponse(HttpStatusCode.OK);
            var zones = await _service.GetZonesAsync();
            await response.WriteAsJsonAsync(zones);
            return response;
        }

        [Function("AddTxtRecord")]
        public async Task<HttpResponseData> AddTxtRecord(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "zones/{zone}/records/{name}")] HttpRequestData req,
            string zone, string name)
        {
            var body = await JsonSerializer.DeserializeAsync<RecordRequest>(req.Body);
            await _service.AddTxtRecordAsync(zone, name, body);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }

        [Function("DeleteTxtRecord")]
        public async Task<HttpResponseData> DeleteTxtRecord(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "zones/{zone}/records/{name}")] HttpRequestData req,
            string zone, string name)
        {
            var body = await JsonSerializer.DeserializeAsync<RecordRequest>(req.Body);
            await _service.DeleteTxtRecordAsync(zone, name, body);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
    }
}