# Comparable Games Domain Model

## Purpose

Define the initial domain model and read behavior required to support the Comparable Games feature in GameMarketIntel.

The model should provide enough information to support early production and market-analysis questions without introducing unnecessary complexity before real data sources and usage patterns are validated.

The initial model is intentionally pragmatic and may evolve as data-source limitations, storage constraints, product requirements, and frontend behavior become clearer.

## Product Questions

The first domain vertical should help answer:

* Which games may be considered comparable?
* Which genres are associated with each game?
* Which platforms are associated with each game?
* When was each game initially released?
* Which games match a partial-name search?
* Which games match a selected genre?
* Which games match a selected platform?
* Which games were released in a selected year?
* Which publisher is associated with a game when that information becomes available?
* Which source provided the information?
* When was the information collected?
* Which limitations affect interpretation of the data?

The model should later support:

* comparable-game filtering;
* genre-saturation analysis;
* launch-window analysis;
* platform comparison;
* historical market indicators;
* commercial performance comparison.

## Domain Scope

The current domain scope includes:

* `Game`;
* `Genre`;
* `Platform`;
* `DataSource`;
* game-to-genre relationships;
* game-to-platform relationships;
* read contracts for Comparable Games search;
* future data-source association;
* future external identifiers;
* future metric snapshots.

`DataSource` and `SourceReliability` already exist and provide a foundation for traceability, licensing information, attribution requirements, and reliability context.

## Current Status

Implemented:

* `Game`;
* `Genre`;
* `Platform`;
* game-to-genre many-to-many relationship;
* game-to-platform many-to-many relationship;
* normalized-name uniqueness;
* domain invariants;
* EF Core configurations;
* PostgreSQL migration;
* PostgreSQL persistence;
* domain tests;
* PostgreSQL integration tests;
* Comparable Games search contracts;
* Application search service;
* repository abstraction;
* PostgreSQL search repository;
* partial case-insensitive game-name filtering;
* genre filtering;
* platform filtering;
* release-year filtering;
* AND combination between different filter categories;
* alphabetical ordering;
* pagination;
* pagination metadata;
* `GET /api/games`;
* FluentValidation for search parameters;
* OpenAPI and Scalar documentation;
* game-details query and projection;
* `GET /api/games/{id:guid}`;
* missing-game handling through `NotFoundException`;
* centralized API exception handling;
* standardized `ProblemDetails` responses;
* validation failures mapped to HTTP `400`;
* missing resources mapped to HTTP `404`;
* conflicts mapped to HTTP `409`;
* unexpected failures mapped to HTTP `500`;
* 97 automated tests passing across the solution.

Not yet implemented:

* game-to-source association;
* external provider identifiers;
* publisher modeling;
* collection timestamps on game records;
* genre listing query;
* platform listing query;
* ingestion from a real external source;
* commercial metric observations.

## Game

### Purpose

Represents a game used in market comparison and analysis.

### Implemented Properties

| Property      | Required | Purpose                    |
| ------------- | -------: | -------------------------- |
| `Id`          |      Yes | Internal identity          |
| `Name`        |      Yes | Public game name           |
| `Description` |       No | Short descriptive context  |
| `ReleaseDate` |       No | Initial known release date |
| `ImageUrl`    |       No | External image reference   |

### Implemented Relationships

* A game may have multiple genres.
* A game may be available on multiple platforms.

### Current Release-Date Assumption

`ReleaseDate` represents a simplified initial release date.

It does not currently attempt to represent:

* regional release dates;
* platform-specific release dates;
* Early Access dates;
* ports;
* remasters;
* relaunches.

The model may later introduce:

```text
GameRelease
├── Platform
├── Region
├── ReleaseDate
└── ReleaseType
```

This should only be added when platform-specific or regional launch analysis becomes necessary.

### Future Game Properties

Potential future properties include:

| Property              | Purpose                                           |
| --------------------- | ------------------------------------------------- |
| `Slug`                | Stable normalized identifier for URLs and lookup  |
| `Publisher`           | Main publisher associated with the game           |
| `PrimaryDataSourceId` | Main source used to create or maintain the record |
| `CollectedAt`         | Time when the record was collected or refreshed   |

These properties should be introduced when required by a validated source or use case.

### Release-Date Validation

Searches currently accept an optional `ReleaseYear`.

The search year may be equal to the current year but may not be greater than it.

Future game creation or update behavior must validate the full release date.

A game release date may be equal to the current date but must not be later than the current date.

Example:

```text
Current date:
2026-06-17

Valid:
2026-06-17

Invalid:
2026-06-18
```

This full-date rule belongs to creation or update behavior, not to `SearchGamesQuery`, because the search currently receives only an integer year.

## Genre

### Purpose

Represents a game genre used for filtering, comparison, aggregation, and saturation analysis.

### Implemented Properties

