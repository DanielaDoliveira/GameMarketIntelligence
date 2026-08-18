# Comparable Games Domain Model

> Review status: updated by GMI-13 after IGDB PoC approval and the lightweight
> multi-source compatibility spike on August 18, 2026.

## Purpose

Describe the implemented foundation and the approved direction after external-source selection. The domain must answer validated producer questions without copying upstream API schemas.

## Current implementation

- `Game`, `Genre`, `Platform`, `DataSource`, and `SourceReliability`;
- many-to-many game/genre and game/platform relationships;
- normalized-name invariants and uniqueness;
- PostgreSQL persistence and EF Core configuration;
- partial-name, genre, platform, and release-year search;
- AND semantics across categories;
- alphabetical pagination;
- game details, genre, and platform read endpoints;
- standardized validation and error responses;
- responsive Blazor read experience.

## Current `Game`

| Property | Required | Meaning |
|---|---:|---|
| `Id` | Yes | Internal identity |
| `Name` | Yes | Display name |
| `Description` | No | Short context |
| `ReleaseDate` | No | Simplified known release date |
| `ImageUrl` | No | Optional external reference |

The current release date is intentionally simplified and does not represent all platform, region, Early Access, port, remake, or remaster events.

## Canonical database and approved source roles

The database represents GMI's own canonical dataset, not a permanent copy of
IGDB. IGDB is the only active source for the first MVP and provides its initial
taxonomy and values. Future increments may materialize selected values from
multiple sources without retaining complete or interchangeable copies of every
catalog.

- IGDB: base catalog source and general fallback when no more appropriate
  contextual source is available;
- Wikidata: gap filling, enrichment, cross-identifiers, and inconsistency
  detection;
- Steam: specialized and preferred only for facts about the Steam ecosystem;
- future official sources: preferred only for fields and contexts where they
  have authority and authorized access.

There is no absolute provider precedence. Selection is field- and
context-specific. One game may, for example, use an IGDB cover and genres,
Wikidata identifiers, an IGDB Switch release, and a Steam-sourced Steam release.

Provider identifiers must not become permanent source-specific properties on `Game`.

## Approved architectural direction

The lightweight multi-source compatibility spike confirmed the separation
between an external source record and a canonical entity. The complete decision
is recorded in
[`ADR-0003`](../architecture/adr/ADR-0003-external-source-identity-and-provenance.md).
It describes an extensible direction, not a requirement to deliver every future
source or analytical capability in the first IGDB MVP.

```text
DataSource
└── ExternalGameRecord
    ├── ExternalId
    ├── optional GameId
    ├── SourceUpdatedAt
    ├── LastSeenAt
    └── ProcessingStatus

Game
├── zero or more linked ExternalGameRecords
├── Genres
├── Themes
├── GameModes
├── Companies
└── contextual releases with provenance
```

Future supporting concepts are introduced only when justified by a concrete
need and may include:

- `MatchConfidence`;
- source/evidence separation;
- field- or assertion-level provenance;
- reliability and conflict status;
- temporal `SourceObservation`.

GMI stores the selected canonical value and the minimum evidence required to
explain its origin. Retaining every competing observation or allowing producers
to switch the displayed catalog by source is not a requirement.

## Minimum provenance

`DataSource` describes the integration and its operational obligations,
including a stable code, public name, official URL, attribution, status, and
retention rules. Value origin does not belong only to `DataSource`: it must be
associated with the field or contextual entity that received the contribution.

- releases preserve source, platform, ecosystem, region, precision, and status;
- images preserve source identifiers needed to construct a URL, without storing
  the binary;
- localized names, websites, external identifiers, and relationships preserve
  their source when materialized;
- simple fields receive compact provenance only when required for attribution,
  audit, removal, or recomposition;
- the implementation does not anticipate a heavy generic polymorphic table for
  every field.

Provenance is also an operational rule. If a source must be disabled or its data
removed, the system must locate its contributions, remove them, and, when
possible, recompose the value from an allowed base or fallback source without
changing the canonical game identity.

## Storage strategy

The design accounts for the production database limit below 1 GB:

- do not store complete raw payloads, dumps, HTML, or routine snapshots;
- do not mirror complete auxiliary catalogs;
- do not store image binaries;
- query Wikidata and Steam selectively for known games;
- persist only accepted values and required external identity, synchronization,
  and provenance;
- initially create only constraints and indexes supported by real queries;
- measure high-cardinality tables such as releases, images, keywords, and
  many-to-many associations with representative data before expanding intake.

## Product and API impact

GMI provides blended, auditable canonical data, not selectable alternative
catalogs. In addition to game details, the API must eventually expose a
provenance summary mapping fields or contexts to contributing sources without
duplicating the values.

`Data Sources` gains two responsibilities:

1. show the source of relevant fields and associations for the selected game;
2. explain each integration's role, limitations, attribution, and links.

Official, curated, or community nature remains visible evidence, but is not a
selector that replaces the whole displayed dataset.

## GMI-13 gap analysis

| Current state | Approved need | Implementation direction |
|---|---|---|
| `DataSource` is not linked to an imported game | external identity and idempotency | introduce an external record linked to the source and optionally to `Game` |
| `Game` has one simplified `ReleaseDate` | contextual releases | model releases by platform, region, precision, status, and source |
| `Game.ImageUrl` is a direct URL | auditable, storage-efficient images | persist source metadata and IDs and construct display URLs |
| provenance is not attached to values | attribution, removal, and recomposition | source on contextual entities and compact support for simple fields |
| Data Sources is institutional | selected-game audit | add a field/context → source map while retaining the institutional section |
| future vision allowed provider selection | selective retention cannot support switching | remove source-catalog selection |
| the model starts with IGDB | selective future composition | scope IGDB to the first MVP and the base/fallback role |

Approved identity rules:

- `Game.Id` identifies the canonical GMI game;
- `DataSource + ExternalId` safely identifies one record within a source;
- an external record may remain unlinked to `Game` until sufficient evidence
  exists;
- normalized name supports search and candidate generation but does not
  authorize automatic reconciliation;
- provider IDs are not provider-specific properties on `Game`.

## Modeling rules

- producer needs come before source schemas;
- source identity and provenance must remain inspectable;
- conflicts must not be silently erased;
- game base, DLC, bundle, remake, remaster, and port must not be merged blindly;
- prices, reviews, rankings, player counts, and other changing values are observations;
- images and descriptions require separate rights review;
- raw payloads, HTML, and image binaries should not be stored by default.
- integrations must honor source-specific licensing, authorized endpoints,
  attribution, retention, and removal requirements;
- SteamDB remains a manual reference and never feeds a Worker, persistence, or
  the public API.

## Migration gate

No major migration before:

1. required questions and filters for the current IGDB MVP are fixed;
2. permitted IGDB fields and their nullability are approved by the completed
   PoC;
3. apply the compatibility confirmed by the spike and `ADR-0003` to canonical
   identity, external records, provenance, and source-specific contracts;
4. domain and ingestion architecture are reviewed;
5. the migration is limited to concepts justified by the current iteration.

The first migration does not need production reconciliation, competing
observations, user source selection, or generic field history. It must,
however, avoid a structure that prevents provenance, removal, and recomposition
when the second source is integrated.

Complete Wikidata and Steam field mappings are not prerequisites for the first
IGDB persistence increment. They remain future-iteration gates before those
sources are integrated. This preserves the multi-source product direction
without forcing future fields into the current schema.
