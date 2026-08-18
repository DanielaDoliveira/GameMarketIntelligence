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

## Decision Drivers

- preserve provider-independent canonical identity;
- support IGDB now without blocking Wikidata and Steam later;
- keep Steam optional for identity and catalogue admission;
- support idempotent imports through a stable source identity;
- allow unlinked, rejected, inactive, or under-review source records;
- preserve minimum synchronization and provenance evidence;
- avoid automatic reconciliation by name;
- preserve relational integrity where practical;
- avoid implementing the complete Milestone 3 reconciliation system early;
- remain compatible with the zero-cost PostgreSQL operating model.

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

## Provenance Boundaries

Canonical entities and source assertions do not have identical provenance
needs.

- games and queryable taxonomy concepts use internal GMI identity while
  preserving their source identity;
- releases preserve source, platform, region, precision, and status because
  dates may differ by context and source;
- product relationships preserve the declaring source and do not merge or
  propagate data automatically;
- images preserve source image identifiers, attribution, synchronization
  metadata, and URLs, but binary storage remains deferred;
- cross-source identifiers and websites remain evidence until validated for
  the intended use;
- source absence marks a source record missing or inactive and does not cause
  immediate physical deletion of the canonical game.
- source-specific retention or termination rules may require deleting that
  source's contributions; provenance must make those values locatable and
  allow an authorized fallback to be materialized without changing `Game.Id`.

`DataSource` also carries operational metadata needed by the integration, such
as public attribution, official links, status, and retention/removal rules.
Every integration must use authorized endpoints and honor its own license and
terms. SteamDB remains a manual research reference and cannot feed collection,
persistence, or public API responses.

The implementation may use entity-specific external-identity mappings when
needed to retain foreign-key integrity. A single polymorphic table with an
unconstrained `EntityType + EntityId` pair is not required by this decision.

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
6. a disappeared source record becomes missing or inactive without immediately
   deleting the canonical game;
7. the absence of a Steam record has no effect on canonical game identity.

## Consequences

### Positive

- adding a source does not add a provider-specific property to `Game`;
- IGDB imports can be idempotent without making IGDB the canonical identifier;
- unmatched and suspicious records can be preserved safely;
- future reconciliation can be introduced without replacing the identity
  foundation;
- source disappearance and reactivation can be represented;
- provider contracts remain isolated from the domain;
- Steam remains optional.

### Negative

- the persistence model requires an additional source-record concept and
  uniqueness constraint;
- first-source ingestion is more explicit than storing an IGDB ID directly on
  `Game`;
- synchronization status and minimum provenance metadata require lifecycle
  rules;
- query and import code must distinguish canonical games from external source
  records.

## Deferred Decisions

The following remain outside this ADR and must not block the first IGDB MVP:

- complete Wikidata and Steam mappings;
- production cross-source reconciliation algorithms;
- producer selection among complete source catalogs;
- final confidence scoring;
- manual reconciliation workflows;
- full field-level temporal observation history;
- unlimited source snapshots;
- exact checksum strategy;
- exact checkpoint cursor and recovery implementation;
- all entity and table names used by the final persistence model.

## Review Conditions

Review this ADR when:

- a second catalogue source is integrated;
- official cross-source identifiers prove insufficient for the required
  reconciliation cases;
- field-level conflicts require a richer observation model;
- storage constraints require a different evidence-retention strategy;
- source records need many-to-many canonical links rather than the currently
  assumed optional single-game link.
