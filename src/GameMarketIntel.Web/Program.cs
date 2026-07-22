using GameMarketIntel.Web;
using GameMarketIntel.Web.Configuration;
using GameMarketIntel.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiOptions = builder.Configuration .GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? throw new InvalidOperationException("API configuration was not found.");

if (string.IsNullOrWhiteSpace(apiOptions.BaseUrl))
{
    throw new InvalidOperationException("The API base URL was not configured.");
}
builder.Services.AddScoped<IGenreApiService, GenreApiService>();
builder.Services.AddSingleton(apiOptions);

var apiBaseUrl = apiOptions.BaseUrl.TrimEnd('/') + "/";

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });


await builder.Build().RunAsync();
