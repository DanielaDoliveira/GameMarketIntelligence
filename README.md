# Game Market Intelligence

GameMarketIntel is a decision-support platform designed to help Game Producers and small studios reduce uncertainty during early product planning.

Instead of competing with official game websites, digital stores, review platforms, or search engines, the platform organizes comparable games, trustworthy research references, and commercial evidence into a practical research workflow.

```text
Project Status: Active Development — Early Stages
Current Delivery Focus: Milestone 2 — Comparable Games Read Experience
```
```text
website: https://agreeable-ground-00051ce10.7.azurestaticapps.net/ 

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

The current milestone delivers the first complete Comparable Games read experience:

- partial-name search;
- genre and platform filters;
- OR semantics within a filter category;
- AND semantics between filter categories;
- responsive Blazor WebAssembly interface;
- game summaries and basic details;
- loading, result, empty, error, and not-found states;
- frontend-to-API integration.

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

A separate architecture exploration records a possible post-MVP recent-and-historical data-tiering strategy. That exploration is not an approved decision and does not change the current implementation.