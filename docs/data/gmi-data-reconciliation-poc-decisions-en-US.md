# Game Market Intelligence — Data, Reconciliation, and PoC Decisions

## 1. Purpose

This document consolidates the decisions that will guide the **Game Market Intelligence (GMI)** MVP before the IGDB proof of concept.

GMI is not intended to store every possible piece of game data. Its purpose is to select, organize, and present only what genuinely helps producers with early market research, comparable-game discovery, niche exploration, and competitive-context analysis.

> GMI creates value through a focused set of useful data, not through excessive information that makes analysis harder.

## 2. MVP scope

The MVP will provide:

- search by name and aliases;
- exploration through keywords;
- filters for genre, theme, platform, game mode, perspective, multiplayer, and release period;
- company context only after coverage can be complemented and reconciled across
  suitable sources;
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

**Defer from the first Comparable Games iteration:** developer, publisher,
porting, supporting, and the related company mapping.

The 100-game IGDB PoC sample found company information for 51% of records,
developers for 47%, and publishers for 44%. The 79 relationships that were
present had good structural integrity, but the missing-data rate is too high for
companies to support a principal and balanced MVP comparison dimension.

This is a deferral, not a rejection. Developer and publisher coverage must be
compared and reconciled with Wikidata and other suitable sources before the
dimension is reconsidered. Company names may later help producers investigate
organizational context, but the field alone does not establish company size,
budget, official status, distribution strength, or responsibility for a game's
commercial outcome.

Rules:

- companies are multivalued relationships;
- role is mandatory;
- absence in one source is not a conflict;
- different roles may be complementary;
- porting and supporting do not replace developer or publisher.
- missing company data means "not informed by the source", never "no company
  existed";
- company presence does not establish that a record is an official market
  product;
- companies must not drive filters, rankings, confidence scores, or eligibility
  in the first iteration.

### 4.4 Product types and relationships

**Include:** `game_type`, `parent_game`, `dlcs`, `expansions`, `standalone_expansions`, `ports`, `remakes`, `remasters`, `bundles`, `version_parent`, `version_title`.

**Defer:** `expanded_games`, detailed `game_versions`, `forks`, and `similar_games`.

> Related products remain separate records.

For the first analytical catalogue, records classified by IGDB as
`game_type = Mod` are excluded from the general Comparable Games search. This
simple rule removes many mods, ROM hacks, and fan games while IGDB is the only
active source, but it does not guarantee complete detection because IGDB has no
specific ROM-hack type and may classify records imprecisely. The rule is
provisional and independent of company data.

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
- missing, under-review, or inactive observations;
- manual decisions;
- minimum required evidence.

Do not permanently persist complete raw payloads, repeated snapshots, fully processed API responses, or detailed logs for normal cases.

### 7.3 Canonical model

Accept only data that satisfies identity, type compatibility, relationship consistency, provenance, contextual precedence, and no unresolved severe conflict.

## 8. Conflict handling

### 8.1 Release dates

Treat as a conflict only for the same product, platform, region, release type/status, and precision.

For a true conflict:

- prefer the most appropriate and reliable source for that platform;
- preserve the divergent observation;
- display platform with the date;
- show ecosystem context when useful, such as `PC — 2020-08-07 (Steam)`.

### 8.2 Companies

- treat companies as multivalued relationships;
- combine complementary information;
- consider product, version, platform, and role;
- do not merge companies by name alone;
- preserve relevant conflicts.

### 8.3 Classifications

IGDB is canonical for genres, themes, modes, perspectives, and keywords.

External classifications remain preserved with provenance, but are not automatically merged and do not alter canonical filters.

### 8.4 Product types and relationships

- keep products separate;
- preserve original classifications;
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

## 10. Missing data, deactivation, and retention

### 10.1 Consecutive absence

```text
1st absence
→ keep and mark

2nd absence
→ reevaluate

3rd absence
→ deactivate

explicit removal or trusted complete snapshot
→ deactivate immediately
```

Absence does not mean automatic deletion.

### 10.2 Reactivation

If the record reappears:

- reactivate it;
- reset the absence counter;
- reevaluate the canonical value.

### 10.3 Physical deletion

There will be no abrupt automatic deletion.

Inactive records will remain within a maximum storage budget. When it is exceeded:

- remove the oldest inactive records;
- delete only unprotected records;
- process in small batches;
- exclude active and under-review records.

The limit will be based on occupied bytes, not a fixed record count.

The PoC will measure average active, intermediate, and inactive record sizes, index cost, growth per cycle, and safe remaining Neon capacity.

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
- reduced reprocessing.

### Validation layers

- normal cases proceed without full intermediate persistence;
- ambiguous cases retain only minimum evidence;
- the canonical model receives approved data only.

### Storage

- growth compatible with the Neon free tier;
- measurable inactive-storage budget;
- controllable retention;
- sustainable indexes;
- no unnecessary raw payload retention.

### Final criterion

> IGDB and the pipeline are sufficient for the MVP when they produce useful searches, preserve context and provenance, avoid dangerous merges, operate incrementally and idempotently, and remain compatible with free infrastructure.

## 13. Next step

The next work cycle will implement the IGDB PoC and validate this document without expanding the MVP scope.
