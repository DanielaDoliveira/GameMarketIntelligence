using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using GameMarketIntel.Infrastructure;
using GameMarketIntel.Application;
using GameMarketIntel.Api.Endpoints;
using Microsoft.AspNetCore.HttpOverrides;

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

app.MapOpenApi();
    app.MapScalarApiReference();
// Configure the HTTP request pipeline.
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

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapDataSourceEndpoints();



app.Run();
