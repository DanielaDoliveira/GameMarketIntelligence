# Hybrid Database Data Tiering Exploration

## Status

**Exploratory**

This document records a possible post-MVP strategy for mitigating future relational database storage limitations.

It does not represent an approved architectural decision.

It must not affect the current MVP implementation, deployment model, database provider, or delivery sequence.

## Purpose

GameMarketIntel currently uses Neon PostgreSQL as its operational relational database.

This database is expected to support the MVP and the first Comparable Games read experience.

However, the platform may eventually store a substantially larger volume of comparable games, research references, market metrics, sales observations, regional observations, source evidence, and historical snapshots.

As these datasets grow, the available storage capacity may become insufficient.

This document preserves one possible mitigation strategy so it can be evaluated later without being mixed with the current MVP architecture.

## Current Position

For the MVP:

```text
Blazor WebAssembly
        ↓
ASP.NET Core API
        ↓
Neon PostgreSQL
```

Neon PostgreSQL remains the single operational database.

No secondary database, cross-database query, automated archival process, or data-tiering worker is planned for the MVP.

## Problem Being Explored

Future storage growth may create risks such as:

- the primary database reaching its storage limit;
- historical observations consuming most of the available capacity;
- older data competing for space with recent data;
- retaining all collected evidence becoming financially or operationally impractical;
- a complete migration becoming necessary under time pressure.

The objective is to evaluate whether recent and historical data could eventually be stored in different relational databases.

## Proposed Concept

The possible future architecture would use two storage tiers.

### Recent Data Tier

Neon PostgreSQL would retain more recently released games.

Comparable Games results would be ordered by release date in descending order, causing recent records to appear first.

```text
Most recently released games
        ↓
Neon PostgreSQL
```

### Historical Data Tier

A second relational database would retain older games after they become eligible for transfer.

Azure SQL may be evaluated as one possible historical store because of its greater potential capacity.

Azure SQL is not approved by this document and remains only one candidate.

```text
Older released games
        ↓
Historical relational database
```

## Release-Date Boundary

The separation would be based primarily on release date.

Possible policies include:

- retaining games released after a configured date in Neon;
- retaining the newest configured number of games in Neon;
- moving records when Neon reaches a storage threshold;
- combining release date with storage and query-frequency criteria.

The final policy must be based on measured post-MVP data volume and usage.

The boundary must be explicit, deterministic, configurable, and auditable.

## Search Ordering

Comparable Games searches would return games from newest to oldest.

The first pages would normally be served by Neon.

Historical results would be required only when the ordered matching Neon result range had been exhausted.

The transition may occur on page 2, page 10, or another page depending on:

- search text;
- selected genres;
- selected platforms;
- page size;
- number of matching recent games;
- configured release-date boundary.

The implementation must not assume a fixed page number.

## Historical Database Preparation

A previous Azure SQL proof of concept observed behavior in which an initial request failed while a later request succeeded.

A future proof of concept may evaluate whether the API can initiate a silent preparation request before historical results are needed.

```text
User requests Comparable Games
        ↓
API queries Neon
        ↓
API returns recent results
        ↓
API initiates historical database preparation
        ↓
User continues browsing recent results
        ↓
API queries historical data when required
```

The preparation request would not return data to the frontend.

Its purpose would be to initiate or verify historical-store availability before the user reaches older records.

Preparation may begin when:

- the first Comparable Games query is executed;
- the result set may reach the historical tier;
- remaining matching Neon records fall below a configured threshold;
- a continuation cursor approaches the storage boundary.

Preparation must not delay the current Neon response.

It also cannot be treated as a guarantee that the historical database will be ready.

## User Experience Boundary

The frontend must not expose infrastructure details such as provider names, database wake-up, server resume, transfer workers, or storage tiers.

From the user’s perspective, Comparable Games must remain one continuous search experience.

If older data is temporarily unavailable, the interface may present a controlled state such as:

> Preparing older market records...

The system should avoid exposing a generic internal-server error when a specific recoverable state can be identified.

It must not falsely claim that data is available before it can be returned.

## Cross-Database Pagination

The frontend must not decide which database to query.

The API would coordinate pagination across both stores.

A future opaque continuation cursor may need to preserve:

- active filters;
- release-date ordering;
- last returned release date;
- deterministic secondary key;
- active storage tier;
- query snapshot.

