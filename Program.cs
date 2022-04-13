using GoogleDomains.Ddns.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Google Domains DDNS Client";
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient();
        services.AddOptions();
        services.Configure<DdnsOptions>(ctx.Configuration.GetRequiredSection("GoogleDdnsOptions"));
    })
    .Build();

await host.RunAsync();
