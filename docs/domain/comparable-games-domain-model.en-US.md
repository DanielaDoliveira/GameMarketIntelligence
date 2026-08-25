# Comparable Games Domain Model

## Purpose

Define the current domain model and read behavior that support the Comparable Games feature in GameMarketIntel.

The model should provide enough information to support early production and market-analysis questions while remaining source-neutral, provenance-aware, and compatible with the storage constraints of the MVP.

The model is intentionally pragmatic. It has evolved from the initial Comparable Games foundation through the source-identity, contextual-release, classification, product-relation, company, collection, and image-metadata increments validated during Milestone 2.

## Product Questions

The Comparable Games vertical should help answer:

* Which games may be considered comparable?
* Which genres are associated with each game?
* Which platforms are associated with each game?
* When was each game first released?
* Which games match a partial-name search?
* Which games match a selected genre?
* Which games match a selected platform?
* Which games were released in a selected year?
* What type of product is a game, when known?
* Which related products are known for a game?
* Which companies are associated with a game and in which roles?
* Which collections contain or contextualize a game?
* Which source records support contextual facts and associations?
* Which limitations affect interpretation of the data?

The model should later support:

* advanced Comparable Games filtering;
* genre-saturation analysis;
* launch-window analysis;
* platform comparison;
* source-aware detail views;
* historical market indicators;
* commercial performance comparison.

## Domain Scope

The current domain and persistence scope includes:

* `Game`;
* `Genre`;
* `Platform`;
* `DataSource`;
* `SourceReliability`;
* `ExternalGameRecord`;
* contextual `GameRelease`;
* `Theme`;
* `GameMode`;
* `PlayerPerspective`;
* `Keyword`;
* external identities for approved classifications;
* provenance-bearing game/classification associations;
* `GameProductType`;
* `GameProductRelation`;
* `Company`;
* `ExternalCompanyRecord`;
* `GameCompanyRole`;
* `GameCompany`;
* `Collection`;
* `ExternalCollectionRecord`;
* `GameCollection`;
* `GameImage`;
* `GameImageType`;
* source-aware image URL resolution;
* game-to-genre relationships;
* game-to-platform relationships;
* read contracts for the current Comparable Games search.

Not every persisted concept is exposed through the public API or frontend yet.

The persistence model may contain more treated information than a specific API contract or UI view needs. Application and Shared contracts should expose only the information required by each use case.

## Current Status

Implemented:

* canonical `Game`, `Genre`, and `Platform`;
* game-to-genre many-to-many relationship;
* game-to-platform many-to-many relationship;
* game-name normalization with a non-unique lookup index;
* normalized-name uniqueness for canonical classifications, companies, and collections where applicable;
* external source identity through `DataSource + ExternalId`;
* contextual release modeling with provenance;
* approved queryable classifications:
  * themes;
  * game modes;
  * player perspectives;
  * keywords;
* provenance-bearing classification associations;
* product type modeling;
* directed product relationships with provenance;
* canonical company modeling;
* company external identities;
* game/company roles with provenance;
* canonical collection modeling;
* collection external identities;
* game/collection associations with provenance;
* cover and screenshot metadata with provenance;
* source-aware construction of public image URLs from persisted image metadata;
* batch primary-cover lookup for paginated search results;
* restrictive delete behavior for provenance-bearing relationships;
* EF Core configurations;
* PostgreSQL migrations;
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
* first-release-year filtering;
* AND combination between different filter categories;
* alphabetical ordering;
* pagination;
* pagination metadata;
* `GET /api/games`;
* `GET /api/games/{id:guid}`;
* `GET /api/genres`;
* `GET /api/platforms`;
* FluentValidation for search parameters;
* OpenAPI and Scalar documentation;
* centralized API exception handling;
* standardized `ProblemDetails` responses;
* responsive Blazor Comparable Games experience;
* visible Search action;
* loading, error, empty, and no-results states;
* 460 automated tests passing across the solution during the GMI-29 implementation quality gate.

