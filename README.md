# Game Market Intelligence

GameMarketIntel is a decision-support platform designed to help Game Producers and small studios reduce uncertainty during early product planning.

Instead of competing with official game websites, digital stores, review platforms, or search engines, the platform organizes comparable games, trustworthy research references, and commercial evidence into a practical research workflow.

```text
Project Status: Active Development — Early Stages
Current Delivery Focus: Milestone 2 — Comparable Games Read Experience
Current Delivery: First responsive frontend increment completed
```

```text
Website: https://agreeable-ground-00051ce10.7.azurestaticapps.net/
```

## Product Vision

GameMarketIntel helps producers move from a game idea to an informed decision.

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

The platform does not decide whether a game should be produced. It provides transparent, contextualized, and source-aware evidence that producers can use when evaluating opportunities and discussing them with teams, publishers, and stakeholders.

## Problem

Early-stage game market research is often fragmented across search engines, official websites, storefronts, videos, reviews, news, and market reports.

Generic lists such as “best action games” frequently emphasize famous titles without showing:

- less-visible comparable games;
- commercial performance across a wider sample;
- source reliability;
- regional or platform context;
- the difference between official and estimated values;
- direct paths to the sources a producer may want to inspect next.

GameMarketIntel aims to reduce the time required to organize this first research pass without replacing the original sources.

## Current Increment

Milestone 2 remains in progress and is delivered through smaller, reviewable increments.

The first responsive frontend increment is complete and includes:

- environment-based connection between the Blazor WebAssembly frontend and the API;
- responsive application shell;
- expanded and compact desktop navigation;
- temporary mobile navigation drawer;
- Overview, Comparable Games, Data Sources, and route-not-found pages;
- contextual partial-name search in the application header;
- one optional genre filter supported by the current API contract;
- one optional platform filter supported by the current API contract;
- optional release-year filtering;
- AND semantics between supplied filter categories;
- removable active-filter chips and clear-all behavior;
- paginated search requests and pagination controls;
- search, filter, and page state preserved in the URL;
- browser back-and-forward navigation synchronized with the interface;
- branded initial loading and in-application query loading;
- no-data, no-results, request-error, and route-not-found states;
- reusable button, feedback, loading, card, image-fallback, filter, and pagination components;
- separation of Razor markup, C# code-behind, and isolated CSS where it improves maintainability;
- successful local build, automated test execution, deployment, and browser validation with the current empty production dataset.

The milestone goal has not been redefined by this delivery. Remaining Definition of Done items continue to be tracked in the implementation roadmap.

The current production database does not yet contain the representative external game dataset. Genre options, platform options, populated result cards, and multi-page behavior therefore still require final data-dependent validation after the external-source evaluation and ingestion work.

Multiple genre and platform selection remains part of the planned product evolution. It will require an API contract extension and will be implemented in a later delivery after representative data is available.

Commercial metrics are intentionally deferred to a dedicated post-Comparable-Games vertical.

## Product Direction

The planned evolution is:

1. Comparable Games discovery;
2. organized research references, including official websites and relevant storefronts;
3. commercial evidence, prioritizing sales;
4. complementary indicators such as estimated owners, downloads, active players, and concurrent players;
5. market analysis and source-aware comparisons;
6. decision-oriented market signals.

Sales are the highest-priority commercial indicator. Complementary engagement indicators may be used when sales data is unavailable, incomplete, or insufficient for the question being evaluated.

## Technologies

| Frontend | Backend | Database | Infrastructure as Code | Deployment |
|---|---|---|---|---|
| Blazor WebAssembly | ASP.NET Core REST API | PostgreSQL | Terraform | Azure Static Web Apps, Render, and Neon |

Additional technologies include:

- .NET 10;
- Entity Framework Core;
- Npgsql;
- Scalar/OpenAPI;
- xUnit;
- Shouldly;
- Testcontainers;
- GitHub Actions;
- Docker.

## Architecture

The solution separates responsibilities across:

- Domain;
- Application;
- Infrastructure;
- API;
- Shared contracts;
- Collector;
- Web;
- automated tests;
- Terraform infrastructure.

The project favors small, complete vertical increments and records exploratory ideas separately from approved architectural decisions.

## Current Database Position

Neon PostgreSQL remains the single operational database for the MVP.

The production database is currently empty while the project prepares the external-source evaluation and ingestion flow.

A separate architecture exploration records a possible post-MVP recent-and-historical data-tiering strategy. That exploration is not an approved decision and does not change the current implementation.
