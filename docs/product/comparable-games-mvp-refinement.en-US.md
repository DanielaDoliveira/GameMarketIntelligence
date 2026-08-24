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

## Implementation status note — August 24, 2026

The domain and persistence refinements anticipated by this document have advanced substantially through GMI-25 to GMI-28.

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
- restrictive delete behavior for provenance-bearing relationships;
- PostgreSQL mappings, migrations, and integration tests.

These additions are persistence and domain foundations.

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

The final scope depends on producer need, data availability, source reliability, legal and technical viability, and domain-model suitability.

The existence of a persisted field or relationship does not automatically justify exposing it as a filter.

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

Temporal market metrics such as price, sales, reviews, and player counts should still not be static fields on `Game`.

## Persistence, API, and frontend responsibilities

The current architecture separates three concerns.

### Persistence

Stores the treated canonical dataset plus the minimum external identity and provenance needed for integrity, auditability, source removal, and future reconciliation.

### Application and API

Expose use-case-specific contracts.

The API does not need to return every persistence property.

Potential future read concepts include:

- product type;
- related products;
- companies grouped by role;
- collections;
- contextual releases;
- selected classifications;
- human-readable source information.

### Frontend

Organizes the API contracts for the producer's workflow.

The frontend should not mirror the database schema.

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

For the PoC-approved Milestone 2 persistence fields covered by GMI-25 to GMI-28, this sequence has now been completed.

Future schema changes must continue to follow the same evidence-first rule.

Generated migrations must also be reviewed for unrelated schema changes before they are applied.

## Current implementation boundary

As of the GMI-28 quality gate:

```text
Domain and persistence foundations
→ implemented for GMI-25 to GMI-28

Public API exposure
→ still selective/current contracts only

Frontend presentation
→ current Comparable Games experience only

Collector production ingestion
→ still pending

Multi-source reconciliation
→ deferred to Milestone 3
```

The full solution quality gate after GMI-28 passed:

```text
Tests: 424
Passed: 424
Failed: 0
Ignored: 0
```

## Out of scope for this MVP refinement

Future capabilities include sales and revenue analysis, engagement trends, price history, review sentiment, audience analysis, market saturation, saved research, side-by-side comparison, and predictive models.

Also deferred until separate validated increments:

- multi-source reconciliation;
- franchises;
- arbitrary multi-keyword public filtering;
- exposing every persisted classification as a frontend filter;
- automatic propagation across related game products.