Not yet implemented in the public read experience:

* public API contracts for product type;
* public API contracts for product relationships;
* public API contracts for companies and their roles;
* public API contracts for collections;
* public API contracts for contextual releases and provenance details;
* advanced filters using the newly persisted classifications and product metadata;
* Worker ingestion of the complete approved Milestone 2 subset;
* multi-source reconciliation;
* commercial metric observations.

## Game

### Purpose

Represents one canonical game product used in market comparison and analysis.

A canonical `Game` is not tied permanently to one provider. Source-specific identity and evidence live in external records and provenance-bearing contextual entities.

### Implemented Properties

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal canonical identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Technical normalized name used for lookup and candidate discovery |
| `Description` | No | Short descriptive context |
| `FirstReleaseDate` | No | First known canonical product release date used by the current year filter |
| `ProductType` | No | Canonical product type when known |

### Name Normalization

`Name` preserves the display value after trimming.

`NormalizedName` is derived from the trimmed name using invariant uppercase normalization.

Example:

```text
Name:
The Legend of Zelda: Ocarina of Time

NormalizedName:
THE LEGEND OF ZELDA: OCARINA OF TIME
```

`Game.NormalizedName` is intentionally not unique.

Distinct products, editions, ports, or source records may legitimately have the same or very similar title. A normalized title supports lookup and candidate comparison but does not prove cross-source identity.

### Implemented Relationships

A game may:

* have multiple genres;
* be available on multiple platforms;
* have multiple contextual release records;
* have multiple external source records;
* have multiple themes;
* have multiple game modes;
* have multiple player perspectives;
* have multiple keywords;
* have multiple company associations;
* belong to multiple collections;
* have multiple source-derived image metadata records;
* participate in directed product relationships.

No product relationship automatically propagates genres, platforms, releases, companies, collections, classifications, or other fields from one `Game` to another.

## First Release Date and Contextual Releases

### `Game.FirstReleaseDate`

`FirstReleaseDate` is a canonical summary value used by the current Comparable Games release-year filter.

It answers:

> What is the first known release date for this canonical product?

It is nullable because source coverage may be incomplete.

It does not replace detailed release observations.

### `GameRelease`

`GameRelease` represents contextual release evidence.

It preserves the relevant release context, including:

* canonical `Game`;
* `Platform`;
* source `ExternalGameRecord`;
* external release identity;
* release-date precision/components;
* region when available;
* status when available;
* observation metadata.

This separation allows the model to distinguish:

```text
Game.FirstReleaseDate
→ canonical summary/filter value

GameRelease
→ platform/region/source-specific release occurrence
```

Ports, remakes, remasters, bundles, expansions, and other distinct products are not collapsed into one game merely because they are related.

## Game Images

### `GameImage`

`GameImage` stores source-derived image metadata associated with a canonical `Game`.

It replaces the previous persistence dependency on `Game.ImageUrl`.

Implemented properties:

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal image-metadata identity |
| `GameId` | Yes | Canonical game receiving the image contribution |
| `ExternalGameRecordId` | Yes | Source game observation that supplied the image |
| `ExternalId` | Yes | Identity of the image record inside the source |
| `SourceImageId` | Yes | Source asset token used later to construct a public image URL |
| `Type` | Yes | Source-neutral image purpose: `Cover` or `Screenshot` |
| `Width` | No | Source-reported width when available |
| `Height` | No | Source-reported height when available |
| `SortOrder` | No | Source ordering hint when one is available |

`ExternalId` and `SourceImageId` have intentionally different responsibilities:

```text
ExternalId
→ identifies the source image record

SourceImageId
→ identifies/addressses the source asset used to construct a rendition URL
```

For example, an IGDB observation may expose:

```text
id       = 12345
image_id = co8abc
```

which is represented as:

```text
ExternalId    = "12345"
SourceImageId = "co8abc"
Type          = Cover
```

The database does not persist a ready-made game image URL and does not store image binary content.

### Image Provenance

