using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace GameMarketIntel.Infrastructure.Persistence.Repositories;

public sealed class DataSourceRepository(GameMarketIntelDbContext dbContext) : IDataSourceRepository
{
    public async Task<IReadOnlyList<DataSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.
            DataSources
            .AsNoTracking()
            .OrderBy(source => source.Name)
            .ToListAsync(cancellationToken);
    }
}
