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

* answer or enable a real product question;
* provide a usable end-to-end experience when appropriate;
* preserve Domain, Application, Infrastructure, API, and frontend boundaries;
* include appropriate automated tests;
* update technical and product documentation;
* pass the required Pull Request validation before entering `main`;
* remain deployable through the existing infrastructure;
* avoid premature features that have not yet been validated by real data.

## Milestone 0 — Project Foundation

Status: **Completed**

Delivered:

* .NET solution structure;
* Domain, Application, Infrastructure, API, Shared, Collector, and Web projects;
* PostgreSQL local development environment;
* EF Core and Npgsql configuration;
* initial `DataSource` and `SourceReliability` modeling;
* domain, application, API, and infrastructure test foundations;
* GitHub Actions Continuous Integration;
* Terraform infrastructure foundation;
* API deployment infrastructure;
* frontend deployment infrastructure;
* initial production and architecture documentation.

## Milestone 1 — Comparable Games Domain Foundation

Status: **Completed**

Delivered:

* `Genre` entity;
* `Platform` entity;
* `Game` entity;
* game-to-genre many-to-many relationship;
* game-to-platform many-to-many relationship;
* domain invariants and normalization;
* canonical `NormalizedName` strategy;
* unique normalized-name indexes;
* EF Core entity configurations;
* PostgreSQL migration;
* PostgreSQL database update;
* unit tests for domain rules;
* Testcontainers PostgreSQL integration infrastructure;
* persistence and relationship integration tests;
* duplicate normalized-name integration tests;
* fast CI on pushes to `develop`;
* full validation on Pull Requests targeting `main`;
* required `Build and Test` status check.

## Milestone 2 — Comparable Games Read Experience

Status: **In Progress**

### Goal

Deliver the first complete Comparable Games read experience, including searchable and filterable API endpoints, a responsive frontend, and integration between the Blazor WebAssembly application and the API.

The increment should allow a game producer or small-studio decision-maker to discover comparable games through name, genre, platform, and release-year criteria.

### Current Backend Progress

Completed:

* `SearchGamesQuery`;
* `SearchGamesResult`;
* `GameSearchItem`;
* `GameSearchCategory`;
* `IGameSearchRepository`;
* `IGameSearchService`;
* `GameSearchService`;
* PostgreSQL `GameSearchRepository`;
* partial case-insensitive game-name search;
* optional genre filtering;
* optional platform filtering;
* optional release-year filtering;
* AND combination between different filter categories;
* alphabetical ordering by game name;
* paginated results;
* pagination metadata;
* `GET /api/games`;
* `GET /api/games/{id:guid}`;
* game-details Application service;
* game-details projection with genres and platforms;
* missing-game handling through `NotFoundException`;
* OpenAPI and Scalar endpoint documentation;
* FluentValidation for search parameters;
* standardized HTTP `400 Bad Request` validation responses;
* `GameMarketIntel.Exceptions` class library;
* reusable `NotFoundException` and `ConflictException`;
* centralized global exception handling through ASP.NET Core `IExceptionHandler`;
* standardized `ProblemDetails` and `ValidationProblemDetails` responses;
* validation failures mapped to HTTP `400`;
* missing resources mapped to HTTP `404`;
* conflicts mapped to HTTP `409`;
* unexpected failures mapped to HTTP `500`;
* Application tests for query validation and game-details behavior;
* API tests for exception-to-response mappings;
* API HTTP test for game retrieval by ID;
* PostgreSQL integration tests for filtering and pagination;
* 97 automated tests passing across the solution.

Pending:

* `GET /api/genres`;
* `GET /api/platforms`;
* responsive frontend implementation;
* Blazor WebAssembly integration with the API.

### Backend Scope

#### Search Contracts

The implemented Comparable Games search uses:

* `SearchGamesQuery`;
* `SearchGamesResult`;
* `GameSearchItem`;
* `GameSearchCategory`.

The search response exposes only read contracts and does not return domain entities directly.

#### Application Layer

Implemented:

