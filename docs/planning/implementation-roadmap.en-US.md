# Implementation Roadmap

> Updated: August 26, 2026

## Purpose

This document provides a milestone-level view of the Game Market Intelligence implementation.

It records completed increments, the current delivery focus, and the expected evolution of the product without replacing detailed domain, architecture, source-assessment, design, proof-of-concept, or increment-specific documentation.

The roadmap may change as real data sources, infrastructure constraints, product validation, deployment learning, and storage measurements provide new evidence.

## Product direction

Game Market Intelligence is a decision-support platform for Game Producers and small studios.

The product organizes comparable games, trustworthy research references, source-aware evidence, and future commercial context into a workflow that helps reduce uncertainty during early product planning.

```text
Game idea
    ↓
Comparable-games discovery
    ↓
Research references
    ↓
Commercial evidence
    ↓
Market analysis
    ↓
Stakeholder decision support
```

The platform must preserve provenance, distinguish source observations from internal conclusions, and remain extensible to multiple sources.

## Delivery principles

Game Market Intelligence is developed through small, complete vertical increments.

Each delivery should:

- answer or enable a real product question;
- provide a usable end-to-end experience when appropriate;
- preserve Domain, Application, Infrastructure, API, Collector, Shared, and Web boundaries;
- preserve source provenance and external identities;
- include appropriate automated tests;
- update technical and product documentation;
- pass Pull Request validation before entering `main`;
- remain deployable through the approved zero-cost infrastructure;
- avoid irreversible domain or persistence decisions before real-source evidence is sufficient;
- integrate sources incrementally without preventing future multi-source support;
- avoid exposing the persistence model directly through API or frontend contracts;
- treat storage limits as a product constraint;
- preserve relevant catalog coverage before reducing metadata depth for capacity reasons.

## Milestone 0 — Project foundation

Status: **Completed**

Delivered:

- .NET solution structure;
- Domain, Application, Infrastructure, API, Shared, Collector, and Web projects;
- PostgreSQL local development environment;
- EF Core and Npgsql configuration;
- initial `DataSource` and `SourceReliability` modeling;
- automated-test foundations;
- GitHub Actions Continuous Integration;
- Terraform and deployment foundations;
- initial architecture and product documentation.

## Milestone 1 — Comparable Games foundation and first read experience

Status: **Completed**

Delivered:

- `Genre`, `Platform`, and `Game` domain foundations;
- normalized-name rules and duplicate prevention;
- many-to-many game relationships with genres and platforms;
- PostgreSQL persistence and integration tests;
- Comparable Games search and details endpoints;
- genre and platform listing endpoints;
- partial-name, genre, platform, release-year, and pagination support;
- centralized exception handling with `ProblemDetails`;
- Blazor WebAssembly API integration;
- responsive shell, navigation, filters, result states, pagination, and reusable components;
- environment-based deployment integration;
- browser validation with the current empty production dataset;
- visible and explicit search submission in the filter form;
- documentation and learning review for the first frontend milestone.

Data-dependent validation remains pending until representative real data is persisted.

## Milestone 2 — IGDB vertical MVP

Status: **In progress**

### Goal

Deliver the first functional real-data MVP using IGDB as the first active source, from authorized collection through persistence, deployment, API use, and product presentation.

This milestone does **not** abandon the multi-source strategy.

It delivers one source vertically so the project can validate:

- Collector behavior;
- source-specific contracts;
- canonical persistence;
- provenance;
- storage impact;
- deployment;
- API use;
- frontend presentation;
- attribution and operational behavior.

## 2.1 IGDB proof of concept

Status: **Completed for the approved Milestone 2 persistence scope**

Validated areas include:

- Twitch OAuth authentication;
- IGDB game retrieval;
- source-specific contracts;
- controlled samples;
- reproducible samples;
- product types;
- parent and related-product behavior;
- bundles;
- contextual platform releases;
- release precision and nullability;
- regions and statuses;
- genres;
- themes;
- game modes;
- player perspectives;
- keywords;
- involved companies;
- collections;
- covers and image metadata;
- alternative/localized-title observations;
- nullability and incomplete records;
- field usefulness for Comparable Games;
- source-specific limitations.

Key implementation decisions produced by the PoC include:

