# ADR-0003 — External Source Identity and Provenance Boundaries

## Status

Accepted

## Date

2026-08-17

## Context

GameMarketIntel is implementing IGDB as the first active catalogue source for
the Comparable Games MVP. Wikidata and Steam remain planned for later
increments. The first implementation must not make Steam a requirement for
catalogue identity or admission, and it must not require provider-specific
properties such as `IgdbId`, `WikidataId`, or `SteamId` on `Game`.

The current domain contains internal `Game`, `Genre`, `Platform`, and
`DataSource` entities, but it does not persist the identity of a record within
an external source. `DataSource` is not currently connected to imported games.
Normalized names support search and candidate discovery, but names do not
provide safe cross-source identity.

The IGDB PoC established these invariants:

- a record is safely identified within a source by `Source + ExternalId`;
- each canonical game has its own GMI `Game.Id`;
- official cross-source identifiers have priority for reconciliation;
- name and other composite signals may produce candidates, but not automatic
  merges;
- base games, DLCs, bundles, remakes, remasters, ports, and editions remain
  separate products;
- missing or conflicting source data must remain transparent;
- complete raw source payloads and repeated snapshots are not stored by
  default.

The architecture must also support an external record that has been collected
but cannot yet be linked safely to a canonical game. Requiring every external
identity to point immediately to `Game` would force unsafe merges, duplicate
canonical games, or data loss when a later source lacks a strong cross-source
identifier.

The persistence design now also operates under a measured capacity constraint.
GMI-30 confirmed that relationship cardinality and supporting indexes can grow
materially faster than the canonical `Games` table. Capacity therefore cannot
be governed by an arbitrary maximum game count alone.

## Decision Drivers

- preserve provider-independent canonical identity;
- support IGDB now without blocking Wikidata and Steam later;
- keep Steam optional for identity and catalogue admission;
- support idempotent imports through a stable source identity;
- allow unlinked, rejected, or under-review source records without forcing unsafe canonical links;
- preserve minimum synchronization and provenance evidence;
- avoid automatic reconciliation by name;
- preserve relational integrity where practical;
- avoid implementing the complete Milestone 3 reconciliation system early;
- remain compatible with the zero-cost PostgreSQL operating model;
- preserve relevant catalogue coverage before reducing non-essential metadata depth;
- treat high-cardinality associations and indexes as explicit capacity dimensions.

## Considered Options

### Provider-specific identifiers on `Game`

Examples include `IgdbId`, `WikidataId`, and `SteamId` columns.

This option is rejected because each new source would change the canonical
entity and database schema. It would also encourage source presence to become
part of canonical identity.

### External references that must always point to `Game`

This option preserves multiple identifiers per game and is sufficient for a
single already-reconciled source. It is rejected as the complete foundation
because a later unmatched or conflicting source record could not be retained
without immediately creating or selecting a canonical game.

### Independent external source records with an optional canonical link

This option stores the source identity independently and allows a record to be
linked to a canonical game only when the available evidence is sufficient. It
supports initial IGDB creation, idempotent re-import, future reconciliation,
quarantine, inactivity, and reactivation.

This option is selected.

### Complete field-level observation and reconciliation model now

This option would provide maximum historical and conflict detail. It is
deferred because it would implement substantial Milestone 3 scope before a
second source is evaluated and would increase storage and operational
complexity in the zero-cost MVP.

### Fixed maximum catalogue size

This option would cap the number of canonical games persisted regardless of
which metadata consumes the storage budget.

This option is rejected.

GMI-30 showed that the strongest storage growth comes from multiplicative
associations and their indexes rather than from the canonical `Games` table
alone. A fixed catalogue cap would therefore sacrifice useful coverage without
addressing the primary capacity drivers.

## Decision

Represent an external game record independently from the canonical `Game`.

The durable identity rules are:

- `Game.Id` identifies a canonical GMI game;
- `DataSource + ExternalId` identifies one record within an external source;
- an external game record may have an optional link to one canonical `Game`;
- a canonical `Game` may be linked to zero or many external game records;
- `(DataSourceId, ExternalId)` must be unique for external game records;
- names and normalized names are search and candidate signals, never safe
  source identity;
