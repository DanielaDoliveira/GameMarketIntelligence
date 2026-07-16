# Implementation Roadmap

## Purpose

This document provides a milestone-level view of the GameMarketIntel implementation.

It records completed increments, the current delivery focus, and the expected evolution of the product without replacing detailed domain, architecture, requirements, design, or increment-specific documentation.

The roadmap may change as real data sources, infrastructure constraints, product validation, and frontend implementation provide new information.

## Product Direction

GameMarketIntel is a decision-support platform for Game Producers and small studios.

The product organizes comparable games, research references, commercial evidence, and future analytical context into a workflow that helps reduce uncertainty during early product planning.

```text
Game idea
    ↓
Comparable games discovery
    ↓
Research references
    ↓
Commercial evidence
    ↓
Market analysis
    ↓
Stakeholder decision support
```

The platform does not replace official websites, storefronts, review platforms, videos, news, or market reports.

It should help the producer identify what deserves deeper investigation, locate relevant sources more efficiently, and organize enough initial evidence to support the next decision.

Sales are the highest-priority future commercial indicator.

Downloads, estimated owners, active players, concurrent players, reviews, wishlists, and similar observations may complement sales when they are relevant or when direct sales data is unavailable.

The detailed product vision is maintained in:

```text
docs/product/product-vision.md
```

## Delivery Principles

GameMarketIntel is developed through small, complete vertical increments.

Each delivery should:

- answer or enable a real product question;
- provide a usable end-to-end experience when appropriate;
- preserve domain, application, infrastructure, API, and frontend boundaries;
- include appropriate automated tests;
- update technical and product documentation;
- pass the required Pull Request validation before entering `main`;
- remain deployable through the existing infrastructure;
- avoid premature features that have not yet been validated by real data.

## Milestone 0 — Project Foundation

Status: **Completed**

Delivered:

- .NET solution structure;
- Domain, Application, Infrastructure, API, Shared, Collector, and Web projects;
- PostgreSQL local development environment;
- EF Core and Npgsql configuration;
- initial `DataSource` and `SourceReliability` modeling;
- domain, application, API, and infrastructure test foundations;
- GitHub Actions Continuous Integration;
- Terraform infrastructure foundation;
- API deployment infrastructure;
- frontend deployment infrastructure;
- initial production and architecture documentation.

## Milestone 1 — Comparable Games Domain Foundation

Status: **Completed**

Delivered:

- `Genre` entity;
- `Platform` entity;
- `Game` entity;
- game-to-genre many-to-many relationship;
- game-to-platform many-to-many relationship;
- domain invariants and normalization;
- canonical `NormalizedName` strategy;
- unique normalized-name indexes;
- EF Core entity configurations;
- PostgreSQL migration;
- PostgreSQL database update;
- unit tests for domain rules;
- Testcontainers PostgreSQL integration infrastructure;
- persistence and relationship integration tests;
- duplicate normalized-name integration tests;
- fast CI on pushes to `develop`;
- full validation on Pull Requests targeting `main`;
- required `Build and Test` status check.

## Milestone 2 — Comparable Games Read Experience

Status: **Next**

### Goal

Deliver the first complete Comparable Games read experience, including searchable and filterable API endpoints, a responsive frontend, and integration between the Blazor WebAssembly application and the API.

The increment should allow a game producer or small-studio decision-maker to discover comparable games through name, genre, and platform criteria.

### Backend Scope

#### Shared Read Contracts

- `GenreDetails`;
- `PlatformDetails`;
- `GameSummary`;
- `GameDetails`;
- query and filter contracts when necessary.

#### Application Layer

- repository interfaces for read operations;
- query use cases for games;
- query use cases for genres;
- query use cases for platforms;
- partial game-name search;
- genre filtering;
- platform filtering;
- entity-to-contract mapping;
- missing-record handling.

#### Infrastructure Layer

- repository implementations;
- filtered EF Core queries;
- relationship loading for genres and platforms;
- duplicate-safe and read-only query behavior;
- query integration tests.

#### API Endpoints

- `GET /api/games`;
- `GET /api/games/{id}`;
- `GET /api/genres`;
- `GET /api/platforms`;
- OpenAPI documentation;
- Scalar endpoint documentation.

The games endpoint should support query parameters for:

- partial game name;
- one or more genres;
- one or more platforms.

### Query Combination Rules

Different filter categories use AND semantics.

```text
Name condition
AND
Genre condition
AND
Platform condition
```

Multiple values inside the same category use OR semantics.

Example:

```text
Name contains "Hades"
AND
Genre is Action OR Roguelike
AND
Platform is PC OR Nintendo Switch
```

Support for requiring all selected genres is deferred.

### Frontend Scope

#### Application Shell

- responsive application shell;
- persistent and collapsible desktop sidebar;
- temporary mobile navigation drawer;
- visible menu button;
- header with centered contextual search;
- lightweight responsive footer;
- consistent shell across result, detail, loading, error, and not-found pages.

