# Comparable Games Domain Model

> Review status: updated after IGDB PoC approval on August 14, 2026.

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

## Approved source roles

- IGDB: primary catalog.
- Wikidata: reconciliation and enrichment.
- Steam: official Steam-specific evidence.

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

Supporting concepts may include:

- `MatchConfidence`;
- source/evidence separation;
- field- or assertion-level provenance;
- reliability and conflict status;
- temporal `SourceObservation`.

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

## Migration gate

No major migration before:

1. required questions and filters for the current IGDB MVP are fixed;
2. permitted IGDB fields and their nullability are approved by the completed
   PoC;
3. apply the compatibility confirmed by the spike and `ADR-0003` to canonical
   identity, external records, provenance, and source-specific contracts;
4. domain and ingestion architecture are reviewed;
5. the migration is limited to concepts justified by the current iteration.

Complete Wikidata and Steam field mappings are not prerequisites for the first
IGDB persistence increment. They remain future-iteration gates before those
sources are integrated. This preserves the multi-source product direction
without forcing future fields into the current schema.
