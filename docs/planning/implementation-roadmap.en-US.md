# Implementation Roadmap

> Updated: August 14, 2026

## Purpose

This document provides a milestone-level view of the Game Market Intelligence implementation.

It records completed increments, the current delivery focus, and the expected evolution of the product without replacing detailed domain, architecture, source-assessment, design, proof-of-concept, or increment-specific documentation.

The roadmap may change as real data sources, infrastructure constraints, product validation, and deployment learning provide new evidence.

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
- integrate sources incrementally without preventing future multi-source support.

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

Deliver the first functional real-data MVP using IGDB as the first active source, from authorized collection through deployment and product presentation.

This milestone does **not** abandon the multi-source strategy. It delivers one source vertically so the project can validate the Collector, persistence, deployment, API, frontend, provenance, and operational workflow before adding further sources.

The first product-value priority is a trustworthy Comparable Games filtering
experience. The completed IGDB study determines which filters enter this MVP,
which fields remain contextual details, which capabilities are deferred, and
how the Collector preserves nullable or ambiguous evidence. Within the
zero-cost constraint, the goal is not perfect catalogue completeness but the
highest practical reliability with transparent source limitations for the
producer.

### 2.1 IGDB proof of concept

Status: **Completed and approved on August 14, 2026**

Validated:

- Twitch OAuth authentication;
- IGDB games endpoint integration;
- source-specific contracts;
- recently updated sample through `updated_at`;
- controlled sample by identifiers;
- product types and relationships, including parent, version, DLC, remake,
  bundle, port, remaster, Mod, and expansion cases;
- platform- and region-specific releases, nullable precision, and product-owned
  release histories;
- consolidated coverage and nullability in a frozen 100-game sample;
- involved companies, collections, franchises, game modes, player
  perspectives, covers, screenshots, and alternative-name limitations;
- pagination across controlled offsets, live request pacing, token renewal,
  bounded retries, timeout, and cancellation;
- explicit HTTP error diagnostics;
- documentation of field decisions, nullability, metadata completeness,
  relationship behavior, legal image caveats, and operational limits.

Current-MVP outcomes:

- `involved_companies` is included as nullable relationship data; 52% of the
  frozen sample had company information, and future sources may improve
  coverage;
- game modes are approved as a source-qualified public filter;
- player perspectives are detail-only; their public filter is deferred;
- keywords support details and contextual related-game navigation; the manual
  multi-keyword filter is deferred;
- covers are optional, non-dominant visuals; screenshots are detail-only and
  artworks are deferred;
- records classified as `game_type = Mod` will be excluded from the first
  analytical catalogue as a provisional safeguard against mods, ROM hacks, and
  fan games;
- the PoC approves designing a clean definitive Collector but does not claim
  production readiness for persistence, checkpoints, checksums, storage, or
  cross-source reconciliation.

### 2.2 Lightweight multi-source compatibility spike

Before approving the definitive canonical mapping and significant persistence
changes, perform a small architectural compatibility review for Wikidata and
Steam.

The spike is documentary and does not require authentication, production clients, or full proofs of concept.

It must confirm that the Milestone 2 design supports:

- internal canonical game identifiers;
- multiple external identities through `Source + ExternalId`;
- source-specific response contracts;
- source-specific mappers;
- a source-independent import or observation boundary;
- provenance preservation;
- source-specific metadata without forcing it into the canonical model;
- future comparison and reconciliation of observations;
- future field-level differences in names, dates, platforms, companies, and relationships.

Exit criterion:

> The IGDB implementation can proceed without requiring structural redesign when Wikidata and Steam are introduced in Milestone 3.

### 2.3 Collector implementation

After the PoC and compatibility spike:

- refactor the proof-of-concept structure;
- keep the Worker as a small execution coordinator;
- separate concrete Jobs;
- introduce an import service or use case;
- introduce an IGDB mapper;
- keep IGDB contracts isolated from the canonical model;
- implement pagination and rate-limit handling;
- implement retries and safe failure behavior;
- implement incremental collection through `updated_at`;
- evaluate checksums when useful;
- ensure idempotent execution;
- avoid logging secrets or access tokens.

Expected flow:

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

### 2.4 Domain and persistence review

No significant migration should be created before the PoC evidence and multi-source compatibility spike are reviewed.

The approved model must not:

- use an IGDB identifier as the `Game` primary key;
- assume one external identity per game;
- expose IGDB-specific types as universal domain concepts;
- discard source provenance;
- force every source-specific field into `Game`;
- merge related products only by normalized name.

The model should preserve:

- canonical GMI identity;
- external source identity;
- source association;
- collection and source-update timestamps where relevant;
- related products as distinct records;
- optional and contextual release observations;
- conservative reconciliation boundaries.

### 2.5 IGDB persistence and data quality

Implement:

- idempotent creation and update;
- duplicate prevention by external identity;
- conservative catalogue-inclusion rules;
- handling for nullable and incomplete records;
- minimal evidence persistence for problematic or ambiguous records;
- no mandatory permanent storage of every successful raw payload;
- data-quality checks;
- safe re-execution;
- integration tests.

