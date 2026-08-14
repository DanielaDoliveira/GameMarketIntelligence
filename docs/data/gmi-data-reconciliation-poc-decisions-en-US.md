# Game Market Intelligence — Data, Reconciliation, and PoC Decisions

**IGDB PoC status:** completed and approved on 2026-08-14 as a technical and
product investigation. Approval guides the definitive Collector but does not
represent production readiness.

## 1. Purpose

This document consolidates the decisions that guide the **Game Market Intelligence
(GMI)** MVP and was updated as the IGDB proof of concept produced evidence.

GMI is not intended to store every possible piece of game data. Its purpose is to select, organize, and present only what genuinely helps producers with early market research, comparable-game discovery, niche exploration, and competitive-context analysis.

> GMI creates value through a focused set of useful data, not through excessive information that makes analysis harder.

## 2. MVP scope

For this iteration, Comparable Games filtering is the first highest-value
product delivery. What enters, leaves, remains detail-only, or is deferred
results from the focused analysis of IGDB coverage, semantics, operations, and
constraints. Within zero-cost operation, the goal is the highest practical
confidence available while remaining transparent with the producer about
source, missing data, and limitations.

The first IGDB MVP will provide:

- search by name; aliases may be added only from fields and sources with an
  approved provenance policy;
- exploration through keywords;
- filters for genre, theme, platform, game mode, multiplayer, and release period;
- player perspectives as optional detail data when reported by the source;
- covers as optional, non-dominant visual support in results and details;
- screenshots as optional visual context in game details;
- context about involved companies;
- collections/series;
- product relationships such as remake, remaster, port, edition, DLC, and expansion;
- provenance and confidence levels;
- transparency about limitations, conflicts, and coverage.

Out of the first IGDB MVP, without removal from the later-iteration product
vision:

- financial metrics, sales, and revenue;
- success prediction and automatic opportunity scoring;
- a custom subgenre taxonomy;
- separate modeling of IP, subfranchise, universe, editorial line, brand, licensed property, or corporate group;
- deep multiplayer analysis;
- complex corporate history;
- detailed edition-content comparison;
- unlimited archival storage of old observations.

Financial metrics may be considered in later iterations after the first
real-data MVP is completed and validated.

## 3. Selected sources

### IGDB

IGDB is approved with caveats as the main catalog source and canonical first-MVP taxonomy for genres, themes, modes, perspectives, keywords, product types, relationships, platforms, release dates, companies, collections, and external identifiers. It is suitable for validating the first MVP, but it is not an authoritative source: individual records and relationships may be incomplete, inconsistent, or non-official. `franchises` remains available from the source but is deferred until after the first MVP.

The first MVP will preserve provenance and apply basic catalogue protections without blocking delivery on a cross-source officiality system. More sophisticated conflict detection, field-level confidence, quarantine, and cross-validation will be refined in the next increment, when a second source is integrated.

### Wikidata

Wikidata will support reconciliation, enrichment, auxiliary validation, and cross-source identifiers. It will not automatically replace IGDB as the canonical taxonomy.

### Steam

Steam will be a specialized source for Steam-specific facts, such as Steam release dates and product identity. The architecture and reconciliation process must not depend on Steam.

## 4. IGDB mapping

### 4.1 Identity and discovery

**Include:** `id`, `name`, `game_type`, `version_parent`, `game_status`, `summary`.

**Defer or exclude:** exclude `alternative_names` from the MVP mapping; defer
`slug`; do not initially import `storyline`.

Rules:

- external identity = `Source + ExternalId`;
- name alone never supports automatic reconciliation;
- `alternative_names` must not be used for display, search, identity, or
  reconciliation in the MVP because the observed values mix regional and
  linguistic variants with executable names, working titles, and ambiguous
  aliases without sufficient provenance;
- evaluate `game_localizations` separately as a more structured candidate for
  regional names, without assuming that regional structure proves official use;
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
- **Game modes:** multiple values use AND; the filter means that the source associates every selected mode with the game record, not that every mode is available on every platform or edition.
- **Perspectives:** ingest as optional many-to-many detail data; defer the public
  filter because current coverage would create excessive false negatives.
