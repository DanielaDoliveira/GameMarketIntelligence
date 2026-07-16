# Comparable Games Domain Model

## Purpose

Define the initial domain model required to support the Comparable Games feature in GameMarketIntel.

The model should provide enough information to support early production and market-analysis questions without introducing unnecessary complexity before real data sources and usage patterns are validated.

The model is intentionally pragmatic and may evolve as data-source limitations, storage constraints, and product requirements become clearer.

Comparable Games is the first stage of a broader producer decision workflow. Future research references and commercial metrics must be modeled as separate concepts rather than being compressed into unqualified fields on `Game`.

## Current Status

The first domain and persistence increment has been implemented.

Implemented:

- `Game`;
- `Genre`;
- `Platform`;
- `Game` to `Genre` many-to-many relationship;
- `Game` to `Platform` many-to-many relationship;
- normalized-name uniqueness for genres;
- normalized-name uniqueness for platforms;
- EF Core configurations;
- PostgreSQL migration;
- domain unit tests;
- PostgreSQL infrastructure integration tests.

Not yet implemented:

- game-to-source association;
- external provider identifiers;
- publisher modeling;
- collection timestamps;
- application use cases;
- read contracts;
- repositories;
- API endpoints;
- ingestion from a real external data source.

## Product Questions

The current model starts supporting:

- Which games may be considered comparable?
- Which genres are associated with each game?
- Which platforms are associated with each game?
- When was each game released?
- What descriptive information is available for each game?

Future increments should support:

- Which official or specialized sources should the producer inspect next?
- Is an official game website available?
- Which storefront, trailer, gameplay, review, or evidence links are relevant?
- Which publisher is associated with the game?
- Which source provided the information?
- When was the information collected?
- Which limitations affect the interpretation of the data?
- How does classification differ between external sources?

The model should later enable:

- commercial-performance evidence, prioritizing sales;
- complementary download, owner, player, and engagement observations;
- comparison between famous and less-visible comparable games;
- source-aware interpretation of official and estimated values;
- comparable-game filtering;
- genre-saturation analysis;
- launch-window analysis;
- platform comparison;
- historical market indicators.

## Current Domain Scope

The implemented scope includes:

- `Game`;
- `Genre`;
- `Platform`.

The planned scope also includes:

- `DataSource`;
- source-specific external references;
- organized official website, storefront, trailer, gameplay, and review references;
- producer research references such as official websites and storefronts;
- future metric snapshots, prioritizing sales;
- complementary engagement observations;
- future market signals.

`DataSource` and `SourceReliability` already exist elsewhere in the domain, but they are not yet associated with `Game`, `Genre`, or `Platform`.

That association will be designed after the first production data source is selected.

## Game

### Purpose

Represents a game used in market comparison and analysis.

### Implemented Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Display name of the game |
| `Description` | No | Optional descriptive information |
| `ReleaseDate` | No | Initial known release date |
| `ImageUrl` | No | Optional external image reference |
| `Genres` | No | Genres associated with the game |
| `Platforms` | No | Platforms associated with the game |

### Invariants and Normalization

`Name`:

- cannot be `null`;
- cannot be empty;
- cannot contain only whitespace;
- is trimmed before storage.

`Description`:

- is optional;
- empty or whitespace-only values become `null`;
- surrounding whitespace is removed.

`ImageUrl`:

- is optional;
- empty or whitespace-only values become `null`;
- must be an absolute URL;
- must use HTTP or HTTPS.

### Relationships

- A game may have multiple genres.
- A game may be available on multiple platforms.
- Duplicate associations are prevented by entity identity.
- Relationship collections are exposed as read-only collections.
- Removal and full synchronization behavior are deferred until the external-data strategy is defined.

### Release-Date Assumption

`ReleaseDate` currently represents a simplified known release date.

It does not yet represent:

- regional release dates;
- platform-specific release dates;
- Early Access dates;
- ports;
- remasters;
- relaunches.

### Future Evolution

The model may later introduce:

```text
GameRelease
├── Platform
├── Region
├── ReleaseDate
└── ReleaseType
```

This should only be added when platform-specific or regional launch analysis becomes necessary.

The following game properties also remain under evaluation:

- publisher;
- developer;
- canonical slug;
- source association;
- collection timestamp;
- source-specific external identifiers.

## Genre

### Purpose

Represents a game genre used for filtering, comparison, aggregation, and saturation analysis.

### Implemented Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Technical value used for lookup and uniqueness |

### Example

```text
Received value:
"  AcTion RPG  "

Name:
AcTion RPG

NormalizedName:
ACTION RPG
```