Release date alone is insufficient for stable pagination because multiple games may share the same date.

A deterministic order may use:

```text
ReleaseDate descending
GameId ascending
```

Equivalent ordering behavior must be validated in both database providers.

## Scheduled Data Transfer

A scheduled worker, collector operation, or other background process may move eligible records from Neon to the historical store.

```text
Identify eligible records
        ↓
Create transfer batch
        ↓
Copy records and relationships
        ↓
Validate destination data
        ↓
Confirm referential integrity
        ↓
Mark batch as completed
        ↓
Delete validated source records
```

Deletion from Neon must occur only after successful destination validation.

The worker must be:

- idempotent;
- recoverable;
- observable;
- auditable;
- safe to retry;
- protected against partial deletion;
- protected against duplicate historical records.

The execution mechanism remains intentionally undefined.

## Transfer and Query Consistency

The architecture must prevent:

- duplicate results;
- missing results;
- unstable ordering;
- inconsistent page transitions;
- records becoming unavailable during transfer;
- broken genre or platform relationships;
- deletion after incomplete validation.

A game should have one authoritative active storage tier after a transfer completes.

Temporary duplication may exist internally during copying and validation, but it must not be exposed through the API.

## Provider Differences

Neon PostgreSQL and Azure SQL use different database engines.

Equivalent logical tables do not imply identical physical schemas.

Differences may include:

- EF Core providers;
- migrations;
- SQL syntax;
- indexes;
- text comparison;
- case sensitivity;
- date and time types;
- decimal precision;
- query plans;
- pagination behavior;
- transaction semantics.

Separate provider-specific configurations and migrations may be required.

## Backup and Transfer Format

A native PostgreSQL backup should not be assumed to be directly restorable into SQL Server.

The transfer would likely use a logical representation such as:

- application DTOs;
- batched projections;
- JSON;
- CSV;
- another structured interchange format.

The selected format must preserve identifiers, dates, nullability, numeric precision, relationships, source metadata, and audit information.

## Potential Benefits

This approach may:

- extend effective relational storage capacity;
- preserve recent data in the current operational store;
- align storage tiers with release-date ordering;
- delay a complete database migration;
- reduce pressure on the primary database;
- protect the user experience from a known historical-store initialization behavior.

## Main Risks

This approach may introduce:

- two database providers;
- separate migration strategies;
- cross-database pagination;
- transfer orchestration;
- additional failure modes;
- data-consistency risks;
- higher integration-test requirements;
- increased operational complexity;
- potential infrastructure costs;
- different query behavior between providers.

These risks may still be preferable to an unmitigated storage limit if real usage demonstrates sufficient product value.

The purpose of a future proof of concept is to determine which risk set is more acceptable.

## Alternatives to Evaluate

Before approving the strategy, alternatives must also be considered:

- upgrading the existing PostgreSQL plan;
- moving completely to another PostgreSQL provider;
- performing a full migration to a larger service;
- storing historical raw data in object storage;
- retaining aggregated historical metrics only;
- reducing snapshot frequency;
- applying retention policies;
- separating raw evidence from query-ready data.

## Validation Required

This exploration must not become an architectural decision until a post-MVP proof of concept validates:

1. actual storage growth;
2. current provider capacity and pricing;
3. historical query frequency;
4. current Azure SQL availability behavior;
5. preparation and retry behavior;
6. provider-specific migrations;
7. logical schema equivalence;
8. safe relationship transfer;
9. idempotent worker execution;
10. validation before deletion;
11. deterministic cross-store pagination;
12. filtered search across the release-date boundary;
13. total-count behavior;
14. recovery after partial failure;
15. operational monitoring;
16. total cost and maintenance burden.

## Decision Boundary

This exploration remains deferred until:

- the MVP is complete;
- Comparable Games search is validated;
- a real external data source is selected;
- representative dataset size is measured;
- market-metric ingestion is defined;
- storage growth becomes measurable;
- the current limit becomes a demonstrated risk.

Until then:

```text
Neon PostgreSQL remains the single operational database.
```

## Documentation Classification

This document is an exploration.

It is not:

- a roadmap commitment;
- an Architecture Decision Record;
- an approved infrastructure plan;
- an MVP requirement;
- an implementation specification.

If the approach is validated later, the decision should be recorded in a new ADR.

If rejected, this document should remain as historical evidence of the evaluated alternative.