* repository abstraction for game searches;
* game-search service;
* game-details service;
* validation before repository execution;
* partial game-name search;
* genre filtering;
* platform filtering;
* release-year filtering;
* entity-to-contract projection;
* game-details projection with genres and platforms;
* pagination metadata;
* missing-game handling through `NotFoundException`.

Pending:

* genre listing query;
* platform listing query.

#### Infrastructure Layer

Implemented:

* PostgreSQL search repository;
* EF Core filtered queries;
* read-only query behavior;
* case-insensitive PostgreSQL search through `ILike`;
* many-to-many relationship filtering;
* alphabetical result ordering;
* pagination through `Skip` and `Take`;
* direct projection to search DTOs;
* PostgreSQL integration tests.

#### API Endpoints

Implemented:

* `GET /api/games`;
* `GET /api/games/{id:guid}`;
* OpenAPI documentation;
* Scalar endpoint documentation;
* standardized validation responses;
* standardized not-found responses;
* centralized exception-to-HTTP mapping.

Pending:

* `GET /api/genres`;
* `GET /api/platforms`.

### Current Games Search Contract

The current endpoint supports:

* optional partial game-name search;
* one optional genre identifier;
* one optional platform identifier;
* one optional release year;
* page number;
* page size.

Current query parameters:

```text
search
genreId
platformId
releaseYear
page
pageSize
```

Pagination rules:

* `page` must be greater than or equal to `1`;
* `pageSize` must be between `1` and `100`;
* the default page is `1`;
* the default page size is `20`.

Release-year rules:

* `releaseYear` is optional;
* when provided, it cannot be greater than the current year.

### Query Combination Rules

Different supplied filter categories use AND semantics.

```text
Name condition
AND
Genre condition
AND
Platform condition
AND
Release-year condition
```

Example:

```text
Name contains "Hades"
AND
Genre is Action
AND
Platform is PC
AND
Release year is 2020
```

The current API contract accepts one optional genre and one optional platform per request.

Support for multiple genres or multiple platforms in the same request is deferred until the first frontend query experience is validated.

A future contract may support:

```text
Genre is Action OR Roguelike
AND
Platform is PC OR Nintendo Switch
```

Support for requiring all selected genres is also deferred.

### Validation

The Comparable Games search uses FluentValidation.

Implemented rules:

```text
Page >= 1
PageSize >= 1
PageSize <= 100
ReleaseYear <= current year
```

Validation is executed by the Application service before the repository is called.

A failed validation raises a FluentValidation `ValidationException`.

The centralized API exception handler translates validation failures into standardized HTTP `400 Bad Request` responses using `ValidationProblemDetails`.

### Global Exception Handling

Centralized exception handling is implemented through ASP.NET Core `IExceptionHandler`.

The API uses standardized `ProblemDetails` responses and maps:

* FluentValidation failures to HTTP `400`;
* missing resources to HTTP `404`;
* resource conflicts to HTTP `409`;
* unexpected failures to HTTP `500`.

Unexpected responses do not expose exception messages, stack traces, database information, or other internal implementation details.

Expected application failures are logged as warnings. Unexpected failures are logged as errors.

HTTP-specific behavior remains in `GameMarketIntel.Api`.

The `GameMarketIntel.Exceptions` project remains independent from:

* ASP.NET Core;
* HTTP status codes;
* `ProblemDetails`;
* API middleware;
* API logging concerns.

### Frontend Scope

#### Application Shell

* responsive application shell;
* persistent and collapsible desktop sidebar;
* temporary mobile navigation drawer;
* visible menu button;
* header with centered contextual search;
* lightweight responsive footer;
* consistent shell across result, detail, loading, error, and not-found pages.

#### Comparable Games Experience

The first frontend implementation must support the current API contract:

* contextual partial-name search;
* one optional genre filter;
* one optional platform filter;
* optional release-year filter when included in the interface;
* active-filter presentation;
* filter removal;
* clear-all-filters action;
* result count;
* vertically stacked mobile results;
* responsive desktop result composition;
* connection between Blazor WebAssembly and the API.

