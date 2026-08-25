# Comparable Games — MVP Refinement

## Purpose

Comparable Games is the primary research experience of the Game Market Intelligence MVP.

It should help producers discover and refine candidate comparable games through structured, source-aware research.

The feature is not intended to declare that every returned game is a direct competitor.

## Current capability

The existing implementation provides:

- search by name;
- genre filter;
- platform filter;
- release-year filter;
- visible Search button;
- combined form submission;
- URL-based search state;
- pagination;
- loading, error, empty, and no-results states;
- responsive game cards.

This is a strong basic discovery flow but still represents a shallow comparison stage.

## Implementation status note — August 25, 2026

The domain and persistence refinements anticipated by this document have advanced substantially through GMI-25 to GMI-29, while GMI-30 has now validated the current persistence model against migration, query-plan, and representative storage scenarios.

Implemented foundations now include:

- source-neutral external game identity through `DataSource + ExternalId`;
- `Game.FirstReleaseDate` separated from contextual `GameRelease`;
- provenance-bearing contextual releases;
- canonical themes, game modes, player perspectives, and keywords;
- external identities and provenance-bearing associations for those classifications;
- `Game.ProductType`;
- directed product relationships with provenance;
- canonical companies;
- external company identities;
- game/company roles with provenance;
- canonical collections;
- external collection identities;
- game/collection associations with provenance;
- source-derived game image metadata through `GameImage`;
- `GameImageType` with `Cover` and `Screenshot`;
- source image record identity through `ExternalId`;
- source asset addressing through `SourceImageId`;
- source-aware public image URL resolution in the backend;
- batch primary-cover lookup for paginated search results;
- restrictive delete behavior for provenance-bearing relationships;
- PostgreSQL mappings, migrations, and integration tests.

GMI-30 additionally validated that:

- the migration chain can be applied from zero in PostgreSQL;
- the local existing database can be migrated to the current schema;
- the EF model and migration snapshot are aligned;
- current public contracts remain compatible;
- genre and platform filters use their expected indexes;
- batch cover lookup uses the `GameId` image index;
- current name substring and release-year searches still use sequential scans at the measured 10,000-game volume, but remain inexpensive enough that no additional index is currently justified;
- indexes already represent a material part of storage cost;
- high-cardinality associations can grow much faster than the canonical `Games` table.

These additions are persistence, domain, and operational foundations.

They are **not yet equivalent to public API or frontend features**.

Advanced filters, richer details, source presentation, reliability controls, and other UI changes still require separate Application/API and frontend decisions.

The persistence model is intentionally richer than any single screen or endpoint.

## MVP refinement goals

The complete MVP should refine two existing areas and connect them through a third capability:

1. advanced filters in Comparable Games;
2. populated and informative Data Sources;
3. reliability-based filtering.

## 1. Basic and advanced filters

Comparable Games should remain one page with progressive disclosure.

### Basic filters

Visible by default:

- game name;
- genre;
- platform;
- release year or period;
- Search action.

### Advanced filters

A control such as `Advanced filters` should reveal additional criteria without overwhelming the initial interface.

Candidate filters include:

- subgenre;
- tags or gameplay characteristics;
- themes;
- game modes;
- player perspective;
- developer;
- publisher;
- release status;
- product type;
- single-player, multiplayer, cooperative, or competitive support;
- business model;
- source reliability.

The final scope depends on producer need, data availability, source reliability, legal and technical viability, domain-model suitability, and measured storage/query cost.

The existence of a persisted field or relationship does not automatically justify exposing it as a filter.

The existence of a provider field also does not automatically justify ingesting it at maximum depth.

### Why progressive disclosure

A single large form could be difficult to use. A separate advanced-search page would split one research intention across multiple routes.

Progressive disclosure preserves one page, one result model, one shareable URL, one clear user goal, a simple initial experience, and room for deeper research.

## 2. Data Sources as a product feature

For each source, the page should show:

- source name;
- organization or owner;
- source type;
- official or independent status;
- data categories supplied;
- reliability by data category;
- known limitations;
- collection method;
- update or verification frequency;
- attribution requirements;
- relevant usage restrictions;
- original source URL.

Data Sources should help producers understand provenance, inspect the original site, distinguish facts from estimates, and understand missing data.

Providing source links reinforces that the application is a non-commercial research aid rather than a replacement for original platforms.

The source experience should present human-readable provenance.

Internal persistence identifiers such as `ExternalGameRecordId`, `ExternalCompanyRecordId`, or `ExternalCollectionRecordId` should not be exposed directly in the frontend.

## 3. Reliability filter

A reliability filter is the preferred first expansion after advanced filters and Data Sources.

It aligns more directly with the application's value proposition than a source filter.

### Possible modes

- High confidence;
- Balanced;
- Broad coverage.

### High confidence

Prioritizes official, primary, strongly verified data, or agreement among reliable sources.

The interface must warn that coverage may be reduced.

### Balanced

Combines official data, curated sources, reliable aggregators, and normalized values with acceptable provenance.

This is the likely default.

### Broad coverage