- `first_release_date` remains a canonical summary/filter value;
- contextual releases require a separate release model;
- original games, ports, remakes, remasters, bundles, DLCs, expansions, and versions remain distinct products;
- relationships must not automatically propagate classifications or metadata;
- companies require roles and provenance;
- collections are approved;
- franchises remain deferred;
- themes, modes, perspectives, and keywords are persisted as separate source-neutral classifications;
- image binaries are not stored;
- source nullability remains meaningful and is not converted into false values;
- normalized names are useful for lookup but are not sufficient identity evidence.

Historical PoC documents remain the evidence source for detailed field decisions and should not be rewritten as implementation documents.

## 2.2 Lightweight multi-source compatibility spike

Status: **Completed**

The compatibility spike confirmed that the Milestone 2 architecture can support IGDB now without structurally depending on it.

Validated architectural rules include:

- internal canonical identifiers;
- multiple external identities through `DataSource + ExternalId`;
- source-specific contracts;
- source-specific mappers;
- source-independent canonical entities;
- provenance-bearing contextual entities;
- source-specific metadata outside universal domain concepts;
- conservative future reconciliation;
- no dependency on Steam for canonical identity;
- no provider-specific ID properties on `Game`.

Exit criterion achieved:

> The IGDB-oriented persistence design can proceed without requiring structural redesign when Wikidata and Steam are later introduced.

Multi-source reconciliation itself remains deferred to Milestone 3.

## 2.3 Domain and persistence implementation

Status: **In progress — GMI-25 through GMI-29 completed and integrated; GMI-30 validation in progress**

The persistence implementation is delivered through child Jira issues under GMI-14.

### GMI-25 — External source identity

Status: **Completed**

Delivered:

- canonical/external identity separation;
- `ExternalGameRecord`;
- source identity through `DataSourceId + ExternalId`;
- optional link from external record to canonical `Game`;
- observation timestamps;
- source-update timestamp;
- duplicate prevention;
- EF Core mappings;
- migration;
- persistence tests.

### GMI-26 — Contextual releases with provenance

Status: **Completed**

Delivered:

- contextual `GameRelease`;
- canonical `Game.FirstReleaseDate`;
- platform-specific release context;
- external release identity;
- source `ExternalGameRecord`;
- partial release-date representation;
- region, status, and observation metadata;
- restrictive delete behavior;
- EF Core mappings;
- migration;
- persistence tests.

### GMI-27 — Approved queryable classifications

Status: **Completed**

Delivered canonical classifications:

- `Theme`;
- `GameMode`;
- `PlayerPerspective`;
- `Keyword`.

Delivered external identities:

- `ExternalThemeRecord`;
- `ExternalGameModeRecord`;
- `ExternalPlayerPerspectiveRecord`;
- `ExternalKeywordRecord`.

Delivered provenance-bearing associations:

- `GameTheme`;
- `GameGameMode`;
- `GamePlayerPerspective`;
- `GameKeyword`.

The implementation preserves source identity and does not treat normalized names as proof of cross-source equivalence.

### GMI-28 — Products, companies, collections, and relationships

Status: **Completed and integrated**

Delivered product modeling:

- `Game.ProductType`;
- `GameProductType`;
- `GameProductRelationType`;
- `GameProductRelation`;
- directed relationships between distinct canonical products;
- provenance through source and target `ExternalGameRecord`;
- no automatic propagation between related products.

Delivered company modeling:

- canonical `Company`;
- `ExternalCompanyRecord`;
- `GameCompanyRole`;
- provenance-bearing `GameCompany`;
- N:N game/company cardinality;
- multiple roles when supported by evidence.

Delivered collection modeling:

- canonical `Collection`;
- `ExternalCollectionRecord`;
- provenance-bearing `GameCollection`;
- N:N game/collection cardinality.

Delivered persistence integrity:

- source-neutral canonical concepts;
- unique external identities per source;
- composite association keys;
- restrictive delete behavior;
- migration coverage;
- delete/integrity tests;
- integration-test database reset updated for new schema tables.

Final GMI-28 quality gate:

```text
Build: passed
Tests: 424 passed
Failures: 0
Ignored: 0
```

GMI-28 was merged into `develop`, pushed to the remote branch, and closed with a clean working tree.

### GMI-29 — Cover and screenshot metadata

Status: **Completed and integrated**

Delivered image metadata modeling:

- source-neutral `GameImage`;
- `GameImageType` with `Cover` and `Screenshot`;
- source image record identity through `ExternalId`;
- source asset addressing through `SourceImageId`;
- optional width and height;
- optional `SortOrder`;
- canonical `GameId`;
- provenance through `ExternalGameRecordId`;
- no duplicated `DataSourceId`;
- no image binary persistence;
- no persisted ready-made game image URL.

