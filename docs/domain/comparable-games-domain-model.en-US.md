# Comparable Games Domain Model

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

## Proposed concepts for review

```text
Game
├── Genres
├── Themes
├── GameModes
├── Companies
├── Releases
│   ├── Platform
│   ├── Region
│   ├── ReleaseDate
│   └── ReleaseStatus
└── ExternalReferences
    ├── Source
    ├── ExternalId
    ├── SourceUrl
    ├── ImportedAt
    └── LastVerifiedAt
```

Supporting concepts may include:

- `MatchConfidence`;
- source/evidence separation;
- field- or assertion-level provenance;
- reliability and conflict status;
- temporal `SourceObservation`.

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

1. required producer questions and filters are fixed;
2. permitted fields are mapped for IGDB, Wikidata, and Steam;
3. reconciliation rules are proposed;
4. domain and ingestion architecture are reviewed;
5. a small proof of concept confirms the assumptions.
