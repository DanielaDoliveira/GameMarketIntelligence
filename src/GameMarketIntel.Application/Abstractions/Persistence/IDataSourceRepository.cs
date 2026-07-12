using GameMarketIntel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IDataSourceRepository
{
    Task<IReadOnlyList<DataSource>> GetAllAsync(CancellationToken cancellationToken = default);
}