- provider IDs must not become provider-specific properties on `Game`.

The conceptual model is:

```text
DataSource
└── ExternalGameRecord
    ├── ExternalId
    ├── optional GameId
    ├── source update metadata
    ├── collection/last-seen metadata
    └── processing status

Game
└── zero or more linked ExternalGameRecords
```

The exact entity names, fields, statuses, relationships, and table mappings are
not fixed by this ADR. They must be refined during the domain and persistence
review before migrations are created.

## Source and Layer Boundaries

- provider DTOs, authentication, HTTP queries, pagination, and retry behavior
  remain at the Collector/provider boundary;
- a provider-specific mapper converts source contracts into a source-neutral
  application import model;
- Application coordinates lookup by source identity, create/update behavior,
  idempotency, validation, and persistence ports;
- Domain enforces provider-independent invariants and never depends on IGDB,
  Wikidata, Steam, HTTP, JSON, offsets, or Entity Framework Core;
- Infrastructure implements PostgreSQL persistence, constraints, transactions,
  checkpoints, and required synchronization state;
- the Worker/job orchestrates execution and remains thin;
- future reconciliation remains separate from provider clients and Workers.

The accepted flow is:

```text
Provider response
→ provider-specific mapper
→ source-neutral import model
→ application import use case
→ domain rules and persistence ports
→ PostgreSQL implementation
```

Provider responses must not be mapped directly into Entity Framework entities.

## Canonical Materialization

The canonical database belongs to GMI. IGDB is the base source and general
fallback for the first MVP, but canonical values may later be selected from
multiple sources through field- and context-specific rules. Steam is preferred
only for authorized facts about its own ecosystem; Wikidata may fill gaps,
enrich records, preserve cross-identifiers, or raise inconsistencies for
evaluation.

GMI does not retain complete interchangeable catalog copies and does not allow
the producer to replace the displayed dataset by selecting a provider. It
stores the accepted value plus the minimum external identity and provenance
required to explain, synchronize, remove, or recompose it.

Catalogue coverage and metadata depth are separate decisions.

If a source exposes additional games that are relevant to the approved product
scope, those games should not be discarded merely because a validation sample
used a smaller number of records.

The preferred capacity order is:

```text
preserve relevant catalogue coverage
→ persist metadata required by approved product questions
→ control or defer non-essential high-cardinality metadata
→ monitor table and index growth
```

Provider availability alone does not justify persisting every field or
association at maximum depth.

## Provenance Boundaries

Canonical entities and source assertions do not have identical provenance
needs.

- games and queryable taxonomy concepts use internal GMI identity while
  preserving their source identity;
- releases preserve source, platform, region, precision, and status because
  dates may differ by context and source;
- product relationships preserve the declaring source and do not merge or
  propagate data automatically;
- images preserve source image identifiers, provenance, optional dimensions,
  and ordering metadata; public URLs are derived later by source-aware backend
  resolution rather than persisted on the canonical `Game`; binary storage
  remains deferred;
- cross-source identifiers and websites remain evidence until validated for
  the intended use;
- absence in one collection run does not by itself prove source removal and
  must not automatically mark a record missing/inactive or delete a canonical
  contribution;
- an existing source contribution may be synchronized out of the current state
  only when the relevant observation is known to be complete and reliable for
  that scope, or when the source explicitly reports removal;
- removing a source-specific association does not delete the corresponding
  external identity record;
- source-specific changes must not remove independent evidence from other
  sources;
- source-specific retention or termination rules may require deleting that
  source's eligible contributions; provenance must make those values locatable
  and allow an authorized fallback to be materialized without changing
  `Game.Id`.

`DataSource` also carries operational metadata needed by the integration, such
as public attribution, official links, status, and retention/removal rules.
Every integration must use authorized endpoints and honor its own license and
terms. SteamDB remains a manual research reference and cannot feed collection,
persistence, or public API responses.

The implementation may use entity-specific external-identity mappings when
needed to retain foreign-key integrity. A single polymorphic table with an
unconstrained `EntityType + EntityId` pair is not required by this decision.

