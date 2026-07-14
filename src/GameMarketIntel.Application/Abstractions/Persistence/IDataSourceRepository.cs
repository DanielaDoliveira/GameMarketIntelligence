using GameMarketIntel.Domain.Entities;


namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IDataSourceRepository
{
    Task<IReadOnlyList<DataSource>> GetAllAsync(CancellationToken cancellationToken = default);
}