`GameImage` does not duplicate `DataSourceId`.

The source is derived through:

```text
GameImage
→ ExternalGameRecord
→ DataSource
```

A `GameImage` can be created only from an `ExternalGameRecord` already linked to a canonical `Game`.

The current persistence identity protects duplicate evidence through the unique combination:

```text
ExternalGameRecordId
+ ExternalId
+ Type
```

Delete behavior is restrictive for both the canonical game and the supporting external game record so that provenance cannot be removed silently.

### URL Resolution

Public image URLs are constructed after persistence.

The current read flow is:

```text
GameImage metadata
→ primary-cover selection
→ DataSource.Code
→ IGameImageUrlResolver
→ public ImageUrl
→ Shared contract
→ frontend
```

The domain does not know CDN or provider-specific URL rules.

The current source-aware resolver supports IGDB image metadata and constructs the appropriate rendition URL from `SourceImageId`.

The public search and game-details contracts continue to expose `ImageUrl?` because the frontend needs a ready-to-use URL or `null` for its existing fallback behavior. Provider identifiers, CDN rules, and `GameImage` persistence metadata remain backend concerns.

### Primary Cover Selection

The current read model selects at most one cover for the existing search and details experience.

Selection rules are deterministic:

1. only `GameImageType.Cover` records are considered;
2. a populated `SortOrder` is preferred over `null`;
3. lower `SortOrder` values are preferred;
4. `GameImage.Id` is used as a stable tie-breaker.

Game details retrieve one primary cover.

Paginated search results retrieve primary-cover metadata in batch for all games on the page, avoiding an N+1 query pattern.

Screenshots are persisted as metadata for future detail/gallery use but are not exposed by the current public read contract.

## Product Type

`GameProductType` describes what the product itself is.

Current domain vocabulary:

* `MainGame`;
* `Dlc`;
* `Expansion`;
* `Bundle`;
* `StandaloneExpansion`;
* `Mod`;
* `Episode`;
* `Season`;
* `Remake`;
* `Remaster`;
* `ExpandedGame`;
* `Port`;
* `Fork`;
* `PackAddon`;
* `Update`.

`Game.ProductType` is nullable.

Unknown product type is represented by `null`, not by a synthetic `Unknown` value.

The enum is source-neutral. Provider adapters must map source values explicitly and must not depend on matching provider numeric IDs to the domain enum's numeric values.

## Product Relationships

### Purpose

`GameProductRelation` represents a directed relation between two canonical products.

`GameProductType` answers:

> What is this product?

`GameProductRelationType` answers:

> How is this product related to another product?

### Current Relation Vocabulary

* `DlcOf`;
* `ExpansionOf`;
* `StandaloneExpansionOf`;
* `RemakeOf`;
* `RemasterOf`;
* `PortOf`;
* `ExpandedGameOf`;
* `EpisodeOf`;
* `SeasonOf`;
* `ForkOf`;
* `PackAddonOf`;
* `UpdateOf`;
* `BundleContains`;
* `VersionOf`.

Example:

```text
The Legend of Zelda: Ocarina of Time 3D
    └── RemakeOf → The Legend of Zelda: Ocarina of Time
```

### Provenance

A `GameProductRelation` preserves:

* `SourceGameId`;
* `TargetGameId`;
* `ExternalSourceGameRecordId`;
* `ExternalTargetGameRecordId`;
* `RelationType`.

Both external game records must:

* already be linked to canonical games;
* belong to the same `DataSource`.

A game cannot have a relation with itself through one relation record.

The same external source/target pair plus relation type is unique.

All persistence FKs use restrictive delete behavior.

## Genre

### Purpose

Represents a canonical game genre used for filtering, comparison, aggregation, and saturation analysis.

### Implemented Properties

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Normalized value used for uniqueness and comparison |

### Relationships

* A genre may be associated with multiple games.
* A game may be associated with multiple genres.

Genre classification may differ between sources.