## Synchronization and Retention Boundaries

This ADR distinguishes source reconciliation from storage retention.

The operational database is intended to preserve:

```text
treated current canonical state
+
active external identities
+
minimum provenance required to explain and recompute that state
```

It is not intended to retain indefinite per-run history.

A source value or association that is absent from one collection run is not
automatically considered removed because the run may be partial, a field may
not have been requested, or the source may not guarantee that the returned set
is complete.

Therefore:

```text
not observed
≠
confirmed removed
```

A contribution may be synchronized out of the current state only when the
Collector knows that the relevant scope was requested, completed successfully,
and represents the current complete source state, or when the source explicitly
reports removal.

Capacity-driven deletion is a separate concern. Removing old historical or
auxiliary data to protect the database storage budget must never be interpreted
as evidence that a source changed or removed a fact.

The current zero-cost operating model should treat the approximately 0.5 GB
Neon Free storage allowance as a product constraint.

Capacity actions must preserve:

1. current canonical data;
2. active external identities;
3. provenance that still supports the current state;
4. recent useful historical or auxiliary data only when such history exists.

If future functionality introduces historical records, it must define an
explicit retention window when introduced rather than defaulting to permanent
storage.

Capacity-driven reduction of metadata depth must also remain distinct from
source semantics. If GMI intentionally does not persist every available
keyword, screenshot, release, or other high-cardinality association, that
omission must not be interpreted as proof that the source lacks the omitted
data.

## GMI-30 Persistence and Capacity Evidence

GMI-30 validated the current persistence architecture locally against
PostgreSQL 17.

### Migration integrity

Validated:

- the complete migration chain applies successfully from an empty PostgreSQL
  database;
- the existing local PostgreSQL database can be upgraded to the current schema;
- the EF Core model has no pending model changes relative to the current
  migration snapshot;
- the complete build succeeds;
- the complete automated test suite passes with 460 of 460 tests;
- current public image contracts remain compatible.

Development tooling note:

- `DefaultConnection` may resolve from .NET User Secrets and therefore point to
  Neon;
- local migration validation must explicitly select the intended connection
  when Docker PostgreSQL is the target.

### Representative baseline

A synthetic local dataset contained:

```text
10,000 Games
20,000 GameGenres
20,000 GamePlatforms
10,000 ExternalGameRecords
16,666 GameImages
```

Measured PostgreSQL storage after `ANALYZE`:

```text
table data  ~7.4 MB
indexes     ~11 MB
total       ~19 MB
```

Indexes already occupied more space than table data in this baseline.

### High-cardinality pressure scenario

The same 10,000-game catalogue was preserved while adding:

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

Measured storage after `ANALYZE`:

```text
table data  ~29 MB
indexes     ~33 MB
total       ~63 MB
```

The pressure scenario added approximately 44 MB without increasing the
canonical game count.

Largest observed relations:

| Relation | Rows | Approximate total size | Approximate total bytes per row |
|---|---:|---:|---:|
| `game_keywords` | 80,000 | 14 MB | 181 B |
| `game_releases` | 30,000 | 10.1 MB | 344 B |
| `game_images` | 16,666 | 5.3 MB | 328 B |
| `game_themes` | 30,000 | 5.3 MB | 182 B |
| `game_companies` | 20,000 | 5.3 MB | 271 B |
| `GameGenres` | 20,000 | 3.5 MB | 181 B |
| `GamePlatforms` | 20,000 | 3.5 MB | 181 B |
| `Games` | 10,000 | 3.2 MB | 327 B |

These are budgeting measurements, not fixed production forecasts.

The strongest synthetic pressure points are keywords and contextual releases.

The pressure scenario averaged roughly 6.3 KB of total measured database
objects per game when all synthetic associations were included. That value must
not be extrapolated as a linear production guarantee because real
cardinalities, PostgreSQL page allocation, index behavior, VACUUM, bloat,
future migrations, and source distributions will differ.

### Query-plan evidence

Representative query-plan validation showed:

- name substring search using `ILIKE '%term%'` used a sequential scan and
  completed in approximately 3.7 ms at 10,000 games;
