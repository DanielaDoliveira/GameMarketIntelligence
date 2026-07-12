using GameMarketIntel.Shared.Contracts.Sources;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IDataSourceService
{
    Task<IReadOnlyList<DataSourceDetails>> GetAllAsync(CancellationToken cancellationToken = default);
}
