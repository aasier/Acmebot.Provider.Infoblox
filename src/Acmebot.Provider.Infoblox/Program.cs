using Acmebot.Provider.Infoblox.Infoblox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddSingleton<InfobloxClient>();
        services.AddSingleton<InfobloxService>();
    })
    .Build();

host.Run();