- **Keywords:** use structured source IDs; ingest and display as optional data;
  in the first MVP, a keyword is clickable and opens Comparable Games with that
  keyword as a contextual, removable criterion. Manual keyword selection,
  multiple-keyword `AND`, autocomplete, custom keywords, and automatic term
  merging are deferred.
- **Platforms:** multiple values use OR.
- **Multiplayer:** include multiplayer, online co-op, and local/offline multiplayer; selected capabilities use AND without exclusivity.

Out of scope for multiplayer: maximum player count, LAN, drop-in/drop-out, and detailed platform-specific configurations.

#### Game-modes decision

`game_modes` is approved for the MVP with caveats. In the fixed sample of 100
games, 84 records contained at least one mode, 69 contained exactly one, 15
contained multiple modes, and 16 contained none. No duplicate IDs, invalid IDs,
blank names, or conflicting names for the same ID were found. A separate
targeted sample of 26 games from nine known series had 100% coverage and remains
useful for semantic inspection, but it is not representative of general catalogue
completeness. The relationship is many-to-many and nullable.

The field belongs to each IGDB game record and has no platform-level granularity. It does not identify a primary mode, distinguish limited or asymmetric cooperation, measure the importance or quality of a mode, or prove that a reported mode applies to every platform and edition. Missing modes must be treated as unknown source data rather than proof that the capability does not exist. Modes must not be propagated among originals, ports, remakes, remasters, editions, updates, or other related records, and they are not strong reconciliation evidence.

A targeted semantic inspection also found a purported Android version of `Super Mario Galaxy`. As no official Android version exists, that result must not be used as evidence about the official product. It demonstrates that name search, platform associations, and IGDB relationships do not independently prove officiality. Search-by-name results are discovery candidates only; related records must not influence another game's modes, platforms, or releases unless their identity is sufficiently validated.

For this MVP, that residual risk is accepted and documented. The catalogue will make the source visible and avoid claims of completeness or infallibility. Cross-source validation and stronger handling of suspicious records are deferred until a second database is integrated.

The 84% fixed-sample coverage is sufficient to approve a source-qualified public
filter. The filter means "games for which IGDB reports the selected mode"; it
must not imply that omitted games lack that mode. The field may also be displayed
in details and used in comparisons when available.

#### Player-perspectives decision

`player_perspectives` is approved for ingestion and optional detail display in
the MVP, but not as a public filter. In the same fixed sample of 100 games, 45
records contained at least one perspective, 42 contained exactly one, three
contained multiple perspectives, and 55 contained none. No duplicate IDs,
invalid IDs, blank names, or conflicting names for the same ID were found. All
five source values appeared. The relationship is many-to-many and nullable.

The targeted sample of 26 well-known games had 100% coverage, compared with 45%
in the fixed sample. This difference demonstrates a strong completeness bias
toward prominent, well-maintained records. The fixed sample is therefore used
to assess coverage, while the known-game sample is retained only for semantic
interpretation.

The field does not identify a primary or predominant perspective and may combine
perspectives used in different systems, scenes, or modes. It has no platform- or
edition-level granularity. Missing data means unknown, not that the game lacks a
perspective. Values must not be propagated between related products and are not
strong reconciliation evidence. With 55% of the fixed sample missing the field,
a public filter would create too many false negatives; it is deferred until
coverage can be improved or qualified with another source.

### 4.7 Collections and franchises

**Include in the MVP:** `collections`, with identifiers, names, game relationships, and technical synchronization metadata.

**Defer until after the MVP:** `franchises`.

A targeted sample of 26 games from nine known series returned `collections` for all 26 records, and all 26 contained the expected collection. This supports using `collections` to represent series and related groupings, but it does not demonstrate universal IGDB coverage.

Games and collections have a many-to-many relationship. A game may belong to broad and specific groupings at the same time. IGDB provided no hierarchy, priority, or primary-collection indicator; none of these properties may therefore be inferred from association order or labels.

In the same sample, `franchises` appeared in 24 of 26 records. In 21 of those 24 cases, at least one franchise label was also present among the collections. The five records with an additional label showed that `franchises` may provide broader context, but may also represent crossovers, guest appearances, and licensed properties. For example, `Mario Kart 8` and `Kingdom Hearts III` returned multiple franchises without identifying a primary one.