Delivered integrity rules:

- `GameImage` can be created only from an `ExternalGameRecord` already linked to a canonical `Game`;
- required/trimmed external and source-image identifiers;
- positive optional dimensions;
- non-negative optional sort order;
- unique evidence through `ExternalGameRecordId + ExternalId + Type`;
- restrictive delete behavior for both the canonical game and supporting external record;
- PostgreSQL test reset updated for the new table.

Delivered persistence transition:

- removed persisted `Game.ImageUrl`;
- added and validated the `game_images` migration;
- added and validated the migration removing `Games.ImageUrl`;
- preserved `Platform.ImageUrl` outside the GMI-29 scope.

Delivered source-aware URL resolution:

```text
GameImage metadata
→ primary-cover selection
→ ExternalGameRecord
→ DataSource.Code
→ IGameImageUrlResolver
→ public ImageUrl
→ Shared contract
→ frontend
```

Current IGDB URL resolution is derived from `SourceImageId`.

The frontend continues to receive a simple `ImageUrl?`; provider-specific identifiers, CDN rules, and persistence metadata remain backend concerns.

Delivered primary-cover selection:

- only `Cover` records are eligible;
- populated `SortOrder` is preferred over `null`;
- lower `SortOrder` is preferred;
- `GameImage.Id` provides a deterministic tie-breaker;
- details use a single primary-cover lookup;
- paginated search uses batch cover lookup to avoid N+1 queries.

Delivered Application/API-read integration:

- `GameDetails.ImageUrl` is populated from resolved image metadata when available;
- `GameSearchItem.ImageUrl` is populated after batch cover lookup;
- existing frontend fallback behavior remains valid when no cover or supported URL is available;
- screenshots remain persisted for future detail/gallery use and are not yet exposed by the current public contract.

Consistency review completed during GMI-29:

- canonical links in `External*Record` remain protected from relinking to a different canonical entity;
- `ExternalCompanyRecord` and `ExternalCollectionRecord` timestamp rules were aligned with the other external-record models;
- observation timestamps are normalized to UTC;
- `LastSeenAt` does not move backwards;
- `SourceUpdatedAt` advances only when a newer source timestamp is observed.

Synchronization policy defined for future Collector work:

- absence in one collection run does not automatically mean source removal;
- only a known-complete, reliable observation may justify synchronizing an association out of the current state;
- removing an association does not remove the corresponding `External*Record`;
- source-specific changes must not propagate blindly to evidence from other sources;
- the operational database stores treated current state plus minimum necessary provenance rather than indefinite detailed change history.

Final GMI-29 quality gate:

```text
Build: passed
Tests: 460 passed
Failures: 0
Ignored: 0
```

GMI-29 was merged into `develop`, pushed to the remote branch, and closed with a clean working tree.

### GMI-30 — Persistence and storage-budget validation

Status: **In progress — migration, query-plan, contract, and representative-volume validation completed**

Validated migration and compatibility checks:

- migration sequence reviewed through `RemoveGameImageUrl`;
- `dotnet ef migrations has-pending-model-changes` confirmed no model drift;
- the existing Neon-backed database reported no pending migrations;
- the local PostgreSQL database successfully accepted all pending migrations when targeted explicitly through `--connection`;
- a separate empty PostgreSQL validation database successfully applied the full migration chain from zero;
- the complete solution build passed;
- the complete automated suite passed with 460 of 460 tests;
- current public image contracts remained compatible.

Development-environment finding:

- `DefaultConnection` may resolve from .NET User Secrets and therefore point to Neon;
- local EF validation must use an explicit connection target when the intended database is Docker PostgreSQL.

Representative local validation dataset:

```text
Games                  10,000
GameGenres             20,000
GamePlatforms          20,000
ExternalGameRecords    10,000
GameImages             16,666
```

Baseline measured after `ANALYZE`:

```text
Table data             ~7.4 MB
Indexes                ~11 MB
Total database objects ~19 MB
```

Representative query-plan findings:

- name substring search with `ILIKE '%term%'` used a sequential scan and completed in approximately 3.7 ms at 10,000 games;
- release-year filtering through `EXTRACT(YEAR FROM FirstReleaseDate)` used a sequential scan and completed in approximately 1.8 ms;
- genre filtering used `IX_GameGenres_GenreId`;
- platform filtering used `IX_GamePlatforms_PlatformId`;
- batch cover lookup used `IX_game_images_GameId`;
- no new index is justified solely to eliminate the currently inexpensive sequential scans.

