# Game Market Intelligence — Data, Reconciliation, and PoC Decisions

## 1. Purpose

This document consolidates the decisions that guide the **Game Market Intelligence (GMI)** MVP after the IGDB proof of concept and subsequent persistence-model refinements.

GMI is not intended to store every possible piece of game data. Its purpose is to select, organize, and present only what genuinely helps producers with early market research, comparable-game discovery, niche exploration, and competitive-context analysis.

> GMI creates value through a focused set of useful data, not through excessive information that makes analysis harder.

## 2. MVP scope

The MVP will provide:

- search by name and aliases;
- exploration through keywords;
- filters for genre, theme, platform, game mode, perspective, multiplayer, and release period;
- context about involved companies;
- franchises and collections/series;
- product relationships such as remake, remaster, port, edition, DLC, and expansion;
- provenance and confidence levels;
- transparency about limitations, conflicts, and coverage.

Out of scope:

- financial metrics, sales, and revenue;
- success prediction and automatic opportunity scoring;
- a custom subgenre taxonomy;
- separate modeling of IP, subfranchise, universe, editorial line, brand, licensed property, or corporate group;
- deep multiplayer analysis;
- complex corporate history;
- detailed edition-content comparison;
- unlimited archival storage of old observations.

Financial metrics may be considered after the MVP is complete.

## 3. Selected sources

### IGDB

IGDB will be the main catalog source and canonical MVP taxonomy for genres, themes, modes, perspectives, keywords, product types, relationships, platforms, release dates, companies, franchises, collections, and external identifiers.

### Wikidata

Wikidata will support reconciliation, enrichment, auxiliary validation, and cross-source identifiers. It will not automatically replace IGDB as the canonical taxonomy.

### Steam

Steam will be a specialized source for Steam-specific facts, such as Steam release dates and product identity. The architecture and reconciliation process must not depend on Steam.

## 4. IGDB mapping

### 4.1 Identity and discovery

**Include:** `id`, `name`, `alternative_names`, `game_type`, `version_parent`, `game_status`, `summary`.

**Defer or exclude:** defer `slug`; do not initially import `storyline`.

Rules:

- external identity = `Source + ExternalId`;
- name alone never supports automatic reconciliation;
- `game_status` is contextual;
- `summary` supports details, not identity.

### 4.2 Platforms and releases

**Include:** `platforms`, `release_dates.platform`, `date`, `date_format`, `release_region`, `status`, `updated_at`.

**Derive:** `first_release_date`, only as a convenience.

**Defer:** `platform_version_release_dates`.

Rules:

- preserve dates by platform and region;
- preserve original precision;
- never convert year-only or month-only data into artificial dates;
- support date-range queries;
- different dates on different platforms are not conflicts.

### 4.3 Involved companies

**Include roles:** developer, publisher, porting, and supporting.

**Include minimum company data:** external identifier, name, status when available, and `updated_at`.

**Defer:** `changed_company_id`, websites, parent company, corporate history, and long descriptions.

Rules:

- companies are multivalued relationships;
- role is mandatory;
- absence in one source is not a conflict;
- different roles may be complementary;
- porting and supporting do not replace developer or publisher.

### 4.4 Product types and relationships

**Include:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Defer:** `expanded_games`, detailed `game_versions`, `forks`, and `similar_games`.

> Related products remain separate records.

### 4.5 External identifiers and websites

**Include from `external_games`:** `external_game_source`, `uid`, `url`, `platform`, `name`, `year`, `updated_at`.

**Include from `websites`:** `type`, `url`.

**Defer:** countries, release format, and website-specific checksums.

Rules:

- `Source + UID` identifies an external record;
- URLs must be validated;
- `trusted` is only an auxiliary signal.

### 4.6 Filters and taxonomy

- **Genres:** multiple IDs use AND; extra genres are allowed.
- **Themes:** multiple values use AND; extra themes are allowed.
- **Game modes:** multiple values use AND.
- **Perspectives:** multiple values use AND.
- **Keywords:** multiple values use AND; keywords are central to GMI's value; use structured IDs; do not create custom keywords or automatically merge terms.
- **Platforms:** multiple values use OR.
- **Multiplayer:** include multiplayer, online co-op, and local/offline multiplayer; selected capabilities use AND without exclusivity.

Out of scope for multiplayer: maximum player count, LAN, drop-in/drop-out, and detailed platform-specific configurations.

### 4.7 Franchises and collections

**Include:** franchises, collections/series, identifiers, names, and technical synchronization metadata.

