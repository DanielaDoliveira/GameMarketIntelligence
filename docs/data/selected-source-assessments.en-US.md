# Game Market Intelligence — Selected Source Assessments

> Language: English (United States)  
> Status: living document  
> Reviewed on: August 14, 2026

## Purpose

This document records the detailed assessment of the sources selected for the **Game Market Intelligence (GMI)** MVP.

It complements the concise source-selection document and focuses on the product, data, legal, architecture, and engineering decisions for the three approved sources:

| Source | Classification | Status |
|---|---|---|
| **IGDB** | Primary general source | Selected; PoC completed and approved for the first source iteration |
| **Wikidata** | Reconciliation and enrichment source | Selected; detailed PoC deferred to a future iteration |
| **Steam** | Official specialized source | Selected; detailed PoC deferred to a future iteration |

Sources that were not selected remain documented in the comparative benchmark.

---

# 1. IGDB — Primary general source

## Proposed role

- primary general catalog;
- foundation for Comparable Games filters;
- external-identifier source;
- input for normalization and reconciliation;
- structured source for human use and future agent use through GMI contracts.

## Producer value

IGDB supports both initial discovery and deeper refinement.

Questions it can help answer include:

- Which games share genre, platform, and release period?
- Which games share themes, modes, or player perspectives?
- Which companies developed or published similar titles?
- Which releases belong to a particular platform or region?
- Is the result a main game, port, remake, remaster, expansion, or DLC?
- Which external references can support later validation?

| Dimension | Assessment |
|---|---|
| Initial discovery | Very high |
| Advanced refinement | Very high |
| Company relationships | High |
| Release modeling | High |
| External IDs and reconciliation | High |
| Commercial metrics | Low for the current MVP |
| Future agent usefulness | Very high |

## Relevant data categories

The potentially useful subset includes:

- games;
- genres;
- themes;
- game modes;
- player perspectives;
- platforms;
- release dates;
- regions;
- involved companies;
- developers and publishers;
- keywords;
- multiplayer modes;
- websites and external references;
- game types and relationships;
- creation and update timestamps;
- checksums.

GMI must not import the entire IGDB schema. The final subset must be driven by approved filters and domain needs.

## Reliability by category

| Category | Provisional reliability | Notes |
|---|---|---|
| Game identity and name | High | Editions and duplicates still require reconciliation |
| Platforms | Medium-high | Official sources may validate platform-specific facts |
| Release dates | Medium-high | Strong structure by platform, region, and precision |
| Developer and publisher | Medium-high | Structured relationships without assuming universal official status |
| Genres | Medium | Curated classification |
| Themes | Medium | Useful but interpretive |
| Game modes | Medium-high | More objective, but still curated |
| Player perspective | Medium | Interpretive classification |
| Keywords | Medium-low | Potentially noisy |
| Ratings and popularity | Contextual | Signals, not proof of commercial performance |
| Images | Pending | Rights and storage require a separate review |

## Data nature

IGDB should be represented as a structured and curated database.

It will not be treated as the official source for every game attribute. Official validation may come from publishers, developers, platforms, or official product pages.

## Cost and use

IGDB was approved for the current non-commercial, zero-cost scenario, subject to the applicable terms and attribution requirements.

Any future monetization or commercial use will require reassessment.

## Storage and serving model

```text
IGDB
  ↓
Scheduled worker
  ↓
Validation and normalization
  ↓
PostgreSQL
  ↓
GMI API
  ↓
Blazor and future read-only integrations
```

GMI should store normalized data and expose its own contracts rather than operating as an IGDB mirror.

## Attribution

The implementation should preserve:

- visible IGDB identification on Data Sources;
- source links;
- provenance for relevant data;
- static attribution where required;
- collection and verification dates.

Final wording and placement must follow the applicable agreement and brand guidance.

## Technical feasibility

### Authentication