#### Comparable Games Experience

- contextual partial-name search in the header;
- genre filter controls;
- platform filter controls;
- removable active-filter chips;
- clear-all-filters action;
- result count;
- vertically stacked mobile results;
- responsive desktop result composition;
- basic game-details destination;
- connection between the Blazor WebAssembly frontend and the API.

#### Experience States

- initial Blazor application loading;
- query loading;
- results available;
- no matching results;
- no data available;
- request error;
- route not found.

The initial application loading message is:

> Preparing your market research workspace...

The query-loading message is:

> Searching comparable games...

### Styling Scope

The first frontend implementation will use:

- standard CSS;
- semantic CSS custom properties;
- Blazor CSS isolation;
- CSS Grid;
- Flexbox;
- mobile-first media queries;
- reduced-motion support.

Tailwind CSS is not part of this milestone.

### Result Content

Each game result should initially expose:

- game cover or image;
- game name;
- release year or date;
- genres;
- platforms;
- details affordance.

Long descriptions should not appear in result cards.

### Basic Game Details

The first details destination should expose:

- game cover or image;
- game name;
- release year or date;
- genres;
- platforms;
- short description when available.

Advanced analytical details and commercial metrics are deferred.

Sales and complementary engagement indicators belong to a dedicated future market-metrics vertical and must not expand the scope of Milestone 2.

### Automated Tests

Planned coverage:

- Application query tests;
- repository filtering integration tests;
- API endpoint tests;
- missing-record tests;
- partial-name search tests;
- genre-filter tests;
- platform-filter tests;
- combined-filter tests;
- contract-mapping tests;
- frontend component or behavior tests where practical.

### Out of Scope for This Milestone

- manual creation endpoints;
- update endpoints;
- deletion endpoints;
- external API ingestion;
- source-specific identifier mapping;
- release-period filtering;
- publisher filtering;
- developer filtering;
- advanced genre matching modes;
- sorting by market indicators;
- saved searches;
- advanced pagination optimization;
- market metrics;
- analytical charts;
- similarity scoring;
- recommendation models;
- authentication;
- dark theme.

### Definition of Done

The milestone is complete when:

- read endpoints return Shared contracts instead of domain entities;
- games include their associated genres and platforms;
- missing records produce an appropriate API response;
- users can search games by partial name;
- users can filter games by one or more genres;
- users can filter games by one or more platforms;
- filters follow the documented AND and OR semantics;
- repository and application boundaries are preserved;
- the Blazor frontend consumes the API;
- the responsive application shell is implemented;
- desktop and mobile navigation work;
- the search appears in the header without being duplicated in the page body;
- active filters can be removed individually;
- all filters can be cleared;
- result count is displayed;
- results render correctly on mobile and desktop;
- initial loading, query loading, no-results, no-data, request-error, and route-not-found states are implemented;
- a basic game-details destination is available;
- keyboard navigation and reduced-motion behavior are verified;
- automated tests pass locally and in Pull Request validation;
- OpenAPI and Scalar documentation are updated;
- frontend, API, and roadmap documentation are updated;
- the deployed frontend can communicate with the deployed API;
- the increment is ready to merge into `main`.

## Milestone 3 — External Data Source Evaluation

Status: **Planned**

### Goal

Select and validate the first real source used to populate Comparable Games data.

### Planned Activities

- compare available public APIs and datasets;
- review licensing and attribution requirements;
- evaluate identifier stability;
- evaluate rate limits;
- inspect platform and genre taxonomies;
- document known data limitations;
- determine collection frequency;
- estimate storage impact;
- define the first source-specific mapping strategy;
- evaluate image and media usage rights;
- assess whether the selected source supports the existing query experience.

### Expected Output

- selected initial data source;
- documented source reliability;
- documented licensing and attribution requirements;
- sample dataset;
- external identifier model proposal;
- ingestion risk assessment;
- storage-impact estimate;
- source-specific taxonomy notes.

## Milestone 4 — Comparable Games Ingestion

Status: **Planned**

### Goal

Collect and persist the first real Comparable Games dataset.

### Potential Scope

- Collector integration;
- source client;
- source-specific DTOs;
- external game identifiers;
- external platform references;
- external genre references when necessary;
- normalized entity resolution;
- duplicate prevention;
- collection timestamps;
- source association;
- idempotent import behavior;
- collection execution through GitHub Actions;
- ingestion integration tests;
- collection failure reporting;
- basic data-quality validation.

The final scope depends on the source selected during Milestone 3.

## Milestone 5 — Comparable Games Advanced Exploration

Status: **Planned**

### Goal

Expand the validated Comparable Games experience with more specialized research controls and analytical context.

### Potential Scope