**Do not model separately:** IP, subfranchise, universe, editorial line, brand, licensed property, or corporate group.

Simplified structure:

```text
Franchise
└── Collection / Series
    └── Game
```

Franchises and collections may become advanced filters.

## 5. Search and user experience

GMI will distinguish two intentions:

### Find a known game

- name;
- aliases;
- refinement by platform, period, and type.

### Explore an idea or niche

- one or multiple keywords;
- refinement through genres, themes, modes, perspectives, platforms, and release period.

Name-only search may show related versions in an expandable group.

```text
Mario Kart 8
└── 1 related version
    └── Mario Kart 8 Deluxe
```

## 6. Cross-source reconciliation

### 6.1 Identity

- each game has a GMI `Game.Id`;
- each external identity is `Source + ExternalId`.

### 6.2 Automatic match

Allowed only with:

- an exact cross-source external ID;
- compatible product type;
- no unresolved severe conflict.

### 6.3 Probable match

Without a strong ID, use composite signals: normalized name, aliases, type, companies, platforms, release period, franchise, collection, and declared relationships.

Composite matches create candidates, not automatic merges.

### 6.4 Distinct but related products

Remakes, remasters, ports, editions, DLCs, expansions, bundles, demos, soundtracks, and tools remain separate.

When the relationship is clear but the exact type conflicts, use:

```text
RelatedVersionOf
```

Severe conflicts such as base game versus DLC, demo, soundtrack, or tool block automatic reconciliation.

### 6.5 Simple base-name-plus-suffix filter

The worker may detect:

```text
Mario Kart 8
Mario Kart 8 Deluxe
```

When one complete normalized title is followed by extra content after a valid boundary:

- the products remain distinct;
- they may become relationship candidates;
- the worker does not need to understand the suffix;
- the rule never confirms remake, remaster, port, or edition by itself.

The PoC will evaluate feasibility and false positives.

## 7. Progressive validation

Flow:

```text
External source
→ cheap validation
→ intermediate layer
→ identity and business validation
→ canonical model
```

### 7.1 At the boundary

Apply valid JSON, minimum fields, basic types, non-empty name, external identifier, simple normalization, `updated_at`, checksum, and rejection of obviously invalid data.

### 7.2 Intermediate layer

Persist only:

- reconciliation candidates;
- real conflicts;
- relationship candidates;
- recoverable invalid records;
- under-review observations;
- manual decisions;
- minimum required evidence.

Do not permanently persist complete raw payloads, repeated snapshots, fully processed API responses, detailed logs for normal cases, or indefinite history of missing/inactive observations.

### 7.3 Canonical model

Accept only data that satisfies identity, type compatibility, relationship consistency, provenance, contextual precedence, and no unresolved severe conflict.

## 8. Conflict handling

### 8.1 Release dates

Treat as a conflict only for the same product, platform, region, release type/status, and precision.

For a true conflict:

- prefer the most appropriate and reliable source for that platform;
- preserve the divergent observation only when it remains useful for reconciliation, audit, or an active product decision;
- display platform with the date;
- show ecosystem context when useful, such as `PC — 2020-08-07 (Steam)`.

### 8.2 Companies

- treat companies as multivalued relationships;
- combine complementary information;
- consider product, version, platform, and role;
- do not merge companies by name alone;
- preserve relevant conflicts when they still require reconciliation or user-facing explanation.

### 8.3 Classifications

IGDB is canonical for genres, themes, modes, perspectives, and keywords.

External classifications remain preserved with provenance when materialized, but are not automatically merged and do not alter canonical filters.

### 8.4 Product types and relationships

- keep products separate;
- preserve original classifications where required by active provenance;
- use a generic relationship when the connection is clear but the specific type conflicts;
- block automatic reconciliation for basic-nature conflicts.

## 9. Confidence policy

Confidence is contextual per data point, not assigned to the whole game.

### High confidence

Highly appropriate source, complete context, no relevant conflict, strong evidence or confirmation, and recent precise data.

### Balanced

Reliable source, useful but partial data, limited precision, no additional confirmation, or a minor context limitation.

### Broad coverage

Wider coverage, less appropriate source, relevant conflict or ambiguity, incomplete context, or still-probable correspondence.

The MVP will not use a complex numeric formula.

## 10. Missing data, synchronization, and retention

### 10.1 Absence does not prove removal

A value or association that does not appear in one collection run is not automatically considered removed.

Possible causes include:

- partial source responses;
- a field not requested in that run;
- pagination or transient failures;
- source/API behavior that does not guarantee a complete current snapshot.