Normalized-name uniqueness prevents duplicate canonical concepts caused only by casing or formatting differences. Cross-source equivalence still requires source-aware mapping and must not be inferred from the normalized name alone.

## Platform

### Purpose

Represents a canonical game platform used for filtering, release context, market comparison, and platform-level analysis.

### Implemented Properties

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal identity |
| `Name` | Yes | Platform display name |
| `Family` | No | Product family or ecosystem |
| `Manufacturer` | No | Platform manufacturer |
| `ImageUrl` | No | External image reference |

### Relationships

* A platform may be associated with multiple games.
* A game may be associated with multiple platforms.
* A contextual `GameRelease` belongs to one platform.

Platform image handling remains unchanged in the current model. Game images are stored as source-derived metadata and resolved to public URLs only when required by a read use case; no game image binaries are stored in PostgreSQL.

## Queryable Classifications

The approved Milestone 2 queryable classifications are modeled as separate canonical entities:

* `Theme`;
* `GameMode`;
* `PlayerPerspective`;
* `Keyword`.

Each canonical classification has:

* `Id`;
* `Name`;
* `NormalizedName`.

Each classification also has its own external identity record:

* `ExternalThemeRecord`;
* `ExternalGameModeRecord`;
* `ExternalPlayerPerspectiveRecord`;
* `ExternalKeywordRecord`.

Source identity follows:

```text
DataSourceId + ExternalId
```

The canonical game/classification associations preserve both:

* the canonical IDs used by GMI;
* the external record IDs that support the association.

This prevents the canonical relationship from losing its provenance.

The current persistence associations are:

* `GameTheme`;
* `GameGameMode`;
* `GamePlayerPerspective`;
* `GameKeyword`.

Public filtering by these classifications remains a later API/frontend decision.

## Company

### Purpose

Represents a canonical source-neutral company.

A company is not permanently classified as developer or publisher. The role belongs to the relationship between a company and a specific game.

### Implemented Properties

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal canonical identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Normalized value used for canonical uniqueness |

`Company.NormalizedName` is unique in the current Milestone 2 model.

### External Company Identity

`ExternalCompanyRecord` preserves:

* `DataSourceId`;
* `ExternalId`;
* optional `CompanyId`;
* `FirstSeenAt`;
* `LastSeenAt`;
* optional `SourceUpdatedAt`.

`DataSourceId + ExternalId` is unique.

An external company record may remain unlinked until there is enough evidence.

Once linked, it cannot be silently relinked to another canonical company.

### Game Company Role

`GameCompanyRole` currently includes:

* `Developer`;
* `Publisher`;
* `Porting`;
* `Supporting`.

### `GameCompany`

`GameCompany` represents the provenance-bearing N:N relationship between games and companies.

It preserves:

* `GameId`;
* `CompanyId`;
* `ExternalGameRecordId`;
* `ExternalCompanyRecordId`;
* `Role`.

The same company may:

* participate in many games;
* have different roles in different games;
* have multiple supported roles for the same game.

The persistence identity is:

```text
ExternalGameRecordId
+ ExternalCompanyRecordId
+ Role
```

All FKs use restrictive delete behavior.

Company participation is context and evidence. It is not automatic proof of officiality, authorization, or commercial legitimacy.

## Collection

### Purpose

Represents a canonical source-neutral game collection.

Collections are approved for the current MVP persistence model.

Franchises remain deferred.

### Implemented Properties

| Property | Required | Purpose |
| --- | ---: | --- |
| `Id` | Yes | Internal canonical identity |
| `Name` | Yes | Display name |
| `NormalizedName` | Yes | Normalized value used for canonical uniqueness |

`Collection.NormalizedName` is unique in the current Milestone 2 model.

### External Collection Identity

`ExternalCollectionRecord` preserves:

* `DataSourceId`;
* `ExternalId`;
* optional `CollectionId`;
* `FirstSeenAt`;
* `LastSeenAt`;
* optional `SourceUpdatedAt`.

`DataSourceId + ExternalId` is unique.