| Property         | Required | Purpose                                            |
| ---------------- | -------: | -------------------------------------------------- |
| `Id`             |      Yes | Internal identity                                  |
| `Name`           |      Yes | Display name                                       |
| `NormalizedName` |      Yes | Canonical value used for uniqueness and comparison |

### Example

```text
Name:
Action RPG

NormalizedName:
action rpg
```

The exact normalization representation follows the implemented `NameNormalizer` behavior.

### Relationships

* A genre may be associated with multiple games.
* A game may be associated with multiple genres.

### Domain Considerations

Genre classification may differ between data sources.

The MVP should preserve the selected internal classification while documenting source limitations.

Genre normalization prevents duplicate concepts caused only by casing, spacing, or formatting differences.

### Future Evolution

The model may later support:

* genre aliases;
* source-specific genre mappings;
* genre hierarchies;
* primary and secondary genres.

These features should not be introduced before real source differences demonstrate their value.

## Platform

### Purpose

Represents a game platform used for filtering, market comparison, and platform-level analysis.

### Implemented Properties

| Property       | Required | Purpose                     |
| -------------- | -------: | --------------------------- |
| `Id`           |      Yes | Internal identity           |
| `Name`         |      Yes | Platform display name       |
| `Family`       |       No | Product family or ecosystem |
| `Manufacturer` |       No | Platform manufacturer       |
| `ImageUrl`     |       No | External image reference    |

### Example

```text
Name:
PlayStation 5

Family:
PlayStation

Manufacturer:
Sony

ImageUrl:
https://example.com/playstation-5.png
```

### Relationships

* A platform may be associated with multiple games.
* A game may be associated with multiple platforms.

### Storage Decision

Platform and game images should be stored as URLs rather than binary content.

This reduces:

* Neon storage usage;
* API payload size;
* database growth;
* backup size.

The frontend should provide a fallback image when an external URL is missing or unavailable.

### Future Evolution

The model may later support:

* platform generation;
* release date;
* lifecycle status;
* platform type;
* official platform identifiers.

These properties should be added only when required by market-analysis use cases.

## Data Source Association

A game-to-source association is planned but is not yet part of the implemented `Game` entity.

The intended MVP direction is to associate one primary data source with each game record.

This would simplify provenance while preserving traceability.

The primary source would represent the main origin of a record, but it would not guarantee that every field came from exactly the same source.

That limitation must remain documented.

### Future Evolution

When multiple sources are combined, the model may introduce field-level or observation-level provenance.

Possible future structure:

```text
DataObservation
├── EntityType
├── EntityId
├── FieldName
├── Value
├── DataSourceId
├── CollectedAt
└── Reliability
```

Field-level provenance should not be implemented until its analytical value justifies the additional storage and complexity.

## External Identifiers

Games may have identifiers from different providers.

Examples:

* Steam App ID;
* IGDB ID;
* RAWG ID;
* official publisher identifier.

The domain should avoid permanent provider-specific properties such as:

```text
SteamId
IgdbId
RawgId
```

A future extensible model may use:

```text
GameExternalIdentifier
├── Provider
└── Value
```

External identifiers should be implemented when the first production data source is selected.

## Implemented Relationship Model

The current implemented relationship model is:

```text
Game
  ├── many-to-many → Genre
  └── many-to-many → Platform
```

The planned future relationship is:

```text
Game
  └── many-to-one → DataSource
```

The implemented relationships support the current filters:

* partial name;
* genre;
* platform;
* release year.

## Comparable Games Search

### Search Contract

The current query contract is:

```text
Search
GenreId
PlatformId
ReleaseYear
Page
PageSize
```

The current endpoint accepts:

* one optional text search;
* one optional genre identifier;
* one optional platform identifier;
* one optional release year;
* pagination parameters.

### Search Semantics

Different filter categories use AND semantics.

```text
Search condition
AND
Genre condition
AND
Platform condition
AND
Release-year condition
```

Example:

```text
Name contains "had"
AND
Genre is Action
AND
Platform is PC
AND
Release year is 2020
```

The current implementation does not accept multiple genre or platform values in the same request.

Support for multiple values is deferred until the initial frontend flow is validated.

### Partial-Name Search

Game-name search:

* removes external whitespace from the supplied term;
* uses PostgreSQL `ILike`;
* ignores letter casing;
* matches the term within any part of the game name.

Example:

```text
Search:
zELdA

Possible matches:
The Legend of Zelda
Zelda II: The Adventure of Link
```

### Pagination

The search result exposes:

```text
Items
Page
PageSize
TotalItems
TotalPages
```

Rules:

* `Page` must be greater than or equal to `1`;
* `PageSize` must be between `1` and `100`;
* default `Page` is `1`;
* default `PageSize` is `20`.

The repository:

1. applies filters;
2. counts all matching records;
3. orders games alphabetically;
4. skips records from previous pages;
5. takes only the requested page size;
6. projects the result into read contracts.

### Release-Year Search