Therefore:

```text
not observed in the current run
≠
confirmed removed by the source
```

The Collector must preserve the current accepted state when the observation is incomplete or its completeness is unknown.

### 10.2 Synchronizing a confirmed current-state change

An existing source contribution may be synchronized out of the current state only when the Collector knows that:

- the relevant field or relationship set was explicitly requested;
- the source response for that scope completed successfully;
- the source semantics indicate that the returned set represents the current complete state, or the source explicitly reports removal;
- no independent evidence from another source is being erased by that source-specific change.

A confirmed removal of an association does not delete the corresponding `External*Record`.

For example:

```text
GameTheme removed from IGDB contribution
→ remove/synchronize that IGDB-supported association

ExternalThemeRecord
→ remains a valid identity for the source concept
```

Source-specific changes must not be propagated blindly to evidence from other sources.

### 10.3 Current-state storage policy

The operational database is designed to store:

```text
treated current canonical state
+
active external identities
+
minimum provenance required to explain and recompute that state
```

It is not intended to be an indefinite history warehouse.

Detailed observation history, repeated snapshots, or per-cycle inactive copies must not be introduced unless a concrete product or operational need justifies them.

If a future feature requires historical data, its retention window must be defined when the feature is introduced.

### 10.4 Capacity-driven retention

Storage retention and source reconciliation are separate concerns.

Deleting data because the database is approaching its capacity limit must never be interpreted as evidence that a source removed or changed a fact.

The current Neon Free planning limit is approximately 0.5 GB per project.

Operational thresholds are:

```text
< 70%
→ normal operation

70%+
→ investigate growth by table and index

before 80%
→ execute controlled retention or capacity action

approaching 90%
→ protect essential writes and reduce non-essential ingestion
```

Retention priority is:

1. current canonical state;
2. active external identities;
3. provenance still supporting the current state;
4. recent useful historical or auxiliary data, when such data exists.

When historical or auxiliary data is explicitly eligible for pruning, remove the oldest eligible data first while preserving the most recent useful window.

Capacity decisions must be based on occupied bytes and measured table/index growth rather than a fixed record count.

## 11. Incremental updates and cost

The PoC will compare:

```text
updated_at
versus
updated_at + checksum
```

Goals:

- avoid reprocessing;
- reduce writes;
- reduce worker execution time;
- control infrastructure usage;
- ensure idempotency.

Checksum is technical and will not be part of the user experience.

## 12. PoC approval criteria

The PoC must demonstrate:

### Coverage

- sufficient coverage for essential fields;
- identifiable and explainable limitations;
- no dependency on unreliable fields.

### Keywords

- useful volume;
- relevant results;
- AND combinations;
- feasible autocomplete;
- understandable duplicates and aliases;
- real value for niche exploration.

### Filters

- AND for genres, themes, modes, perspectives, keywords, and multiplayer;
- OR for platforms;
- AND across categories;
- name search;
- release-period search.

### Release dates

- preservation of platform, region, and precision;
- multiple releases;
- date-range queries;
- correct handling of platform-specific differences.

### Relationships

- sufficient coverage for DLC, expansion, port, remake, remaster, bundle, and versions;
- prevention of incorrect merges;
- evaluation of the base-name-plus-suffix rule.

### Reconciliation

- exact cross-source IDs;
- type validation;
- composite candidates;
- rejected candidates not recreated repeatedly;
- related products kept separate.

### Worker

- authentication;
- pagination;
- rate-limit compliance;
- resume after failure;
- idempotency;
- incremental updates;
- checksum evaluation;
- reduced reprocessing;
- explicit distinction between complete and partial observations before synchronizing existing associations out of the current state.

### Validation layers

- normal cases proceed without full intermediate persistence;
- ambiguous cases retain only minimum evidence;
- the canonical model receives approved data only.

### Storage

- growth compatible with the Neon free tier;
- measured growth by table and index;
- controllable retention for any explicitly historical or auxiliary data;
- sustainable indexes;
- no unnecessary raw payload retention;
- preservation of current canonical state and active provenance under storage pressure.

### Final criterion

> IGDB and the pipeline are sufficient for the MVP when they produce useful searches, preserve context and provenance, avoid dangerous merges, operate incrementally and idempotently, and remain compatible with free infrastructure.

## 13. Next step

The current persistence model through GMI-29 is implemented. The next persistence step is GMI-30, which will validate storage behavior and capacity with representative data before production-oriented Collector ingestion is finalized.