An external collection record may remain unlinked and, once linked, cannot be silently relinked to another canonical collection.

### `GameCollection`

`GameCollection` represents the provenance-bearing N:N relationship between games and collections.

It preserves:

* `GameId`;
* `CollectionId`;
* `ExternalGameRecordId`;
* `ExternalCollectionRecordId`.

The persistence identity is:

```text
ExternalGameRecordId
+ ExternalCollectionRecordId
```

A collection may contain multiple games, and a game may participate in more than one collection when supported by source evidence.

All FKs use restrictive delete behavior.

## Data Source and External Identity

### Canonical Identity

`Game.Id` is the canonical GMI identity.

Canonical identity must not depend on Steam, IGDB, or any other single source.

### External Source Identity

An external source record is identified safely by:

```text
DataSourceId + ExternalId
```

`ExternalGameRecord` preserves the source identity of a game observation and may optionally link to a canonical `Game`.

The same general pattern is used for classifications, companies, and collections.

### Source-Neutral Rule

The domain must avoid permanent provider-specific properties such as:

```text
IgdbId
SteamId
WikidataId
```

Provider-specific IDs belong in external identity records.

Normalized names may help discover reconciliation candidates, but they never prove equivalence by themselves.

Multi-source reconciliation remains outside the current Milestone 2 implementation.

## Provenance Model

GMI stores treated canonical data plus the minimum source evidence required to understand, audit, delete, or later reconcile a contribution.

Provenance is preserved on contextual entities and associations where the source matters materially.

Examples include:

* releases;
* queryable classification associations;
* product relationships;
* game/company roles;
* game/collection membership;
* game image metadata.

The current design deliberately avoids a generic polymorphic field-history table.

More granular field-level provenance should be introduced only if validated product or legal requirements justify its storage and complexity.

## Implemented Relationship Model

The current relationship model includes:

```text
Game
  ├── many-to-many → Genre
  ├── many-to-many → Platform
  ├── source identities → ExternalGameRecord
  ├── contextual releases → GameRelease → Platform
  ├── provenance associations → Theme
  ├── provenance associations → GameMode
  ├── provenance associations → PlayerPerspective
  ├── provenance associations → Keyword
  ├── provenance associations → Company + Role
  ├── provenance associations → Collection
  ├── source-derived image metadata → GameImage
  └── directed provenance relations → Game
```

This persistence model is richer than the current public search contract.

The API must expose source-neutral read contracts rather than returning domain or EF Core entities directly.

## Comparable Games Search

### Current Search Contract

The current query contract remains:

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

The newly persisted GMI-27/GMI-28 concepts are not yet public search parameters.

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

The current implementation does not accept multiple genre or platform values in the same request.

Advanced filtering remains a later API/frontend increment.

### Partial-Name Search

Game-name search:

* trims the supplied term;
* uses PostgreSQL `ILike`;
* ignores letter casing;
* matches the term within any part of the game name.

`Game.NormalizedName` exists for technical consistency and candidate discovery but does not replace the current partial-name search semantics.

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

`ReleaseYear` uses `Game.FirstReleaseDate`.

When supplied:

* games without `FirstReleaseDate` are excluded;
* the stored year must match the supplied year;
* the supplied year cannot be greater than the current year.

Detailed `GameRelease` rows do not currently drive the public release-year filter.

## Implemented Comparable Games Response

The current public search response contains:

```text
Game identifier
Game name
Description
Release date
Image URL
Genres
Platforms
```

The public contract continues to use a user-facing release-date field even though the canonical domain property is now named `FirstReleaseDate`.

Genres and platforms are returned as lightweight read categories.

The current frontend does not automatically receive every persisted GMI-27/GMI-28 concept.

Future API contracts may expose selected information such as:

* product type;
* related products;
* companies and roles;
* collections;
* contextual releases;
* source information.

That exposure should be driven by product use cases rather than by the shape of the database.

## Storage Constraints

The Neon Free storage limit remains a product constraint.

