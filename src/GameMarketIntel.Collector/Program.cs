using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Poc;
using GameMarketIntel.Collector.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<IgdbPocOptions>(builder.Configuration.GetSection(IgdbPocOptions.SectionName));

builder.Services.AddHttpClient<IIgdbAuthenticationService, IgdbAuthenticationService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();