Authentication uses application credentials and OAuth tokens tied to the Twitch ecosystem.

Secrets must remain in backend or worker configuration and must never be exposed to Blazor WebAssembly.

### Limits and collection

The worker should operate conservatively with:

- controlled frequency;
- limited concurrency;
- `429` handling;
- delayed retries;
- checkpoints;
- idempotency;
- incremental updates.

### Incremental updates

Timestamps and checksums can help identify changed records.

The first implementation should prefer scheduled collection and avoid adding webhooks to the MVP scope.

### Isolation

IGDB-specific syntax and contracts should remain inside an Infrastructure adapter.

Application, Domain, and public contracts must remain source-independent.

## Architecture implications

Avoid:

- IGDB theme IDs directly on `Game`;
- external enums in public contracts;
- IGDB request objects inside Application;
- external credentials in frontend code.

Prefer:

- normalized concepts;
- explicit external references;
- provenance;
- adapters;
- mapping and reconciliation rules;
- stable GMI contracts.

## Agent use

GMI must add the semantics required for safe agent consumption:

- source identity;
- data nature;
- reliability;
- original URL;
- verification date;
- conflict warnings;
- coverage limitations;
- explicit candidate-comparable interpretation.

## Risks and open questions

1. exact downstream public API boundaries;
2. final attribution requirements;
3. image and description rights;
4. duplicate and edition handling;
5. regional release conflicts;
6. future changes to free-access conditions.

## Decision

**Selected as the MVP primary general source.**

No migration should be created merely to reproduce the IGDB schema.

---

# 2. Wikidata — Reconciliation and enrichment

## Proposed role

- reconcile records from different sources;
- provide external IDs;
- retrieve aliases and multilingual names;
- enrich relationships among games, franchises, companies, and entities;
- support deduplication and references;
- provide an open layer for human-readable and machine-readable contracts.

## Producer value

Wikidata does not replace IGDB as the primary catalog.

Its value lies in connecting and explaining entities:

- identifying that two records represent the same game;
- relating alternate names;
- connecting games to franchises;
- linking developers, publishers, and companies;
- preserving external references;
- supporting multilingual search;
- enabling navigation among entities.

| Dimension | Assessment |
|---|---|
| General catalog | Medium and uneven |
| Reconciliation | Very high |
| External IDs | Very high |
| Alternate names | High |
| Entity relationships | High |
| Smaller-game coverage | Variable |
| Agent compatibility | Very high |

## Relevant data categories

- external identifiers;
- names and aliases;
- multilingual labels;
- franchises;
- developers;
- publishers;
- companies;
- platforms;
- relationships among works and versions;
- URLs and references;
- qualifiers and ranks;
- sources attached to statements.

## Reliability by category

| Category | Provisional reliability | Notes |
|---|---|---|
| External IDs | High | Highly useful for reconciliation |
| Primary name | High when consistent | May vary by language or item |
| Aliases | Medium-high | Useful for search and matching |
| Franchise relationships | Medium-high | Depend on item quality |
| Related companies | Medium | Requires statement and qualifier review |
| Platforms and dates | Variable | Should not replace primary sources without validation |
| Referenced statements | Higher | Prefer statements with sources |
| Unreferenced statements | Lower | Should carry reduced confidence |

## Data nature

Each statement may contain:

- value;
- reference;
- qualifier;
- rank;
- edit history.

Reliability should therefore follow the statement rather than the entire item.

## License and cost

Wikidata structured data is released under **CC0**.

This provides strong compatibility with:

- storage;
- transformation;
- normalization;
- structured redistribution;
- GMI-owned contracts;
- tools and agent use.

Responsible use and public-service operational limits still apply.

## Ingestion model

The recommended strategy is targeted enrichment:

```text
Already identified game
  ↓
Lookup by name or external ID
  ↓
Matching and validation
  ↓
Selective import of IDs, aliases, and relationships
  ↓
Provenance recording
```