Decisions:

- use `collections` in the MVP as a many-to-many relationship;
- do not infer a primary collection or a hierarchy among collections;
- defer `franchises` without permanently rejecting it;
- do not use `franchises` in the MVP's main ingestion, filters, or reconciliation;
- treat a missing collection or franchise as unknown or not applicable, not as proof that a game is standalone;
- do not interpret either field as evidence of commercial success, audience size, or legal ownership;
- do not separately model IP, subfranchise, universe, editorial line, brand, licensed property, or corporate group.

`franchises` may be reconsidered if the product later needs to analyze crossovers, licensed-IP presence, or brand reach across different series. Even then, it must be modeled as a many-to-many relationship without automatically selecting the first association.

### 4.8 Covers, screenshots, and artworks

`cover` is approved with legal and operational caveats for optional use in
search results and game details. In the frozen 100-record sample, 93 records
contained a cover and seven did not. No invalid IDs, blank `image_id` values,
blank URLs, non-positive dimensions, duplicate image IDs, or conflicting
metadata were observed.

`screenshots` is approved with legal and operational caveats for game details
only. In the same sample, 84 records contained screenshots and 16 did not. The
84 populated records contained 507 images: four records had exactly one, 80
had multiple images, and the observed range was one to 21, with an average of
6.04. No structural defects, duplicate image IDs, or conflicting metadata were
observed.

`artworks` is deferred rather than rejected. It may be reconsidered for a
future visual-research or art-direction capability, but it does not directly
answer the current Comparable Games questions.

Product and presentation rules:

- covers and screenshots are nullable; absence means that no image was
  reported by the source, not that the product has no visual material;
- images are complementary and must not become essential to understanding a
  result or dominate its hierarchy;
- results use a single mobile-first card structure; when a valid cover exists,
  it may appear as a compact thumbnail on the right;
- when a cover is absent or invalid, the permanent image container is omitted
  and textual content uses the available width; a placeholder may still be
  used during loading or where a detail-page composition requires one;
- the container is standardized, but the image preserves its original aspect
  ratio with a contain-style fit; mandatory cropping, distortion, and
  excessive upscaling are avoided;
- screenshots are not displayed in filters or initial result cards; game
  details may show one principal image and a small number of previews, with
  additional images available on demand and loaded lazily;
- the source order may be preserved, but GMI must not infer that the first
  screenshot is the best, primary, or most representative image;
- images are not filters, identity evidence, officiality proof, or strong
  reconciliation signals, and must not be propagated between related records.

Operational and rights policy for the first MVP:

- store the IGDB image record ID, `image_id`, dimensions, source provenance,
  and synchronization metadata; binary-file storage is deferred;
- construct HTTPS URLs with the IGDB CDN size appropriate to the component;
- do not offer image downloads, build an independent image repository, or make
  substantive transformations beyond source-supported sizing;
- maintain a refresh/removal path because IGDB states that removed or replaced
  images remain available for approximately 30 days;
- provide visible, static attribution to IGDB in the product and do not imply
  that IGDB owns the underlying artwork;
- state that image rights remain with their respective rights holders;
- re-evaluate terms and contact IGDB before monetization or another material
  expansion of use.

IGDB's documentation allows API data to be stored and cached and describes
user-facing attribution for commercial integrations, but it does not provide
an explicit per-image copyright licence for each cover or screenshot. The MVP
policy is therefore a cautious operational decision, not a legal determination
that GMI owns or may freely redistribute the images.

### 4.9 Consolidated coverage and nullability

The frozen 100-record sample returned every expected identifier, with no
unexpected or duplicate records. The consolidated presence matrix was:

| Field | Present | Absent |
|---|---:|---:|
| `name` | 100% | 0% |
| `summary` | 88% | 12% |
| `first_release_date` | 100% | 0% |
| `updated_at` | 100% | 0% |
| `game_type` | 100% | 0% |
| `game_status` | 7% | 93% |
| `parent_game` | 21% | 79% |
| `version_parent` | 2% | 98% |
| `platforms` | 100% | 0% |
| `genres` | 92% | 8% |
| `themes` | 61% | 39% |
| `keywords` | 43% | 57% |
| `involved_companies` | 52% | 48% |
| `collections` | 17% | 83% |
| `franchises` | 3% | 97% |
| `release_dates` | 100% | 0% |
| `external_games` | 89% | 11% |
| `websites` | 96% | 4% |

