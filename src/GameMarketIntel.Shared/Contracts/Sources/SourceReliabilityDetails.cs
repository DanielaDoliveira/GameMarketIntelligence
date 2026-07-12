using System;
using System.Collections.Generic;
using System.Text;

namespace GameMarketIntel.Shared.Contracts.Sources;

public sealed record SourceReliabilityDetails(string Level, string Reason, string? Limitations);