- release-year filtering through `EXTRACT(YEAR FROM FirstReleaseDate)` used a
  sequential scan and completed in approximately 1.8 ms;
- genre filtering used `IX_GameGenres_GenreId`;
- platform filtering used `IX_GamePlatforms_PlatformId`;
- batch cover lookup used `IX_game_images_GameId`.

No new index is justified solely to eliminate the currently inexpensive
sequential scans.

Indexes must be justified by demonstrated access paths and measurable need,
because they are already a material component of the storage budget.

## Validated Scenarios

The decision supports these scenarios without changing its identity
foundation:

1. an initial IGDB record is collected, validated, and linked to a new game;
2. the same IGDB record is re-imported through `IGDB + ExternalId` without a
   duplicate;
3. a future Wikidata record with an official IGDB identifier links to the same
   game;
4. a future record without a strong identifier remains unlinked or becomes a
   reconciliation candidate instead of being merged by name;
5. type conflicts such as base game versus DLC block automatic reconciliation;
6. a source record or association that is not observed in one run remains
   unchanged when the observation is partial or its completeness is unknown;
7. a confirmed complete source observation may synchronize a source-specific
   association out of the current state without deleting the corresponding
   external identity or unrelated evidence;
8. the absence of a Steam record has no effect on canonical game identity;
9. a relevant catalogue may grow beyond the synthetic validation sample
   without introducing an arbitrary fixed game-count cap;
10. high-cardinality metadata may be limited, deferred, or omitted for
    capacity reasons without being treated as a source-domain removal;
11. storage-driven cleanup does not alter reconciliation semantics;
12. a new index is deferred when measured query latency remains acceptable and
    the index would add unnecessary storage/write cost.

## Consequences

### Positive

- adding a source does not add a provider-specific property to `Game`;
- IGDB imports can be idempotent without making IGDB the canonical identifier;
- unmatched and suspicious records can be preserved safely;
- future reconciliation can be introduced without replacing the identity
  foundation;
- source-specific current-state changes can be synchronized without conflating
  absence with confirmed removal;
- provider contracts remain isolated from the domain;
- Steam remains optional;
- catalogue coverage can grow independently from metadata depth;
- storage pressure can be managed at the dominant relationship/table level
  instead of through arbitrary catalogue truncation;
- index decisions can be made from measured query and storage evidence.

### Negative

- the persistence model requires additional source-record concepts,
  provenance-bearing associations, and uniqueness constraints;
- first-source ingestion is more explicit than storing an IGDB ID directly on
  `Game`;
- synchronization and provenance still require explicit rules for complete
  versus partial observations;
- query and import code must distinguish canonical games from external source
  records;
- high-cardinality relationships require explicit capacity monitoring;
- limiting metadata depth requires Collector/product rules instead of blindly
  mirroring the provider;
- representative local storage measurements must be repeated against real
  production data before treating any budget ratio as reliable forecasting.

## Deferred Decisions

The following remain outside this ADR and must not block the first IGDB MVP:

- complete Wikidata and Steam mappings;
- production cross-source reconciliation algorithms;
- producer selection among complete source catalogs;
- final confidence scoring;
- manual reconciliation workflows;
- full field-level temporal observation history;
- historical retention windows for future features that explicitly require
  history;
- unlimited source snapshots;
- exact checksum strategy;
- exact checkpoint cursor and recovery implementation;
- final per-field metadata-depth caps for real IGDB ingestion;
- production capacity forecasting based on representative real IGDB data;
- specialized text-search indexes unless measured production-like latency
  requires them.

## Review Conditions

Review this ADR when:

- a second catalogue source is integrated;
- official cross-source identifiers prove insufficient for the required
  reconciliation cases;
- field-level conflicts require a richer observation model;
- storage constraints require a different evidence-retention strategy;
- real ingestion shows materially different high-cardinality behavior from the
  GMI-30 synthetic measurements;
- metadata-depth controls materially reduce product usefulness;
- source records need many-to-many canonical links rather than the currently
  assumed optional single-game link;
- production-like query latency justifies specialized indexes or a different
  search strategy.