The 100% `first_release_date` result is a selection effect, not a general IGDB
coverage claim: the frozen population required a non-null first release before
the cutoff. `game_status`, product relationships, collections, and franchises
are conditional fields; absence may mean not applicable or not reported and
must not automatically be classified as a data defect.

Relationship evidence remained separated: 21 records had only `parent_game`,
two had only `version_parent`, none had both, and 77 had neither. All sampled
records had platforms and detailed release dates, while 98 had at least one of
`external_games` or `websites`. These sample results do not authorize field
inheritance between related products.

The 43% keyword coverage is insufficient for a manual catalogue-wide keyword
filter that users could reasonably interpret as exhaustive. Keywords remain
approved for nullable many-to-many ingestion and detail display. Clicking one
opens a source-qualified related-games view with a removable contextual
criterion; results are not presented as exhaustive. The data model and search
boundary must preserve source IDs, provenance, and future collection-based
input so that manual multi-keyword filtering can be added later without a
structural remodel, but that complete interface is not part of the first MVP.

No additional segmentation by `game_type` is required for the current decision:
the types and relationships were already evaluated in controlled samples, and
the reduced keyword scope no longer depends on a coverage threshold for a
global manual filter. The GMI-8 coverage-and-nullability task is complete for
the current PoC scope.

### 4.10 Pagination, rate limit, and operational execution

Operational validation combines controlled live calls with simulated HTTP
tests. The live API will not be deliberately overloaded or used to provoke an
HTTP 429 response.

The live execution found 279,206 eligible records under the frozen
`2026-08-04T00:00:00Z` cutoff. Offsets `0`, `1`, `2`, `499`, `500`, `501`,
`139603`, and `279205` each returned one record, with no empty pages or
duplicate identifiers. Repeating offset `500` returned the same identifier
`506`, confirming stability under controlled ID ordering.

The Worker maintained a configured minimum interval of 275 ms between request
starts. The smallest observed interval was approximately 594.87 ms, every live
call returned HTTP 200, and no HTTP 429 was provoked.

Eleven handler-specific automated cases approved:

- respect for `Retry-After` after HTTP 429;
- exponential backoff of 250, 500, and 1,000 ms;
- retries for HTTP 500, 502, 503, and 504;
- at most three retries after the original attempt;
- retry after a transient timeout;
- cancellation during the delay;
- one token renewal after HTTP 401;
- no loop when the renewed token also receives HTTP 401;
- request cloning before every new attempt.

After the focused tests, the complete solution suite also passed all 119 tests,
with no identified regressions.

The approved policy is bounded recovery, never unlimited retry. A
server-provided delay is capped at 30 seconds per attempt. Jitter may be added
if multiple synchronized Collector instances are ever deployed.

The PoC does not yet persist checkpoints. A future import job may only advance
a checkpoint after committing its complete unit of work; replaying a page must
be idempotent, and a partial or failed batch must not be marked complete.
Checkpoints, checksums, persistent resume, and the overlapping window remain
future implementation criteria rather than proven capabilities.

## 5. Search and user experience

GMI will distinguish two intentions:

### Find a known game

- name;
- approved aliases with sufficient provenance;
- refinement by platform, period, and type.

### Explore an idea or niche

- a keyword selected contextually from a game detail;
- refinement through genres, themes, modes, platforms, and release period;
- perspectives shown as complementary details when available.

Images support recognition and visual breathing room without replacing the
textual comparison. Covers are optional in results, while screenshots remain
inside details and on-demand galleries.

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

Without a strong ID, use composite signals: normalized name, approved aliases
with sufficient provenance, type, companies, platforms, release period,
collections, and declared relationships.

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

The criteria originally defined for the PoC and future pipeline were:

### Coverage

- sufficient coverage for essential fields;
- identifiable and explainable limitations;
- no dependency on unreliable fields.

### Keywords