`Name` preserves the display value, except for surrounding whitespace.

`NormalizedName` is generated by:

- trimming surrounding whitespace;
- applying Unicode canonical normalization;
- converting the value with invariant uppercase casing.

It is used only for lookup, comparison, and database uniqueness.

### Relationships

- A genre may be associated with multiple games.
- A game may be associated with multiple genres.

### Persistence Rule

A unique PostgreSQL index exists on:

```text
Genre.NormalizedName
```

Therefore, values such as:

```text
Action
action
AcTiOn
```

cannot be persisted as separate genre records.

The application layer will later check for an existing normalized name before attempting insertion.

The database constraint remains the final protection against concurrency and duplicate persistence.

### Domain Considerations

Genre classification may differ between data sources.

The MVP should preserve the selected internal classification while documenting the source and known limitations.

Case and whitespace normalization solves formatting differences, but it does not solve semantic aliases.

For example:

```text
Role-Playing Game
RPG
```

may require a future source-specific mapping strategy.

### Future Evolution

The model may later support:

- genre aliases;
- source-specific genre mappings;
- genre hierarchies;
- primary and secondary genres.

These features should not be introduced before real source differences demonstrate the need.

## Platform

### Purpose

Represents a game platform used for filtering, market comparison, and platform-level analysis.

### Implemented Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Platform display name |
| `NormalizedName` | Yes | Technical value used for lookup and uniqueness |
| `Family` | No | Product family or ecosystem |
| `Manufacturer` | No | Platform manufacturer |
| `ImageUrl` | No | External image reference |

### Example

```text
Name:
PlayStation 5

NormalizedName:
PLAYSTATION 5

Family:
PlayStation

Manufacturer:
Sony

ImageUrl:
https://example.com/playstation-5.png
```

### Invariants and Normalization

`Name`:

- is required;
- is trimmed before storage.

`NormalizedName`:

- uses the same normalization strategy as `Genre.NormalizedName`;
- is used for comparison and uniqueness;
- is protected by a unique PostgreSQL index.

`Family` and `Manufacturer`:

- are optional;
- empty or whitespace-only values become `null`;
- surrounding whitespace is removed.

`ImageUrl`:

- is optional;
- must be an absolute HTTP or HTTPS URL.

### Relationships

- A platform may be associated with multiple games.
- A game may be associated with multiple platforms.

### Canonical Platform Decision

`Platform` represents the internal canonical platform identity used by GameMarketIntel.

External provider names must not automatically define new canonical platforms.

For example:

```text
PlayStation 5
PS5
Sony PlayStation 5
```

may represent the same internal platform even though name normalization alone cannot determine that equivalence.

Case-only differences are handled by `NormalizedName`.

Semantic aliases require future source-specific mappings.

### Storage Decision

Platform images are stored as URLs rather than binary content.

This reduces:

- Neon storage usage;
- API payload size;
- database growth;
- backup size.

The frontend should provide a fallback image when an external URL becomes unavailable.

### Future Evolution

The model may later support:

- source-specific platform references;
- platform aliases;
- platform generation;
- release date;
- lifecycle status;
- platform type;
- official platform identifiers.

These properties should be added only when required by market-analysis use cases or by the selected data sources.

## Research References and Commercial Evidence

Comparable Games discovery should eventually help the producer continue research through relevant original or specialized sources.

Possible research references include:

- official game website;
- official publisher or developer page;
- Steam or another storefront;
- official trailer;
- gameplay video;
- review aggregator;
- source article;
- market report.

These references should not be represented as unrelated URL fields added directly to `Game`.

A future extensible model may use a structure such as:

```text
GameExternalReference
├── GameId
├── ReferenceType
├── Url
├── DisplayName
├── IsOfficial
├── DataSourceId
└── CollectedAt
```

GameMarketIntel should organize paths to deeper research without replacing the referenced services.

Commercial performance must also remain separate from the `Game` entity.

The domain should not introduce fields such as:

```text
Game.TotalSales
```

A sales value may vary by:

- source;
- period;
- platform;
- region;
- official or estimated nature;
- units sold, shipped, owners, players, or downloads;
- collection date;
- reliability.

A future market-metric observation should preserve this context.

Sales are the highest-priority future commercial indicator.

Complementary observations may include:

- revenue;
- estimated owners;
- downloads;
- active players;
- peak concurrent players;
- reviews;
- wishlists;
- followers.

These concepts are intentionally deferred from the current Comparable Games implementation and should be introduced through a dedicated market-metrics vertical after real data sources are evaluated.

## External Source Mapping

External provider identifiers and names must not directly become the canonical identity of internal entities.