High-cardinality pressure scenario added, while preserving the same 10,000-game catalog:

```text
Contextual releases            30,000
Theme associations             30,000
Game-mode associations         20,000
Player-perspective associations 20,000
Keyword associations           80,000
Company associations           20,000
Collection associations         5,000
Product relations               2,500
```

Measured after the high-cardinality extension:

```text
Table data             ~29 MB
Indexes                ~33 MB
Total database objects ~63 MB
```

The pressure scenario added approximately 44 MB without increasing the number of games.

Largest observed storage consumers:

- `game_keywords`: approximately 14 MB;
- `game_releases`: approximately 10.1 MB;
- `game_images`: approximately 5.3 MB;
- `game_themes`: approximately 5.3 MB;
- `game_companies`: approximately 5.3 MB.

Measured budgeting evidence through `pg_total_relation_size`:

| Relation | Rows | Approximate total bytes per row |
|---|---:|---:|
| `game_releases` | 30,000 | 344 B |
| `game_images` | 16,666 | 328 B |
| `Games` | 10,000 | 327 B |
| `game_companies` | 20,000 | 271 B |
| `game_themes` | 30,000 | 182 B |
| `GameGenres` | 20,000 | 181 B |
| `GamePlatforms` | 20,000 | 181 B |
| `game_keywords` | 80,000 | 181 B |

Capacity interpretation:

- the measured ~63 MB / 10,000-game pressure scenario is a budgeting reference, not a fixed production forecast;
- catalog coverage and metadata depth are separate capacity decisions;
- relevant games should not be arbitrarily discarded merely to satisfy a fixed record-count target;
- metadata depth and multiplicative associations should be controlled according to approved product questions and observed capacity;
- indexes are a material part of storage cost and must be justified by real access paths;
- keywords and contextual releases are the strongest synthetic high-cardinality pressure points measured so far.

Current capacity policy:

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

Retention priority remains:

1. current canonical product state;
2. active external identities;
3. provenance still supporting the current state;
4. recent useful historical/auxiliary data when such history exists.

Capacity-driven deletion must never be interpreted as a source-domain fact.

Storage retention and source reconciliation remain separate concerns.

Remaining GMI-30 closure work:

- consolidate the measured evidence in bilingual project documentation;
- confirm the final operational budget wording and risk register;
- remove or explicitly retain local synthetic-validation artifacts according to repository policy;
- generate the final GMI-30 `AGENTS.md` revision;
- rerun final build/test and Git quality gates;
- commit documentation and validation artifacts;
- merge the feature branch into `develop`;
- push `develop`;
- update Jira with migration, query-plan, storage, build, and test evidence;
- close the Jira subtask after integration is complete.

## 2.4 Collector implementation

Status: **One-shot execution foundation implemented by GMI-15; client, mapping, and persistent ingestion remain pending**

The Collector should be refactored from PoC structure into production-oriented responsibilities.

Expected structure:

```text
Scheduler
→ Worker
→ IGDB import Job
→ IGDB client
→ IGDB contracts
→ mapper/import boundary
→ canonical model + external identity + provenance
→ repository
→ checkpoint
→ shutdown
```

Required behavior:

- keep the Worker small;
- separate Jobs;
- isolate IGDB contracts;
- map source DTOs into source-neutral application/domain input;
- implement pagination;
- respect rate limits;
- implement retries and safe failure behavior;
- support incremental collection through `updated_at`;
- ensure idempotent execution;
- avoid logging secrets or tokens;
- avoid storing complete raw payloads permanently;
- preserve source identity and provenance;
- preserve relevant catalog coverage while controlling non-essential metadata depth;
- treat complete versus partial source observations explicitly before removing current associations.

The Collector must not map provider responses directly into EF Core entities.

Foundation implemented by GMI-15:

- the Collector runs once per process and leaves recurring scheduling to external infrastructure;
- `IgdbImportWorker` remains small and delegates one execution to `IIgdbImportJob`;
- success, requested cancellation, and unexpected failure have explicit logging and shutdown behavior;
- the Worker and its DI registration have unit tests without network or database access;
- empty template, PoC, and future Steam-integration placeholders were removed;
- the Collector test project was added to the solution and aligned with xUnit, Shouldly, and NSubstitute;
- `AddCollector()` registers only the Worker that is already implemented;
- `Program.cs` does not call `AddCollector()` yet because no concrete, resolvable `IIgdbImportJob` implementation exists.