`ReleaseYear` is optional.

When supplied:

* games without a release date are excluded;
* the stored date year must match the supplied year;
* the supplied year cannot be greater than the current year.

Searching the current year is valid.

The search-year validation does not determine whether a specific day later in the current year is valid because the query contains only the year.

### Query Validation

The implemented validator checks:

```text
Page >= 1
PageSize between 1 and 100
ReleaseYear <= current year
```

Validation occurs in the Application service before the repository is called.

The centralized API exception handler converts validation failures into standardized HTTP `400` responses using `ValidationProblemDetails`.

## Implemented Comparable Games Response

The implemented search response contains:

```text
Game identifier
Game name
Description
Release date
Image URL
Genres
Platforms
```

Genres and platforms are returned as lightweight categories containing:

```text
Id
Name
```

The response returns read DTOs rather than domain entities.

The API response also contains pagination metadata:

```text
Current page
Page size
Total matching games
Total pages
```

## Storage Constraints

The Neon Free storage limit may affect future data-model decisions.

The initial model should prioritize:

* normalized structured data;
* source references;
* processed values;
* essential historical information.

The operational database should avoid storing:

* raw API payloads;
* complete JSON responses;
* HTML pages;
* image binaries;
* large files;
* duplicated source data.

Storage growth may require:

* reduced historical granularity;
* aggregation;
* retention policies;
* selective dataset inclusion;
* postponement of storage-intensive features.

Infrastructure constraints may influence product scope when necessary.

## Validation Responsibilities

Validation is divided by responsibility.

### Domain Validation

Domain entities protect invariants that must remain true regardless of the caller.

Examples:

* required names;
* name normalization;
* duplicate relationship prevention;
* URL rules where implemented;
* valid internal state.

### Application Validation

Application validators protect use-case inputs.

Current example:

```text
SearchGamesQueryValidator
```

Rules include:

* pagination limits;
* current-year search limit.

### API Validation Response

The API translates FluentValidation failures into standardized HTTP responses.

The implemented flow is:

```text
FluentValidation.ValidationException
    ↓
GlobalExceptionHandler
    ↓
ValidationProblemDetails
    ↓
HTTP 400 Bad Request
```

Other application and unexpected failures use the same centralized handler:

```text
NotFoundException
    ↓
HTTP 404 Not Found

ConflictException
    ↓
HTTP 409 Conflict

Unexpected Exception
    ↓
HTTP 500 Internal Server Error
```

The Exceptions project remains independent of HTTP concerns.

## Implementation Progress

Completed sequence:

1. `DataSource` and reliability foundation;
2. `Genre`;
3. `Platform`;
4. `Game`;
5. game-to-genre relationship;
6. game-to-platform relationship;
7. normalization and unique indexes;
8. EF Core configurations;
9. PostgreSQL migration;
10. PostgreSQL integration-test infrastructure;
11. persistence and relationship tests;
12. Comparable Games search contracts;
13. Application search service;
14. search repository abstraction;
15. PostgreSQL search repository;
16. partial-name filter;
17. genre filter;
18. platform filter;
19. release-year filter;
20. AND combination between categories;
21. alphabetical ordering;
22. pagination;
23. search-result projection;
24. `GET /api/games`;
25. OpenAPI and Scalar documentation;
26. FluentValidation;
27. validator tests;
28. repository integration tests;
29. `GameMarketIntel.Exceptions` class library;
30. reusable `NotFoundException` and `ConflictException`;
31. centralized ASP.NET Core `IExceptionHandler`;
32. standardized `ProblemDetails` responses;
33. validation-to-HTTP `400` mapping;
34. missing-resource-to-HTTP `404` mapping;
35. conflict-to-HTTP `409` mapping;
36. unexpected-error-to-HTTP `500` mapping;
37. game-details Application service;
38. game-details contract projection;
39. `GET /api/games/{id:guid}`;
40. Scalar documentation for game details;
41. exception-handler tests;
42. game-service tests;
43. game-details endpoint test;
44. 97 solution tests passing.

## Next Implementation Steps

1. implement genre listing;
2. implement platform listing;
3. document both read endpoints in OpenAPI and Scalar;
4. add Application and API tests for both endpoints;
5. begin the responsive frontend;
6. connect Blazor WebAssembly to the API;
7. validate loading, error, empty, and not-found states;
8. evaluate the first real data source.

## Deferred Concepts

The following concepts remain intentionally deferred:

* provider-specific game identifiers;
* publisher modeling;
* developer modeling;
* game-to-source association;
* collection timestamps;
* regional releases;
* platform-specific releases;
* multiple genres in one search request;
* multiple platforms in one search request;
* source-level taxonomy mappings;
* field-level provenance;
* metric snapshots;
* sales and revenue observations;
* advanced similarity scoring;
* recommendation systems;
* machine learning.

Deferral is intentional and should be reviewed only when validated data or product needs justify the additional complexity.