The intended future mapping model is:

```text
ExternalPlatformReference
├── Id
├── PlatformId
├── DataSourceId
├── ExternalId
└── ExternalName
```

A similar structure may later be used for games and genres.

The expected resolution flow is:

```text
External provider record
          ↓
Source identifier lookup
          ↓
Existing internal mapping?
├── Yes → reuse the internal entity
└── No  → create or review a new mapping
```

The combination below should eventually be unique:

```text
DataSourceId + ExternalId
```

The exact model remains deferred until the first production data source is selected and its identifiers, aliases, and stability are evaluated.

## Data Source Association

`DataSource` and `SourceReliability` already exist, but their association with Comparable Games entities has not yet been implemented.

The initial proposal considered one primary source per game.

This decision remains under review because external ingestion may require:

- source-specific identifiers;
- multiple sources for one entity;
- different sources for different fields;
- collection history;
- conflict resolution.

The first implementation should avoid field-level provenance until real data demonstrates that the additional complexity is justified.

A future structure may include:

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

## Current Relationship Model

The implemented relationship model is:

```text
Game
├── many-to-many → Genre
└── many-to-many → Platform
```

The future model may add:

```text
Game
└── source-specific association → DataSource
```

The current relationships support filters by:

- genre;
- platform;
- release date;
- game name.

## Persistence Model

The current PostgreSQL model includes:

```text
Games
Genres
Platforms
GameGenres
GamePlatforms
```

The associative tables use composite primary keys:

```text
GameGenres
├── GameId
└── GenreId
```

```text
GamePlatforms
├── GameId
└── PlatformId
```

Unique indexes protect:

```text
Genres.NormalizedName
Platforms.NormalizedName
```

## Persistence Validation

Infrastructure integration tests use a temporary PostgreSQL container managed by Testcontainers.

The implemented tests validate:

- saving a game with a genre and platform;
- loading relationships through a new EF Core context;
- materializing the private domain collections;
- the `GameGenres` relationship;
- the `GamePlatforms` relationship;
- rejection of duplicate genre normalized names;
- rejection of duplicate platform normalized names.

## Initial Comparable Games Response

The first read contract is expected to include:

```text
Game identifier
Game name
Description
Release date
Image URL
Genres
Platforms
```

Source and reliability information will be included after the data-source association is modeled.

Domain entities must not be returned directly by API endpoints.

The Application layer will map entities to Shared response contracts.

## Storage Constraints

The Neon Free storage limit may affect future data-model decisions.

The initial model prioritizes:

- normalized structured data;
- source references;
- processed values;
- essential historical information.

The operational database should avoid storing:

- raw API payloads;
- complete JSON responses;
- HTML pages;
- image binaries;
- large files;
- unnecessarily duplicated source data.

Storage growth may require:

- reduced historical granularity;
- aggregation;
- retention policies;
- selective dataset inclusion;
- postponement of storage-intensive features.

Infrastructure constraints may influence product scope when necessary.

A separate exploratory document records a possible post-MVP recent-and-historical relational data-tiering strategy:

```text
docs/architecture/explorations/hybrid-database-data-tiering-exploration.md
```

That exploration is not an approved decision and does not alter the current Neon PostgreSQL architecture.

## Implementation Progress

Completed:

1. `Genre`;
2. `Platform`;
3. `Game`;
4. many-to-many relationships;
5. normalized-name strategy;
6. EF Core mappings;
7. PostgreSQL migration;
8. domain unit tests;
9. infrastructure integration tests;
10. Pull Request integration validation.

Next:

1. Shared read contracts;
2. Application repository interfaces;
3. Application query use cases;
4. Infrastructure repository implementations;
5. API query endpoints;
6. Application and API tests;
7. first real data source evaluation;
8. source-specific external-reference design;
9. first Comparable Games dataset;
10. Comparable Games filters.

## Deferred Concepts

The following concepts are intentionally deferred:

- source-specific external references;
- source association for Comparable Games entities;
- release dates by platform;
- regional release dates;
- publisher modeling;
- developer modeling;
- genre hierarchy;
- genre aliases;
- field-level data provenance;
- multiple publisher relationships;
- score or opportunity prediction;
- market-signal generation;
- sales and revenue observations;
- download, owner, player, and engagement observations;
- historical metric snapshots;
- machine learning.

Deferring these concepts reduces premature complexity while preserving clear evolution paths.

## Design Principle

The initial domain model should support real product questions without attempting to represent every possible market-data scenario.

The guiding principle is:

> Model the complexity required by validated product decisions, not the complexity that may exist in every future scenario.