This temporary inactivity is deliberate. GMI-15 does not create an empty job, an implementation that throws `NotImplementedException`, or a Collector that appears to complete an import without processing data.

Boundaries for the next tasks:

- GMI-16 implements authentication, the IGDB client, pagination, pacing, retries, timeout, and operational cancellation;
- GMI-17 implements response contracts and mapping for the approved first-MVP subset;
- GMI-18 implements persistent orchestration, idempotency, checkpoints, resumption, and progress observability.

## 2.5 IGDB persistence and data quality

Status: **Persistence model and operational budget validated through GMI-30**

Already implemented at the persistence-model level:

- external identity;
- duplicate prevention by source identity;
- contextual releases;
- approved classifications;
- product types;
- product relationships;
- companies and roles;
- collections;
- cover/screenshot metadata;
- source-aware image URL construction for current read use cases;
- restrictive delete behavior;
- provenance-bearing associations.

Still required in the ingestion path:

- idempotent create/update orchestration;
- canonical selection rules;
- conservative inclusion rules;
- handling of rejected/problematic source records;
- source-update behavior;
- safe re-execution;
- explicit handling of complete versus partial observations before removing current associations;
- representative real-data validation;
- measured production storage impact;
- product-driven metadata-depth decisions for high-cardinality fields.

## 2.6 API and frontend integration

Status: **Pending representative real data and post-persistence API work**

The API should expose source-neutral use-case contracts.

It should **not** expose every persisted field or internal provenance identifier simply because it exists in the database.

The current read contracts already preserve a simple frontend image boundary:

- `GameDetails.ImageUrl?`;
- `GameSearchItem.ImageUrl?`.

These URLs are resolved from persisted image metadata in the backend rather than stored directly on `Game`.

Potential additional API/detail concepts after persistence completion include:

- product type;
- related products;
- companies and roles;
- collections;
- contextual releases;
- selected classification context;
- screenshots/gallery metadata if a validated detail experience requires them;
- source/attribution information.

The frontend should organize these contracts for the user's decision workflow rather than mirror the database schema.

Likely product presentation:

### Comparable Games result cards

Keep concise.

Potential additions:

- product type;
- selected high-value context only.

Avoid displaying complete provenance or every persisted association in cards.

### Game details

Potential additions:

- product type;
- related products;
- companies grouped by role;
- collections;
- contextual release information;
- selected classifications;
- source and attribution context.

### Filters

Potential future filters include:

- product type;
- themes;
- game modes;
- companies;
- release periods.

A filter should be introduced only when it answers a validated product question and data coverage is sufficient.

Validation with real data must include:

- populated genre/platform controls;
- populated result cards;
- cover URL resolution and fallback behavior;
- pagination without image-query N+1 behavior;
- game details;
- source and provenance presentation;
- reliability and limitations;
- related products;
- product types;
- company roles;
- collections;
- keyword usefulness;
- attribution and original links where required and permitted.

## 2.7 Worker deployment and operation

Status: **Planned**

Define and validate:

- scheduled one-shot execution;
- approved zero-cost scheduler;
- production secrets;
- Neon connectivity;
- packaging/build;
- logs and failure visibility;
- retry/rerun procedure;
- checkpoints;
- execution duration;
- rate-limit compliance;
- operating-cost confirmation.

## Milestone 2 Definition of Done

Milestone 2 is complete when:

- IGDB PoC decisions are documented and approved;
- the multi-source compatibility spike confirms extensibility;
- the approved persistence model is complete;
- GMI-25 through GMI-30 are integrated and validated;
- the Collector has clear responsibilities;
- collection is paginated, incremental, and idempotent;
- IGDB data is mapped without making IGDB the internal model;
- canonical records retain external identity and provenance;
- representative data is persisted in Neon;
- storage impact is measured and acceptable;
- API contracts expose the selected product-useful data;
- frontend operates with representative real data;
- source and reliability context are visible where useful;
- attribution requirements are satisfied;
- the Worker is deployed and successfully executed;
- failure and rerun behavior are documented;
- automated tests pass;
- documentation is updated;
- the real-data MVP works end to end.

## Milestone 3 — Multi-source enrichment and reconciliation

Status: **Planned**