May include recognized estimates, structured community data, lower-confidence attributes, or conflicting records.

All such information must remain clearly identified.

## Why reliability precedes source filtering

A producer often cares first about whether a value is trustworthy, not which website supplied it.

Reliability filtering allows workers and normalization rules to continue selecting and reconciling data while the producer controls the acceptable confidence threshold.

## Source filtering as a later possibility

A source filter remains valid but lower priority.

If implemented, it should support multiple selected sources, preserve aggregation, clarify any/all matching behavior, and function mainly as an advanced auditing or reproducibility tool.

## Normalized data and provenance

The default experience should present normalized Game Market Intelligence data while preserving source detail.

Example:

```text
Release date
October 18, 2024

Normalized by Game Market Intelligence
Sources:
- Steam — official
- IGDB — curated, matching value
- Aggregated source — conflicting date
```

The exact source composition in this example is illustrative.

Milestone 2 currently uses IGDB as the active source. Wikidata and Steam remain future Milestone 3 integrations.

## Domain-model impact

The original refinement identified concepts that the initial `Game` model would eventually need to support.

A large part of that foundation is now implemented, but not all information was added directly to `Game`.

Current source-neutral structure includes concepts such as:

```text
Game
├── Genres
├── Platforms
├── Themes
├── GameModes
├── PlayerPerspectives
├── Keywords
├── ProductType
├── Companies + Roles
├── Collections
├── Contextual Releases
├── Product Relationships
├── GameImage metadata
└── External source identities and provenance
```

This confirms an important modeling decision from the original refinement:

> Not all information should be added directly to `Game`.

Source-specific identity and provenance are modeled separately.

For example:

```text
Game
    ↓
ExternalGameRecord
    ↓
DataSource + ExternalId
```

And contextual associations preserve the external records that support them.

The current model also distinguishes:

```text
Game.FirstReleaseDate
→ canonical summary value used by the current year filter

GameRelease
→ contextual platform/region/source release evidence
```

Product relationships remain separate from product type:

```text
GameProductType
→ what the product is

GameProductRelationType
→ how one product relates to another
```

Related products do not automatically inherit or propagate genres, platforms, releases, companies, collections, classifications, or other metadata.

Game image handling now follows the same separation principle.

The canonical `Game` no longer persists a ready-made `ImageUrl`.

Instead:

```text
GameImage
→ ExternalGameRecord
→ DataSource
```

stores source-derived metadata and provenance, while a backend resolver converts `SourceImageId` into a public URL when a read use case needs one.

The current public contracts intentionally remain simple:

```text
GameDetails.ImageUrl?
GameSearchItem.ImageUrl?
```

The frontend therefore receives a ready-to-use URL or `null` for fallback behavior and does not need to know provider-specific image identifiers or CDN rules.

The current search flow resolves primary covers in batch for the games on the page, avoiding N+1 image queries.

Screenshots are persisted as metadata for future detail/gallery work but are not exposed by the current MVP read contract.

Temporal market metrics such as price, sales, reviews, and player counts should still not be static fields on `Game`.

## Storage-aware ingestion boundary

GMI-30 introduced an important product/persistence distinction:

```text
catalog coverage
≠
metadata depth
```

The target is not to ingest an arbitrary fixed maximum number of games simply because the database has a storage limit.

If a source exposes a larger set of games that are relevant to the approved MVP scope, preserving those relevant catalog entries is preferred over discarding them solely to satisfy an arbitrary record-count cap.

Capacity should instead be managed first through product-driven metadata depth.

For example:

```text
preserve relevant games
↓
persist the classifications and relationships required by approved product questions
↓
limit, defer, or avoid non-essential high-cardinality metadata
↓
monitor actual table and index growth
```

This means the Collector should not interpret "the provider exposes it" as sufficient reason to persist every available field or every association at maximum depth.

The source-product question map remains the decision boundary.

### GMI-30 representative storage evidence

The baseline synthetic scenario used:

```text
10,000 Games
20,000 GameGenres
20,000 GamePlatforms
10,000 ExternalGameRecords
16,666 GameImages
```

Measured PostgreSQL storage after `ANALYZE` was approximately:

```text
table data  ~7.4 MB
indexes     ~11 MB
total       ~19 MB
```

A second pressure scenario kept the same 10,000-game catalog and added:

```text
30,000 contextual releases
30,000 theme associations
20,000 game-mode associations
20,000 player-perspective associations
80,000 keyword associations
20,000 company associations
5,000 collection associations
2,500 product relations
```

Measured storage became approximately:

```text
table data  ~29 MB
indexes     ~33 MB
total       ~63 MB
```

The increase was approximately 44 MB without adding more games.

The strongest synthetic pressure points were:

- `game_keywords`;
- `game_releases`;
- `game_images`;
- `game_themes`;
- `game_companies`.

This evidence reinforces that storage risk is driven primarily by multiplicative associations and supporting indexes, not only by the canonical game count.

The measured ~63 MB / 10,000-game pressure scenario is a budgeting reference only. It must not be treated as a fixed linear production forecast.

## Query-plan observations relevant to Comparable Games