### 2.6 API and frontend integration

Validate the existing experience with representative IGDB data:

- populated genre and platform controls;
- populated result cards;
- multi-page pagination;
- game details;
- source and provenance presentation;
- reliability and limitation communication;
- treatment of related versions and product types;
- usefulness of themes and keywords;
- keyword-led idea and niche exploration;
- visible source links where required and permitted.

### 2.7 Worker deployment and operation

Define and validate:

- execution model: scheduled one-shot process;
- GitHub Actions or another approved zero-cost scheduler;
- production secrets;
- Neon connectivity;
- deployment/build packaging;
- logs and failure visibility;
- retry and rerun procedure;
- checkpoint strategy;
- execution duration and rate-limit compliance;
- operating-cost confirmation.

### Milestone 2 Definition of Done

Milestone 2 is complete when:

- the IGDB PoC satisfies its approved criteria;
- the multi-source compatibility spike confirms extensibility;
- the Collector is refactored into clear responsibilities;
- collection is paginated, incremental, and idempotent;
- IGDB data is mapped without making IGDB the internal model;
- canonical records retain external identity and provenance;
- representative data is persisted in Neon;
- API and frontend operate with real data;
- source and reliability context are visible;
- the Worker is deployed and successfully executed in production;
- failure and rerun behavior are documented;
- automated tests pass;
- documentation is updated;
- the real-data MVP is usable end to end.

## Milestone 3 — Multi-source enrichment and reconciliation

Status: **Planned**

### Goal

Add Wikidata and Steam as planned complementary sources without replacing the IGDB vertical MVP or rewriting its structural foundations.

### 3.1 Wikidata proof of concept

Evaluate:

- authorized structured access;
- QID and external identifiers;
- aliases and canonical links;
- company, franchise, and relationship statements;
- statement-level variability and missing data;
- licensing and attribution;
- query limits and operational stability;
- reconciliation value.

### 3.2 Steam proof of concept

Evaluate only official and permitted access paths:

- AppId identity;
- names and platform-specific release information;
- developers and publishers;
- categories and features;
- supported systems and languages;
- Steam-specific signals that are legally usable;
- storage, attribution, regional, and endpoint limitations.

A game without a Steam identity remains valid in the canonical catalogue.

### 3.3 Common observation and reconciliation model

Define a source-independent comparison boundary for common concepts such as:

- source and external identity;
- observed name and aliases;
- release observations with context;
- platforms;
- developers and publishers;
- franchises or collections;
- product type;
- parent or related-product relationship;
- source-specific metadata.

Source-specific fields remain in source-specific contracts or metadata.

Reconciliation must:

- prioritize strong crossed identifiers;
- use conservative candidate generation;
- preserve distinct related products;
- keep classifications and provenance;
- avoid majority voting without semantic context;
- block automatic reconciliation for serious product-type conflicts;
- preserve manual approvals and rejections when necessary.

### 3.4 Multi-source product experience

Add:

- source convergence and divergence context;
- confidence profiles;
- source-aware field presentation;
- reconciliation status where useful;
- source links and attribution;
- clear distinction between observations, canonical values, and GMI inference.

### Milestone 3 Definition of Done

Milestone 3 is complete when:

- Wikidata and Steam PoCs are documented and approved for defined roles;
- both integrations follow the same architectural boundaries as IGDB;
- observations can be compared without erasing source context;
- reconciliation is conservative and testable;
- provenance and confidence are visible;
- the multi-source product flow works end to end.

## Later milestones

### Advanced Comparable Games exploration

This is future product scope after the first IGDB MVP; it is not silently
promoted into the current iteration.

Potential scope:

- multiple genres with all-selected matching;
- multiple platforms with OR semantics;
- themes, modes, perspectives, and keywords;
- release-period and company filters;
- advanced sorting;
- saved searches;
- performance optimization;
- richer details and analytical summaries.

Keywords remain a central product-value strategy because they let producers begin with an idea or micro-niche rather than only with a known game.

### Market metrics foundation

Priority:

- sales;
- revenue;
- estimated owners;
- downloads;
- active players;
- concurrent players;
- reviews;
- wishlists;
- other justified engagement observations.

All metrics must preserve source, meaning, period, method, and confidence.

### Market analysis and decision support

Deferred until stable data and validated questions exist:

- market signals;
- genre and platform analysis;
- launch-window context;
- source-aware reports;
- recommendations;
- forecasting and machine-learning evaluation.

## Current delivery focus

```text
Milestone 2 — IGDB vertical MVP
```

Immediate sequence:

1. run the lightweight multi-source compatibility spike;
2. approve the source-independent import boundary;
3. refactor the Collector into Worker, Jobs, client, mapper, and import service;
4. review domain and persistence changes against the approved PoC;
5. implement paginated, incremental, and idempotent IGDB ingestion;
6. validate checkpoints, checksums, storage, and safe reruns with persistence;
7. deploy and operate the Worker;
8. validate API and frontend with representative data;
9. close the real-data IGDB MVP.