### Goal

Add Wikidata and Steam as complementary sources without replacing the IGDB vertical MVP or rewriting its source-neutral foundations.

### 3.1 Wikidata proof of concept

Evaluate:

- authorized structured access;
- QID and external identifiers;
- aliases and canonical links;
- company and relationship statements;
- statement-level variability and missing data;
- licensing and attribution;
- query limits and operational stability;
- reconciliation value.

Franchises remain deferred unless a later product requirement explicitly activates them.

### 3.2 Steam proof of concept

Evaluate only official and permitted access paths:

- AppId identity;
- names and Steam-specific release information;
- developers and publishers;
- categories and features;
- supported systems and languages;
- Steam-specific signals that are legally usable;
- storage, attribution, regional, and endpoint limitations.

A game without a Steam identity remains valid in the canonical catalogue.

Steam must not become a required identity authority.

### 3.3 Common observation and reconciliation model

Define a source-independent comparison boundary for concepts such as:

- source and external identity;
- observed name and aliases;
- release observations with context;
- platforms;
- developers and publishers;
- collections;
- product type;
- related-product relationships;
- source-specific metadata.

Reconciliation must:

- prioritize strong crossed identifiers;
- use conservative candidate generation;
- preserve distinct related products;
- keep classifications and provenance;
- avoid majority voting without semantic context;
- block automatic reconciliation for serious product-type conflicts;
- preserve manual approvals and rejections when necessary.

Normalized names may contribute to candidate discovery but do not prove identity.

### 3.4 Multi-source product experience

Potential additions:

- source convergence and divergence context;
- confidence profiles;
- source-aware field presentation;
- reconciliation status where useful;
- source links and attribution;
- clear distinction between observations, canonical values, and GMI inference.

## Milestone 3 Definition of Done

Milestone 3 is complete when:

- Wikidata and Steam PoCs are documented and approved for defined roles;
- both integrations follow the same architectural boundaries as IGDB;
- observations can be compared without erasing source context;
- reconciliation is conservative and testable;
- provenance and confidence are visible where useful;
- the multi-source product flow works end to end.

## Later milestones

### Advanced Comparable Games exploration

Potential scope:

- multiple genres;
- multiple platforms;
- themes;
- modes;
- perspectives;
- keywords;
- product-type filters;
- company filters;
- release-period filters;
- richer details;
- sorting;
- performance optimization;
- saved searches;
- analytical summaries.

Keywords remain a central product-value strategy because they allow producers to begin with an idea or micro-niche rather than only with a known title.

### Market metrics foundation

Priority metrics:

- sales;
- revenue;
- estimated owners;
- downloads;
- active players;
- concurrent players;
- reviews;
- wishlists;
- other justified engagement observations.

All metrics must preserve:

- source;
- meaning;
- period;
- method;
- confidence.

### Market analysis and decision support

Deferred until stable data and validated questions exist:

- market signals;
- genre analysis;
- platform analysis;
- launch-window context;
- source-aware reports;
- recommendations;
- forecasting;
- machine-learning evaluation.

## Current delivery focus

```text
Milestone 2 — IGDB vertical MVP
→ GMI-30 persistence model and storage budget complete
→ GMI-15 one-shot Collector foundation
→ GMI-16 IGDB client and operational behavior
```

Immediate sequence:

1. complete documentation, final gates, and integration of the GMI-15 Collector foundation;
2. implement authentication, the IGDB client, and operational behavior in GMI-16;
3. implement first-MVP IGDB contracts and mapping in GMI-17;
4. implement incremental, idempotent, resumable ingestion in GMI-18;
5. populate representative real data;
6. validate production storage behavior against the measured local budget;
7. expose selected new concepts through API contracts;
8. organize those contracts in the frontend;
9. deploy and operate the Worker;
10. validate the end-to-end real-data MVP.

## Current architectural checkpoint

The current persistence direction is:

```text
External source
→ source-specific observation
→ External*Record / provenance
→ Worker filtering and selection
→ canonical GMI model
→ Application/API use-case contract
→ frontend presentation
```

The persistence layer is intentionally richer than any single frontend screen.

The API selects what each use case needs.

The frontend organizes that selected information for the producer and must not become a direct mirror of the database.

The current capacity lesson is equally explicit:

```text
Preserve relevant catalog coverage
→ control metadata depth by product value
→ monitor multiplicative associations and indexes
→ act before storage pressure becomes an incident
```