Multiple genre and platform selections remain a planned frontend evolution and require an API contract extension before implementation.

#### Experience States

* initial Blazor application loading;
* query loading;
* results available;
* no matching results;
* no data available;
* validation or request error;
* route not found.

The initial application loading message is:

> Preparing your market research workspace...

The query-loading message is:

> Searching comparable games...

### Styling Scope

The first frontend implementation will use:

* standard CSS;
* semantic CSS custom properties;
* Blazor CSS isolation;
* CSS Grid;
* Flexbox;
* mobile-first media queries;
* reduced-motion support.

Tailwind CSS is not part of this milestone.

### Result Content

Each game result currently supports:

* game identifier;
* game cover or image URL;
* game name;
* description;
* release date;
* genres;
* platforms.

The frontend result card should initially expose:

* cover or fallback image;
* game name;
* release year or date;
* genres;
* platforms;
* details affordance when the details destination exists.

Long descriptions should not appear in result cards.

### Basic Game Details

The first details destination should expose:

* game cover or image;
* game name;
* release year or date;
* genres;
* platforms;
* short description when available.

Advanced analytical details and commercial metrics are deferred.

Sales and complementary engagement indicators belong to a dedicated future Market Metrics vertical and must not expand the scope of Milestone 2.

### Automated Tests

Current solution status:

```text
97 automated tests passing
```

Implemented coverage includes:

* domain invariants;
* normalized-name behavior;
* persistence configurations;
* many-to-many relationships;
* duplicate prevention;
* search-service behavior;
* query validation;
* partial-name search;
* case-insensitive PostgreSQL search;
* genre filtering;
* platform filtering;
* release-year filtering;
* combined filters;
* pagination;
* empty search results;
* contract projection;
* game-details mapping;
* missing-game behavior;
* exception-to-HTTP mappings;
* validation `ProblemDetails`;
* not-found `ProblemDetails`;
* conflict `ProblemDetails`;
* unexpected-error protection;
* game-details HTTP endpoint.

Remaining test areas for Milestone 2:

* genre listing;
* platform listing;
* frontend component and behavior tests where practical.

### Out of Scope for This Milestone

* manual creation endpoints;
* update endpoints;
* deletion endpoints;
* external API ingestion;
* source-specific identifier mapping;
* publisher filtering;
* developer filtering;
* multiple genre values in one search request;
* multiple platform values in one search request;
* advanced genre matching modes;
* sorting by market indicators;
* saved searches;
* advanced pagination optimization;
* market metrics;
* analytical charts;
* similarity scoring;
* recommendation models;
* authentication;
* dark theme.

### Definition of Done

The milestone is complete when:

* read endpoints return contracts instead of domain entities;
* games include their associated genres and platforms;
* missing records produce appropriate API responses;
* users can search games by partial name;
* users can filter by the supported genre, platform, and release-year criteria;
* filters follow the documented AND semantics;
* query validation returns standardized HTTP `400` responses;
* repository and application boundaries are preserved;
* the Blazor frontend consumes the API;
* the responsive application shell is implemented;
* desktop and mobile navigation work;
* the search appears in the header without being duplicated in the page body;
* active filters can be removed;
* all filters can be cleared;
* result count is displayed;
* results render correctly on mobile and desktop;
* initial loading, query loading, no-results, no-data, request-error, and route-not-found states are implemented;
* a basic game-details destination is available;
* keyboard navigation and reduced-motion behavior are verified;
* automated tests pass locally and in Pull Request validation;
* OpenAPI and Scalar documentation are updated;
* frontend, API, and roadmap documentation are updated;
* the deployed frontend can communicate with the deployed API;
* the increment is ready to merge into `main`.

## Milestone 3 — External Data Source Evaluation

Status: **Planned**

### Goal

Select and validate the first real source used to populate Comparable Games data.

### Planned Activities