The model prioritizes:

* normalized structured data;
* source identities;
* selected canonical values;
* contextual records needed for product questions;
* provenance required for audit, deletion, attribution, and future reconciliation.

The operational database should avoid storing:

* raw API payloads;
* complete JSON responses;
* HTML pages;
* image binaries;
* unnecessary source duplicates;
* complete auxiliary-provider mirrors.

Storage growth may require:

* selective source ingestion;
* reduced historical granularity;
* aggregation;
* retention policies;
* postponement of storage-intensive features.

## Delete Behavior and Integrity

Provenance-bearing relationships introduced in Milestone 2 use restrictive deletes.

A referenced canonical or external record cannot be deleted while dependent association rows still exist.

Representative integration tests validate restrictions involving:

* product relationships;
* companies;
* collections;
* external game records.

The integration-test database reset must include every table added by completed migrations so tests do not leak state through the shared PostgreSQL fixture.

## Validation Responsibilities

Validation remains divided by responsibility.

### Domain Validation

Domain entities protect invariants independent of the caller.

Examples:

* required names;
* trimming and normalization;
* maximum lengths;
* external identity validity;
* linked-record consistency;
* same-source provenance requirements;
* prevention of self-product relationships;
* prevention of silent relinking.

### Application Validation

Application validators protect use-case inputs.

Current search rules include:

```text
Page >= 1
PageSize between 1 and 100
ReleaseYear <= current year
```

### Persistence Validation

EF Core/PostgreSQL constraints enforce persistence-level integrity such as:

* primary keys;
* composite keys;
* unique external identities;
* canonical normalized-name uniqueness where approved;
* foreign keys;
* restrictive delete behavior;
* duplicate-provenance prevention.

## Implementation Progress

Completed foundation:

1. `DataSource` and reliability foundation;
2. canonical `Genre`, `Platform`, and `Game`;
3. game-to-genre and game-to-platform relationships;
4. Comparable Games search and details API;
5. genre and platform list APIs;
6. responsive Comparable Games frontend;
7. initial real-source research and IGDB PoC;
8. source-neutral identity and provenance design.

Completed Milestone 2 persistence increments:

1. **GMI-25** — external source identity;
2. **GMI-26** — contextual releases with provenance;
3. **GMI-27** — approved queryable classifications;
4. **GMI-28** — product types, product relationships, companies, company roles, collections, provenance, constraints, mappings, tests, and migrations.

GMI-28 final quality gate:

```text
Build: passed
Tests: 424 passed
Failures: 0
Ignored: 0
```

## Next Implementation Steps

Within the persistence milestone:

1. complete GMI-29 documentation and close the image-metadata increment;
2. GMI-30 — persistence and storage-budget validation.

After the approved persistence model is complete:

1. implement the Worker/Collector ingestion flow for the approved source subset;
2. populate the canonical dataset with treated real data;
3. expose selected new concepts through Application and API contracts;
4. decide which persisted information belongs in Comparable Games cards, details, and advanced filters;
5. preserve source-aware presentation and attribution;
6. validate the complete production flow with real data.

The frontend should organize the API's use-case-specific contracts. It should not mirror the database schema or expose every internal provenance identifier.

## Deferred Concepts

The following concepts remain intentionally deferred unless a later Jira item explicitly activates them:

* multi-source reconciliation;
* franchises;
* field-level generic history tables;
* multiple genres in one current search request;
* multiple platforms in one current search request;
* advanced similarity scoring;
* recommendation systems;
* sales and revenue observations;
* historical commercial metric snapshots;
* machine learning;
* public filters for every persisted classification;
* automatic propagation of data across product relationships.

## Design Principle

The domain model should support validated product questions without becoming a copy of an upstream API schema.

The guiding principle remains:

> Model the complexity required by validated product decisions, not the complexity that may exist in every future scenario.

A second principle now also applies:

> Preserve enough identity and provenance to explain where a fact or relationship came from without forcing the public API or frontend to expose the complete persistence model.
