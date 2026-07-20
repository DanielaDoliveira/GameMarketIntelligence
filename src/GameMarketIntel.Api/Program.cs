using GameMarketIntel.Api.Endpoints;
using GameMarketIntel.Api.ExceptionHandling;
using GameMarketIntel.Application;
using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure;
using GameMarketIntel.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "A connection string 'DefaultConnection' não foi configurada.");

builder.Services.AddDbContext<GameMarketIntelDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});





var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapPost(
        "/development/data-sources/seed",
        async (
            GameMarketIntelDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var reliability = new SourceReliability(
                ReliabilityLevel.PublicDirect,
                "Dados obtidos diretamente de uma fonte pública.",
                "Os dados não representam vendas ou receita.");

            var dataSource = new DataSource(
                "Steam Web API",
                "https://partner.steamgames.com/",
                reliability,
                attributionRequired: false,
                licenseNotes: "Uso sujeito aos termos oficiais da Steam.");

            await dbContext.DataSources.AddAsync(
                dataSource,
                cancellationToken);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return Results.Created(
                $"/api/data-sources/{dataSource.Id}",
                new
                {
                    dataSource.Id,
                    dataSource.Name
                });
        });
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapDataSourceEndpoints();
app.MapGameEndpoints();
app.MapGenreEndpoints();
app.MapPlatformEndpoints();
app.Run();
public partial class Program;