* compare available public APIs and datasets;
* review licensing and attribution requirements;
* evaluate identifier stability;
* evaluate rate limits;
* inspect platform and genre taxonomies;
* document known data limitations;
* determine collection frequency;
* estimate storage impact;
* define the first source-specific mapping strategy;
* evaluate image and media usage rights;
* assess whether the selected source supports the existing query experience.

### Expected Output

* selected initial data source;
* documented source reliability;
* documented licensing and attribution requirements;
* sample dataset;
* external identifier model proposal;
* ingestion risk assessment;
* storage-impact estimate;
* source-specific taxonomy notes.

## Milestone 4 — Comparable Games Ingestion

Status: **Planned**

### Goal

Collect and persist the first real Comparable Games dataset.

### Potential Scope

* Collector integration;
* source client;
* source-specific DTOs;
* external game identifiers;
* external platform references;
* external genre references when necessary;
* normalized entity resolution;
* duplicate prevention;
* collection timestamps;
* source association;
* idempotent import behavior;
* collection execution through GitHub Actions;
* ingestion integration tests;
* collection failure reporting;
* basic data-quality validation.

The final scope depends on the source selected during Milestone 3.

## Milestone 5 — Comparable Games Advanced Exploration

Status: **Planned**

### Goal

Expand the validated Comparable Games experience with more specialized research controls and analytical context.

### Potential Scope

* support for multiple genre filters;
* support for multiple platform filters;
* OR semantics inside the same filter category;
* optional all-selected-genres matching;
* release-period filtering;
* publisher filtering when available;
* developer filtering when available;
* sorting;
* pagination optimization;
* URL-preserved filter state;
* saved searches;
* query-performance review;
* source and reliability presentation;
* contextual analytical summaries;
* genre-distribution charts;
* platform-distribution charts;
* release-timeline visualization;
* richer desktop research layout;
* expanded game-details experience;
* organized external research references;
* official game website links when available;
* storefront references when licensing and source rules allow;
* official trailer or gameplay references when appropriate;
* producer-oriented paths for deeper manual research.

This milestone should be refined only after the first query experience and initial real dataset have been validated.

## Milestone 6 — Market Metrics Foundation

Status: **Future — High Product Priority**

### Goal

Establish the source-aware commercial evidence model required to evaluate how comparable games performed in the market.

Sales are the highest-priority metric family.

### Priority Metric Scope

Primary commercial indicators:

* units sold;
* revenue.

Complementary indicators, when relevant or when sales data is unavailable:

* estimated owners;
* downloads;
* active players;
* peak concurrent players;
* reviews;
* wishlists;
* followers;
* other engagement observations.

### Potential Scope

* metric snapshots;
* historical observations;
* official-versus-estimated classification;
* metric meaning and units;
* source association;
* reliability context;
* collection timestamps;
* platform and regional dimensions when available;
* storage-retention rules;
* data-quality notes;
* commercial evidence presentation;
* initial market-performance comparison.

This milestone must remain source-aware and transparent about the difference between official, estimated, inferred, and unavailable information.

## Milestone 7 — Market Analysis and Decision Support

Status: **Future**

### Goal

Transform validated Comparable Games and Market Metrics data into higher-level production and market insights.

### Potential Scope

* market signals;
* genre opportunity indicators;
* platform comparisons;
* launch-window context;
* trend summaries;
* source-aware confidence presentation;
* research reports;
* decision-support summaries;
* scenario comparison;
* recommendation experiments;
* forecasting and machine-learning evaluation.

Machine learning must not be introduced before the project has:

* stable datasets;
* reliable historical observations;
* documented metric meanings;
* sufficient data volume;
* a validated product question that benefits from prediction.

## Current Delivery Focus

The current delivery focus is:

```text
Complete remaining read endpoints
    ↓
Implement genre listing
    ↓
Implement platform listing
    ↓
Build responsive frontend
    ↓
Connect Blazor WebAssembly to the API
```

The immediate technical follow-up is the implementation of the genre and platform read endpoints.

The next product-facing delivery is the responsive Comparable Games frontend integrated with the deployed API.