- preserve source IDs, provenance, and nullable many-to-many associations;
- display available keywords in game details;
- allow a single clicked keyword to open non-exhaustive related games;
- defer manual selection, multiple-keyword `AND`, and autocomplete;
- keep the search boundary extensible without requiring a structural remodel;
- do not create custom keywords or automatically merge terms.

### Filters

- AND for genres, themes, modes, and multiplayer;
- no manual keyword filter in the first MVP; one contextual keyword may be
  active through related-game navigation;
- no public perspective filter in the first MVP;
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
- offset pagination validated from the first through the last eligible record;
- rate-limit compliance through controlled pacing without provoking HTTP 429;
- bounded retries for HTTP 429, 500, 502, 503, 504, and timeout;
- validated `Retry-After`, cancellation, and one-time renewal after HTTP 401;
- persistent resume deferred until the import job has a checkpoint;
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

### Future pipeline acceptance criterion

> The definitive pipeline is sufficient for the MVP when it produces useful
> searches, preserves context and provenance, avoids dangerous merges, operates
> incrementally and idempotently, and remains compatible with free
> infrastructure.

### 12.1 Consolidated approval — GMI-10

The criteria were originally written across two different phases: what the
exploratory PoC could demonstrate without persistence and what only a real
pipeline can measure. Missing capabilities from the second phase do not
invalidate the investigation; they become acceptance criteria for the future
implementation.

| Area | Classification | Conclusion |
|---|---|---|
| Comparable Games usefulness | Approved | IGDB provides sufficient data for MVP research, comparison, filters, and details. |
| Coverage and nullability | Approved with limitations | Missing values were measured and must remain explicit rather than invented. |
| Types and relationships | Approved with conditions | `game_type`, `parent_game`, and `version_parent` must be interpreted together and do not authorize automatic merging. |
| Platform and regional dates | Approved with conditions | They are useful but may be incomplete and have variable precision. |
| Complementary fields | Classified | Every candidate was approved, deferred, or rejected for the first MVP. |
| Keywords | Partially approved | Ingestion, details, and contextual navigation are approved; the complete manual filter is deferred. |
| Covers and screenshots | Approved with restrictions | Optional, non-dominant use with visible attribution and preserved rights. |
| Pagination and pacing | Approved | Live offsets, including the last eligible record, and safe pacing were validated. |
| HTTP resilience | Approved | HTTP 429, `Retry-After`, 5xx failures, timeout, cancellation, and retry limits were tested. |
| Authentication | Approved | OAuth succeeded and one renewal after HTTP 401 was validated without a loop. |
| Cross-source reconciliation | Not validated in this PoC | Rules are defined, but IGDB, Wikidata, and Steam were not jointly integrated. |
| Persistence and idempotency | Future implementation | The PoC does not write to the database. |
| Checkpoint and resume | Future implementation | No persistent synchronization state exists yet. |
| `updated_at` versus checksum | Future implementation | The comparison depends on real ingestion and persistence. |
| Neon storage | Future implementation | Volume, indexes, retention, and cost must be measured with imported data. |
| Definitive canonical model | Outside PoC scope | It will be designed from the consolidated decisions. |

IGDB is not considered a perfect or complete source. The decision concerns fit
for the current context: for a free application with constrained
infrastructure and budget, the observed value is satisfactory for the first
MVP. The product must communicate missing data and provenance rather than hide
the gaps. Future iterations may improve coverage and confidence by combining
complementary sources without making Steam a requirement for identity or
catalogue admission.

### Investigation-closing decision

> The IGDB PoC is approved as a technical and product investigation. The source
> is suitable to support the Comparable Games MVP provided that nullable fields,
> provenance, product relationships, incomplete dates, and image restrictions
> are preserved. Approval authorizes the design of a definitive Collector but
> does not represent production readiness. Persistence, idempotency,
> checkpoints, incremental updates, checksums, storage, and cross-source
> reconciliation must be validated while implementing the real pipeline.

## 13. Next step

The IGDB PoC is closed. Only the consolidated documentation will move to the
`develop` branch; the temporary branch remains technical evidence. The next
iteration must design and implement a clean definitive Collector while
validating persistence, idempotency, checkpoints, incremental updates,
checksums, storage, and cross-source reconciliation.