Not recommended:

- depending on public SPARQL at request time;
- importing the full dump for the MVP;
- using Wikidata as the only identity source;
- accepting automatic matches without confidence controls.

## Reconciliation

The process should consider:

- names;
- aliases;
- platforms;
- dates;
- developers;
- publishers;
- external IDs;
- product type;
- relationships among originals, remakes, remasters, DLCs, and ports.

A match should carry confidence and sufficient evidence before records are merged.

## Technical feasibility

- targeted SPARQL queries;
- open endpoints and dumps;
- stable IDs;
- RDF and structured JSON;
- worker-friendly integration;
- compatibility with incremental processing.

The worker should limit requests, use caching, and avoid unnecessary pressure on the public service.

## Architecture implications

Wikidata reinforces the need for concepts such as:

```text
ExternalReference
├── Source
├── ExternalId
├── SourceUrl
├── ImportedAt
├── LastVerifiedAt
└── MatchConfidence
```

It also supports separating:

- the source that supplied a value;
- the evidence that confirmed it;
- the observation captured at a point in time.

## Agent use

Wikidata provides strong value for agents because it offers:

- persistent IDs;
- semantic relationships;
- aliases;
- references;
- machine-readable structure;
- an open license.

GMI should still expose reconciled and contextualized data rather than raw SPARQL responses.

## Risks and open questions

1. uneven coverage;
2. unreferenced statements;
3. incorrect matching across editions;
4. public endpoint availability and limits;
5. uneven quality across languages and items;
6. manual review requirements for ambiguous cases.

## Decision

**Selected as the MVP reconciliation and enrichment source.**

It complements IGDB and does not compete for the primary general-catalog role.

---

# 3. Steam — Official specialized source

## Proposed role

- confirm official Steam presence;
- preserve Steam App IDs;
- provide official product pages;
- validate platform-specific information;
- potentially record authorized temporal observations;
- provide official evidence complementary to the general catalog.

## Producer value

Steam can support questions such as:

- Is the game officially available on Steam?
- What is its App ID?
- What is its official product page?
- Which operating systems or features are listed?
- What was the observed availability state?
- Which aggregate reviews exist in the Steam ecosystem?
- What was the price in a specific region and at a specific time, when permitted?

| Dimension | Assessment |
|---|---|
| Authority within Steam | Very high |
| Platform identity | Very high |
| Official product page | Very high |
| General market coverage | Low |
| Absolute commercial metrics | Not available |
| Temporal data value | Potentially high, with context |
| Agent value | High, with explicit boundaries |

## Relevant data categories

- Steam App ID;
- product name and type;
- official product page;
- platform presence;
- store information;
- displayed developer and publisher;
- Steam release;
- supported operating systems;
- categories and features;
- aggregate reviews;
- price and availability, when authorized;
- DLC and package relationships, when applicable.

## Reliability by category

| Category | Provisional reliability | Notes |
|---|---|---|
| Steam App ID | Very high | Official identifier |
| Steam presence | Very high | Official fact |
| Product page | Very high | Official reference |
| Name and type | High | Must distinguish games, DLCs, demos, bundles, and tools |
| Steam release | High | Does not necessarily represent the first worldwide release |
| Displayed developer and publisher | High | May reflect the store listing's commercial name |
| Supported platforms | High within Steam | Limited to the ecosystem |
| Aggregate reviews | High as a Steam metric | Do not represent sales |
| Price | Very high for the observed region and time | Temporal data |
| Sales and revenue | Unavailable | Must not be inferred |

## Data nature

Steam is official for facts about its own ecosystem.

This does not make those facts universal:

- Steam release date is not necessarily the original date;
- Steam reviews are not global satisfaction;
- concurrent players do not equal sales;
- rankings do not reveal units or revenue.

## Access scope

Assessment must remain endpoint-specific.

The project must distinguish:

- public APIs;
- user-key methods;
- publisher-only methods;
- Steamworks functionality;
- store data;
- undocumented endpoints.

GMI will only use documented mechanisms compatible with the intended use.

## Temporal data

Price, availability, reviews, and rankings should be modeled as observations:

```text
SourceObservation
├── GameId
├── Source
├── Metric
├── Value
├── Region
├── Currency
├── ObservedAt
└── Method
```

They should not become static and universal `Game` properties.

## Ingestion model

```text
Steam
  ↓
Specialized worker
  ↓
App ID and product-type validation
  ↓
Normalization
  ↓
Official reference and permitted observations
  ↓
PostgreSQL
```

The worker must respect limits, caching, frequency, and specific permissions.

## Relationship with SteamDB

SteamDB is an independent tool and will not be ingested.

When data can be obtained officially from Steam, Steam is the correct source.

GMI must not:

- scrape SteamDB;
- copy its historical datasets;
- use internal endpoints;
- reproduce its database;
- present SteamDB as official.

## Attribution and branding

Presentation must respect:

- Steam identity;
- branding rules;
- disclaimers;
- official links;
- attribution requirements;
- limits on image and asset use.

## Architecture implications

Steam should remain behind its own adapter.

Avoid:

- making App ID the internal game identity;
- mixing Steam availability with global availability;
- storing price without region and date;
- treating reviews as sales;
- exposing publisher methods or credentials.

## Agent use

Agent-facing contracts must clearly communicate:

- that the fact applies to Steam;
- when it was verified;
- region and observation time;
- that availability can change;
- that reviews and concurrency are not sales;
- that the supplied page is an official platform reference.

## Risks and open questions

1. final endpoint review;
2. storage and exposure permissions;
3. branding and attribution rules;
4. image and description rights;
5. regional differences;
6. temporal-data stability;
7. prevention of improper commercial inference.

## Decision

**Selected as the official specialized source for the Steam ecosystem.**

It complements, but does not replace, IGDB and Wikidata.

---

# Combined responsibilities

| Need | IGDB | Wikidata | Steam |
|---|---|---|---|
| General catalog | Primary | Complementary and uneven | Steam only |
| Comparable Games filters | Primary | Limited | Limited |
| External IDs | Strong | Very strong | Steam App ID |
| Reconciliation | Good | Primary | App ID support |
| Entity relationships | Strong | Strong | Ecosystem-limited |
| Official confirmation | Not universal | Not universal | Official for Steam |
| Official links | External references | Open references | Official Steam page |
| Temporal data | Limited | Not a priority | Possible with context |
| Open license | No | Yes, CC0 | No |
| MVP role | Catalog | Reconciliation | Official specialized validation |

---

# Shared implementation rules

1. Sources will be consumed by controlled workers.
2. The frontend will not call external sources directly.
3. Credentials will remain in backend or worker infrastructure.
4. The domain will use GMI-owned concepts.
5. Provenance will be preserved per value or statement.
6. Conflicts will not be hidden.
7. Temporal data will be modeled as observations.
8. Images and descriptions require a separate review for each source; the IGDB
   image review is complete for the first MVP, while future sources retain this
   gate.
9. The GMI API will not operate as a source mirror.
10. Future agent access will initially be read-only.
11. No significant migration will be created before field definition is
    finalized for the source and iteration that justify it.
12. Changes in terms, cost, or scope will trigger reassessment.

---

# Progression status

The first-source IGDB iteration has defined its product questions, field scope,
nullability, provenance constraints, operational behavior, and PoC approval.
The immediate next stage is the lightweight multi-source compatibility spike,
followed by the clean definitive Collector and IGDB persistence review.

Detailed Wikidata and Steam questions, mappings, permissions, and proofs of
concept remain future-iteration work close to their respective integrations.
This preserves the complete multi-source vision without blocking the current
IGDB MVP or prematurely forcing future-source fields into its model.
