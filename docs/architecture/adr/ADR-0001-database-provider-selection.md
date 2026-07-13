# ADR-0001 — Managed Database Provider Selection

## Status

Proposed

## Context

GameMarketIntel requires a managed relational database for its production environment.

The database will support:

- persistence of collected market data;
- storage of data source metadata;
- historical snapshots;
- API queries;
- scheduled data collection jobs;
- future analytical workloads.

The project is intended to remain publicly available with no mandatory operating cost during its initial portfolio phase.

The initial architecture proposed Neon PostgreSQL as the managed database provider.

However, this choice has not yet been validated through a structured comparison or technical proof of concept.

The evaluation was initially limited to managed PostgreSQL providers.

During the research phase, Azure SQL Database was added as a candidate because it provides:

- a relational database model;
- official Entity Framework Core support;
- official Terraform support;
- substantially larger Free-offer storage capacity;
- strong compatibility with the .NET ecosystem.

The final decision should therefore evaluate managed relational database solutions rather than requiring a specific database engine in advance.

Before provisioning the production database, the candidate providers will be evaluated against the project requirements, operational constraints, cost objectives, security requirements, and expected long-term data growth.

## Decision Drivers

The selected provider should be evaluated according to the following criteria:

- sustainable Free offering;
- low risk of unexpected billing;
- compatibility with the GameMarketIntel relational model;
- Entity Framework Core compatibility;
- migration effort from the current persistence implementation;
- storage capacity;
- compute capacity;
- connection limits and pooling behavior;
- behavior after periods of inactivity;
- backup and recovery capabilities;
- Terraform support;
- maturity of the Terraform provider;
- provider ownership and dependency trust;
- integration with Render;
- secure connection and credential management;
- operational complexity;
- support for scheduled data collection jobs;
- support for historical market-data growth;
- vendor lock-in;
- application portability;
- suitability for a public portfolio application.

## Considered Options

The evaluation will consider:

- Neon PostgreSQL;
- Supabase PostgreSQL;
- Aiven PostgreSQL;
- Azure SQL Database;
- other managed relational database solutions only if they satisfy the project constraints.

## Evaluation

The providers will be compared through:

1. documented provider research;
2. a weighted benchmark;
3. a limited technical proof of concept;
4. documented operational and security analysis.

The proof of concept should validate:

1. database provisioning;
2. secure authentication and secret handling;
3. connection from the ASP.NET Core API;
4. Entity Framework Core provider compatibility;
5. migration execution;
6. schema creation;
7. persistence and retrieval of application data;
8. execution of `GET /api/data-sources`;
9. compatibility with the planned Render deployment;
10. Terraform provisioning experience;
11. Terraform provider behavior;
12. Free-offer limitations;
13. inactivity and wake-up or restoration behavior;
14. connection recovery;
15. backup or export capability;
16. operational risks;
17. billing safeguards.

The central question is:

> Can this managed database solution support GameMarketIntel reliably, securely, and sustainably, with no mandatory operating cost and without introducing unacceptable technical or operational risk?

## Decision

Not decided yet.

The final provider will be selected after the benchmark and proof of concept are completed.

## Consequences

To be completed after the decision.

The final decision may affect:

- the Entity Framework Core database provider;
- migration strategy;
- local development infrastructure;
- Terraform resources;
- deployment configuration;
- connection management;
- backup procedures;
- operational monitoring;
- long-term database portability.

## Risks

Current risks under evaluation include:

- provider inactivity or automatic suspension;
- cold-start or resume latency;
- manual restoration requirements;
- connection exhaustion;
- Free-offer limitations;
- provider policy changes;
- insufficient Terraform support;
- dependency on a community-maintained Terraform provider;
- interruption of provider maintenance;
- operational dependency on provider-specific features;
- database-engine migration effort;
- provider-specific migrations;
- increased vendor lock-in;
- exposure of credentials through Terraform state;
- unexpected billing;
- insufficient storage for long-term historical data;
- monthly compute exhaustion;
- public application unavailability.

## Mitigations

Current planned mitigations include:

- keeping credentials outside the repository;
- treating Terraform state as sensitive;
- using protected environment variables for runtime credentials;
- pinning Terraform provider versions;
- committing `.terraform.lock.hcl`;
- reviewing provider ownership, releases, and maintenance activity;
- validating provider limits before production use;
- documenting operational constraints;
- avoiding unnecessary provider-specific application features;
- keeping database-specific implementation details inside the Infrastructure layer;
- avoiding provider-specific SQL when practical;
- preserving repository abstractions;
- keeping business rules outside the persistence layer;
- defining a historical-data retention and aggregation strategy;
- validating backup and export procedures;
- configuring billing safeguards before provisioning;
- reviewing every `terraform plan` before `terraform apply`.

## References

Supporting evidence will be maintained in:

- `managed-database-provider-benchmark.md`;
- `managed-database-poc.md`;
- provider research documents;
- PoC evidence records.

This ADR will be updated before its status changes from `Proposed` to `Accepted`.