GMI-30 measured the current query shapes with a 10,000-game synthetic local dataset.

### Name search

The current substring search:

```text
ILIKE '%term%'
```

used a sequential scan of `Games`.

Approximate local execution time for the representative query was 3.7 ms.

The current B-tree `NormalizedName` index does not support this substring pattern.

Decision:

- keep the current behavior for the measured MVP scale;
- do not introduce a trigram or other specialized index until production-like volume or latency demonstrates a need.

### Genre filter

The genre path used `IX_GameGenres_GenreId`.

Decision:

- current index is justified;
- no additional genre index is required.

### Platform filter

The platform path used `IX_GamePlatforms_PlatformId`.

Decision:

- current index is justified;
- no additional platform index is required.

### Release-year filter

The current year query applies `EXTRACT(YEAR FROM FirstReleaseDate)` and used a sequential scan.

Approximate local execution time for the representative query was 1.8 ms.

Decision:

- keep the current behavior for the measured MVP scale;
- do not create a functional year index solely to remove the sequential scan.

### Batch cover lookup

Primary-cover lookup used `IX_game_images_GameId`.

Decision:

- current image index is justified;
- the batch lookup remains the preferred search-page access path.

These local measurements are evidence for current design decisions, not production service-level guarantees.

A sequential scan is not automatically considered a defect.

## Persistence, API, and frontend responsibilities

The current architecture separates three concerns.

### Persistence

Stores the treated canonical dataset plus the minimum external identity and provenance needed for integrity, auditability, source removal, and future reconciliation.

Persistence must also respect the operational storage budget.

That means it should preserve data because it supports:

- an approved product question;
- required provenance;
- reconciliation;
- attribution;
- operational correctness.

It should not become a warehouse of every provider field merely because those fields are available.

### Application and API

Expose use-case-specific contracts.

The API does not need to return every persistence property.

Current read contracts already include a deliberately simple image boundary:

- `GameDetails.ImageUrl?`;
- `GameSearchItem.ImageUrl?`.

These values are derived from persisted image metadata rather than stored directly on `Game`.

Potential future read concepts include:

- product type;
- related products;
- companies grouped by role;
- collections;
- contextual releases;
- selected classifications;
- screenshot/gallery data if validated by the detail experience;
- human-readable source information.

### Frontend

Organizes the API contracts for the producer's workflow.

The frontend should not mirror the database schema.

That rule now applies explicitly to image handling as well: `GameImage`, `ExternalGameRecord`, `SourceImageId`, and provider-specific CDN rules remain backend concerns. The frontend consumes only the resolved `ImageUrl?` required by the current card/details experience.

For example, persistence may retain:

```text
ExternalGameRecordId
ExternalCompanyRecordId
DataSourceId
SourceUpdatedAt
```

while the user-facing experience may show:

```text
Developer
Grezzo

Source
IGDB
```

Storage-budget decisions are also backend/ingestion concerns.

The frontend must not expose arbitrary metadata omissions as if a source definitively lacked that data.

## Migration rule

The original migration rule remains valid as a general design principle:

1. confirm producer needs;
2. define MVP fields and filters;
3. evaluate candidate sources;
4. verify availability and permissions;
5. review the domain model;
6. propose the new model;
7. validate architecture and ingestion impact;
8. create migration.

For the PoC-approved Milestone 2 persistence fields covered by GMI-25 to GMI-29, this sequence has now been completed.

GMI-30 validated that the resulting schema can be applied both from zero and over the existing local database, with no pending EF model drift.

Future schema changes must continue to follow the same evidence-first rule.

Generated migrations must also be reviewed for unrelated schema changes before they are applied.

New indexes are subject to the same evidence rule: they must answer a demonstrated query need and justify their storage/write cost.

## Current implementation boundary

As of the current GMI-30 validation checkpoint:

```text
Domain and persistence foundations
→ implemented for GMI-25 to GMI-29

Persistence operability
→ migrations validated from zero and over the existing local database
→ representative query plans measured
→ representative storage pressure measured

Public API exposure
→ still selective/current contracts only
→ game image URLs are derived from persisted metadata

Frontend presentation
→ current Comparable Games experience only
→ existing cover/fallback boundary preserved

Collector production ingestion
→ still pending

Multi-source reconciliation
→ deferred to Milestone 3
```

The current full-solution quality gate during GMI-30 passed:

```text
Tests: 460
Passed: 460
Failed: 0
Ignored: 0
```

Current capacity principle:

```text
preserve relevant catalog coverage
→ control metadata depth by product value
→ monitor high-cardinality associations and indexes
→ optimize only when measured evidence justifies it
```

## Out of scope for this MVP refinement

Future capabilities include sales and revenue analysis, engagement trends, price history, review sentiment, audience analysis, market saturation, saved research, side-by-side comparison, and predictive models.

Also deferred until separate validated increments:

- multi-source reconciliation;
- franchises;
- arbitrary multi-keyword public filtering;
- exposing every persisted classification as a frontend filter;
- automatic propagation across related game products;
- speculative search/index optimization unsupported by measured need.
