# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY GameMarketIntel.slnx ./
COPY src/GameMarketIntel.Shared/GameMarketIntel.Shared.csproj src/GameMarketIntel.Shared/
COPY src/GameMarketIntel.Api/GameMarketIntel.Api.csproj src/GameMarketIntel.Api/
COPY src/GameMarketIntel.Application/GameMarketIntel.Application.csproj src/GameMarketIntel.Application/
COPY src/GameMarketIntel.Domain/GameMarketIntel.Domain.csproj src/GameMarketIntel.Domain/
COPY src/GameMarketIntel.Infrastructure/GameMarketIntel.Infrastructure.csproj src/GameMarketIntel.Infrastructure/

RUN dotnet restore src/GameMarketIntel.Api/GameMarketIntel.Api.csproj

COPY src/ src/

RUN dotnet publish \
    src/GameMarketIntel.Api/GameMarketIntel.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "GameMarketIntel.Api.dll"]