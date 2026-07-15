# Comparable Games Domain Model

## Purpose

Define the initial domain model required to support the Comparable Games feature in GameMarketIntel.

The model should provide enough information to support early production and market-analysis questions without introducing unnecessary complexity before real data sources and usage patterns are validated.

The initial model is intentionally pragmatic and may evolve as data-source limitations, storage constraints, and product requirements become clearer.

## Product Questions

The first domain vertical should help answer:

- Which games may be considered comparable?
- Which genres are associated with each game?
- Which platforms are associated with each game?
- When was each game initially released?
- Which publisher is associated with the game?
- Which source provided the information?
- When was the information collected?
- Which limitations affect the interpretation of the data?

The model should later support:

- comparable-game filtering;
- genre-saturation analysis;
- launch-window analysis;
- platform comparison;
- historical market indicators.

## Domain Scope

The initial domain scope includes:

- `Game`;
- `Genre`;
- `Platform`;
- `DataSource`;
- future external identifiers;
- future metric snapshots.

`DataSource` and `SourceReliability` already exist and provide traceability, licensing information, attribution requirements, and reliability context.

## Game

### Purpose

Represents a game used in market comparison and analysis.

### Initial Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Title` | Yes | Public game title |
| `Slug` | Yes | Normalized identifier used for URLs, lookup, and comparison |
| `PrimaryReleaseDate` | No | Initial known release date used by the MVP |
| `Publisher` | No | Main publisher associated with the game |
| `PrimaryDataSourceId` | Yes | Main source used to create or maintain the record |
| `CollectedAt` | Yes | Date and time when the record was collected or last refreshed |

### Relationships

- A game may have multiple genres.
- A game may be available on multiple platforms.
- A game has one primary data source in the MVP.

### MVP Assumptions

`PrimaryReleaseDate` represents a simplified initial release date.

It does not attempt to represent:

- regional release dates;
- platform-specific release dates;
- Early Access dates;
- ports;
- remasters;
- relaunches.

The property name should make the simplification explicit.

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

## Genre

### Purpose

Represents a game genre used for filtering, comparison, aggregation, and saturation analysis.

### Initial Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Normalized value used for uniqueness, search, and comparison |

### Example

```text
Name:
Action RPG

NormalizedName:
action-rpg
```

### Relationships

- A genre may be associated with multiple games.
- A game may be associated with multiple genres.

### Domain Considerations

Genre classification may differ between data sources.

The MVP should preserve the selected internal classification while documenting the source and known limitations.

Genre normalization should avoid creating duplicate concepts caused only by formatting or casing differences.

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

### Initial Properties

| Property | Required | Purpose |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Platform display name |
| `Family` | No | Product family or ecosystem |
| `Manufacturer` | No | Platform manufacturer |
| `ImageUrl` | No | External image reference |

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

- A platform may be associated with multiple games.
- A game may be associated with multiple platforms.

### Storage Decision

Platform images should be stored as URLs rather than binary content.

This reduces:

- Neon storage usage;
- API payload size;
- database growth;
- backup size.

The frontend should provide a fallback image when an external URL becomes unavailable.

### Future Evolution

The model may later support:

- platform generation;
- release date;
- lifecycle status;
- platform type;
- official platform identifiers.

These properties should be added only when required by market-analysis use cases.

## Data Source Association

The MVP uses one primary data source for each game record.

This simplifies provenance while preserving traceability.

The primary source represents the main origin of the record but does not guarantee that every individual field was obtained from the same source.

This limitation must remain documented.

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

Field-level provenance should not be implemented until the value justifies the additional storage and complexity.

## External Identifiers

Games may have identifiers from different providers.

Examples:

- Steam App ID;
- IGDB ID;
- RAWG ID;
- official publisher identifier.

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

## Relationships

The initial relationship model is:

```text
Game
  ├── many-to-many → Genre
  ├── many-to-many → Platform
  └── many-to-one  → DataSource
```

These relationships support the first product filters:

- genre;
- platform;
- release period;
- title.

## Initial Comparable Games Response

The first comparable-games response may include:

```text
Game title
Primary release date
Publisher
Genres
Platforms
Primary source
Source reliability
Collection date
```

The response should expose enough context for interpretation without returning domain entities directly.

## Storage Constraints

The Neon Free storage limit may affect future data-model decisions.

The initial model should prioritize:

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
- duplicated source data.

Storage growth may require:

- reduced historical granularity;
- aggregation;
- retention policies;
- selective dataset inclusion;
- postponement of storage-intensive features.

Infrastructure constraints may therefore influence product scope when necessary.

## Initial Implementation Order

The recommended implementation sequence is:

1. `Platform`;
2. `Genre`;
3. `Game`;
4. many-to-many relationships;
5. primary data-source association;
6. contracts and mapping;
7. repositories;
8. application services;
9. API endpoints;
10. tests;
11. first real dataset;
12. Comparable Games filters.

## Deferred Concepts

The following concepts are intentionally deferred:

- release dates by platform;
- regional release dates;
- genre hierarchy;
- genre aliases;
- field-level data provenance;
- multiple publisher relationships;
- score or opportunity prediction;
- market-signal generation;
- historical metric snapshots;
- machine learning.

Deferring these concepts reduces premature complexity while preserving clear evolution paths.

## Design Principle

The initial domain model should support real product questions without attempting to represent every possible market-data scenario.

The guiding principle is:

> Model the complexity required by validated product decisions, not the complexity that may exist in every future scenario.