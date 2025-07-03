using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Acmebot.Provider.Infoblox.Infoblox;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddSingleton<InfobloxService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var baseUrl = config["Infoblox__BaseUrl"];
            var username = config["Infoblox__Username"];
            var password = config["Infoblox__Password"];
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();

            return new InfobloxService(httpClient, baseUrl, username, password);
        });
    })
    .Build();

host.Run();