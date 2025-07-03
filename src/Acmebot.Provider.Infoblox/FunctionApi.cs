using Acmebot.Provider.Infoblox.Infoblox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Acmebot.Provider.Infoblox
{
    public class FunctionApi
    {
        private readonly ILogger<FunctionApi> _logger;
        private readonly InfobloxService _infobloxService;

        public FunctionApi(ILogger<FunctionApi> logger, InfobloxService infobloxService)
        {
            _logger = logger;
            _infobloxService = infobloxService;
        }

        public record Payload(string Type, int Ttl, string[] Values);

        [Function(nameof(GetZones))]
        public async Task<IActionResult> GetZones([HttpTrigger(AuthorizationLevel.Admin, "get", Route = "zones")] HttpRequest req)
        {
            _logger.LogInformation("Request to get zones");
            var zones = await _infobloxService.GetZonesAsync();
            return new OkObjectResult(zones);
        }

        [Function(nameof(AddTxt))]
        public async Task<IActionResult> AddTxt(
            [HttpTrigger(AuthorizationLevel.Admin, "put", Route = "zones/{zone}/records/{name}")] HttpRequest req,
            string zone,
            string name)
        {
            var payload = await JsonSerializer.DeserializeAsync<Payload>(req.Body);
            foreach (var value in payload.Values)
                await _infobloxService.AddTxtRecordAsync(zone, name, value);
            return new OkResult();
        }

        [Function(nameof(RemoveTxt))]
        public async Task<IActionResult> RemoveTxt(
            [HttpTrigger(AuthorizationLevel.Admin, "delete", Route = "zones/{zone}/records/{name}")] HttpRequest req,
            string zone,
            string name)
        {
            var payload = await JsonSerializer.DeserializeAsync<Payload>(req.Body);
            foreach (var value in payload.Values)
                await _infobloxService.RemoveTxtRecordAsync(zone, name, value);
            return new OkResult();
        }
    }
}