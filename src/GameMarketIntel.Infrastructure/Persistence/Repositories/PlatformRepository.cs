using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence.Repositories;

public sealed class PlatformRepository(GameMarketIntelDbContext dbContext) : IPlatformRepository
{
    public async Task<IReadOnlyList<Platform>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Platforms
            .AsNoTracking()
            .OrderBy(platform => platform.Name)
            .ToListAsync(cancellationToken);
    }
}