- release-period filtering;
- publisher filtering, when available;
- developer filtering, when available;
- optional all-selected-genres matching;
- sorting;
- pagination optimization;
- URL-preserved filter state;
- saved searches;
- query-performance review;
- source and reliability presentation;
- contextual analytical summaries;
- genre-distribution charts;
- platform-distribution charts;
- release-timeline visualization;
- richer desktop research layout;
- expanded game-details experience;
- organized external research references;
- official game website links when available;
- storefront references such as Steam when licensing and source rules allow;
- official trailer or gameplay references when appropriate;
- producer-oriented paths for deeper manual research.

This milestone should be refined only after the first query experience and the initial real dataset have been validated.

## Milestone 6 — Market Metrics Foundation

Status: **Future — High Product Priority**

### Goal

Establish the source-aware commercial evidence model required to evaluate how comparable games performed in the market.

Sales are the highest-priority metric family.

### Priority Metric Scope

Primary commercial indicators:

- units sold;
- revenue.

Complementary indicators, when relevant or when sales data is unavailable:

- estimated owners;
- downloads;
- active players;
- peak concurrent players;
- reviews;
- wishlists;
- followers;
- other engagement observations.

### Potential Scope

- metric snapshots;
- historical observations;
- official-versus-estimated classification;
- metric meaning and units;
- source association;
- source reliability;
- platform scope;
- regional scope;
- observation period;
- publication and collection dates;
- known limitations;
- data freshness indicators;
- market-metric read contracts;
- commercial evidence presentation.

This milestone must preserve the distinction between sales, downloads, owners, players, and engagement.

It must be refined only after the Comparable Games dataset, source availability, licensing constraints, storage impact, and product usage patterns have been validated.

## Milestone 7 — Market Analysis

Status: **Future**

### Goal

Transform validated comparable-game and market-metric data into contextual analysis without presenting uncertain conclusions as facts.

### Potential Scope

- platform-level comparisons;
- regional comparisons;
- sales distributions;
- comparison between famous and less-visible games;
- genre-saturation indicators;
- launch-window analysis;
- source-aware aggregations;
- commercial-performance summaries;
- analytical comparison contracts;
- charts and trend visualizations;
- survivorship-bias-aware presentation;
- confidence and data-coverage communication.

## Milestone 8 — Market Signals

Status: **Future**

### Goal

Provide decision-oriented signals derived from validated evidence while keeping the producer responsible for the final decision.

### Potential Scope

- market-signal foundations;
- opportunity indicators;
- similarity-based context;
- evidence coverage;
- confidence indicators;
- producer-oriented summaries;
- explainable signal inputs;
- warnings for insufficient or conflicting evidence.

Predictive models and automated recommendations remain deferred until sufficient data quality, product validation, and explainability requirements are demonstrated.

## Deferred Product Areas

The following areas remain intentionally deferred:

- predictive machine learning;
- opportunity scoring;
- recommendation models;
- regional release modeling;
- field-level provenance;
- historical raw payload storage;
- large binary assets;
- subscription-market analysis;
- platform-market analysis beyond validated metric sources;
- advanced market signals;
- authentication and user accounts;
- collaborative workspaces;
- paid features.

They may become separate future verticals after the Comparable Games workflow is validated.

## Architectural Explorations

Exploratory architecture documents preserve ideas that may become useful after the MVP without turning them into current commitments.

The following exploration is recorded separately:

```text
docs/architecture/explorations/hybrid-database-data-tiering-exploration.md
```

It examines a possible future strategy for retaining recent release-date-ordered data in Neon and older data in a historical relational store.

The exploration is not an approved architecture decision and does not change the MVP database, deployment model, or implementation sequence.

## Current Delivery Focus

The current focus is:

```text
Milestone 2 — Comparable Games Read Experience
```

The current increment combines:

```text
Searchable API
+
genre and platform filters
+
responsive frontend
+
API integration
+
complete feedback states
```

## Immediate Implementation Sequence

1. finalize the Comparable Games read contracts;
2. define query parameters for partial name, genres, and platforms;
3. define Application repository interfaces;
4. implement Application query use cases;
5. implement Infrastructure repository filtering;
6. add repository and Application tests;
7. expose game, genre, and platform read endpoints;
8. add API endpoint tests;
9. validate OpenAPI and Scalar documentation;
10. implement the responsive application shell;
11. implement the desktop sidebar and mobile navigation drawer;
12. implement the header with centered contextual search;
13. implement the lightweight footer;
14. implement genre and platform filter controls;
15. implement active-filter chips and clear-all behavior;
16. implement mobile and desktop result layouts;
17. implement initial and query loading states;
18. implement no-results, no-data, request-error, and route-not-found states;
19. implement the basic game-details destination;
20. connect the Blazor WebAssembly frontend to the API;
21. validate the deployed frontend-to-API connection;
22. validate mobile and desktop behavior;
23. verify accessibility and reduced-motion behavior;
24. update technical, UX, and roadmap documentation;
25. create the next Pull Request from `develop` to `main`.