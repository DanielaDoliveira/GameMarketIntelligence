# ADR-0001 — Managed Database Provider Selection

## Status

Accepted

## Date

2026-07-14

## Context

GameMarketIntel requires a managed relational database for its public portfolio environment.

The database must support:

- collected market data;
- data-source metadata and reliability information;
- historical metric snapshots;
- derived market signals;
- API read and write operations;
- scheduled collection jobs;
- future analytical workloads.

The initial phase has a strict zero-mandatory-cost objective. The selected solution must therefore balance application reliability, billing predictability, Entity Framework Core compatibility, infrastructure automation, operational sustainability, and long-term portability.

The initial architecture used PostgreSQL with Npgsql and Entity Framework Core. Neon PostgreSQL, Supabase PostgreSQL, Aiven PostgreSQL, and Azure SQL Database were considered. The evaluation included provider research, benchmark criteria, and practical proof-of-concept evidence.

Azure SQL Database was included because of its official .NET and Terraform support, explicit Free-offer controls, and larger storage allowance. A dedicated proof of concept validated provisioning, Free-limit configuration, SQL Server-specific migrations, and application persistence.

## Decision Drivers

The selected provider should provide:

- no mandatory operating cost during the initial portfolio phase;
- controlled billing risk;
- reliable first-request behavior for a public application;
- compatibility with the current domain and persistence model;
- strong Entity Framework Core support;
- low migration and maintenance effort;
- secure credential handling;
- infrastructure automation support;
- operational visibility;
- backup and recovery capability;
- reasonable support for historical data growth;
- limited vendor lock-in;
- a practical migration path if project requirements outgrow the Free plan.

## Considered Options

### Neon PostgreSQL

Strengths:

- preserved the current PostgreSQL, Npgsql, and Entity Framework Core implementation;
- existing migrations executed without modification;
- automatic wake-up completed without requiring a manual second request;
- pooled runtime connectivity was validated;
- monitoring and usage data were visible;
- standard PostgreSQL backup and restore were validated with `pg_dump` and `pg_restore`;
- database portability remained high.

Trade-offs:

- limited Free-plan storage;
- Free-plan compute and transfer limits;
- Terraform integration depends on a community-maintained provider;
- capacity must be monitored as historical data grows.

### Azure SQL Database Free Serverless

Strengths:

- strong .NET and Azure ecosystem integration;
- official Entity Framework Core SQL Server provider;
- official tooling for core infrastructure;
- explicit Free-limit and automatic-pause controls;
- larger storage allowance than Neon Free.

Trade-offs and failure:

- required a different Entity Framework Core provider and SQL Server-specific migrations;
- required provider-specific database mappings;
- database creation required Azure CLI to expose the intended Free-limit controls explicitly;
- regional provisioning availability required adjustment;
- repeated resume tests caused the first valid API request to fail with an internal server error while the database resumed;
- a second manual request was required before the API returned data.

The repeated first-request failure was classified as a critical availability and user-experience failure for the public zero-cost phase.

### Supabase PostgreSQL and Aiven PostgreSQL

These providers remained part of the research and benchmark scope but were not taken through the same full application proof of concept after Neon passed the required technical validations and Azure SQL exposed a decisive critical failure.

They remain fallback candidates if Neon becomes operationally unsuitable or its plan conditions materially change.

## Decision

Use **Neon PostgreSQL** as the managed database provider for the GameMarketIntel zero-cost portfolio phase.

Continue using:

- PostgreSQL;
- Npgsql;
- Entity Framework Core PostgreSQL provider;
- existing PostgreSQL migrations;
- a direct Neon connection for migrations and backup operations;
- a pooled Neon connection for normal application runtime;
- protected environment variables or .NET User Secrets for credentials;
- Terraform for supported infrastructure automation;
- portable PostgreSQL backup and restore procedures.

Do not use Azure SQL Database Free Serverless for this phase.

This decision rejects the tested Free Serverless configuration for the current product constraints; it does not reject Azure SQL Database as a technology or paid compute configurations that remain continuously available.

## Consequences

### Positive

- no database-engine migration is required;
- current domain, repository, service, DTO, and endpoint boundaries are preserved;
- existing migrations remain valid;
- application and operational complexity remain lower;
- cold access may be slower than warm access but does not require a second manual user request in the validated scenario;
- standard PostgreSQL tooling provides a tested backup, restoration, and migration path;
- provider replacement remains feasible because the application avoids unnecessary Neon-specific persistence features.

### Negative

- Free-plan storage is limited and must be actively monitored;
- historical market data may eventually require aggregation, archival, a paid plan, or migration;
- Free-plan compute and network-transfer limits may affect availability as usage grows;
- the Terraform provider is community-maintained and must be pinned and periodically reviewed;
- the Free plan is suitable for the current portfolio phase but should not be assumed to satisfy future production service-level requirements.

## Storage and Data-Growth Strategy

The operational database will store structured, normalized, application-ready data.

The following data should remain in Neon:

- domain entities;
- metric snapshots required by product features;
- source metadata;
- source reliability information;
- derived market signals;
- references to external assets.

The following data should not be stored as database binary content:

- images; store image URLs instead;
- raw CSV files;
- raw API JSON payloads after validation and transformation;
- HTML captures;
- PDFs;
- database dump files.

Storage growth will be reviewed using the Neon dashboard and PostgreSQL size queries. Internal review thresholds are:

- 50% utilization: begin growth review;
- 70% utilization: prepare and prioritize retention, aggregation, archival, migration, or plan-upgrade actions;
- 80% utilization: execute the selected capacity action before importing additional large datasets.

These are project governance thresholds, not provider guarantees.

## Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Free-plan storage exhaustion | Store only structured application data, use image URLs, exclude raw artifacts, monitor table and index growth, define retention rules, and act before the 80% threshold |
| Historical-data growth | Aggregate data only when product requirements allow it, archive exportable history outside the operational database, and preserve a provider migration path |
| Compute or transfer limit exhaustion | Monitor provider usage, keep queries efficient, cache static content where appropriate, and reassess the hosting plan before limits become operational failures |
| Community Terraform provider dependency | Pin provider versions, commit the lock file, review releases and maintenance, and document manual provisioning fallback procedures |
| Provider policy or pricing changes | Revalidate limits periodically and maintain portable PostgreSQL backups and provider-neutral application boundaries |
| Credential or Terraform-state exposure | Keep secrets outside the repository, use protected environment variables, treat state as sensitive, and rotate credentials when required |
| Backup or recovery failure | Run `pg_dump` through a direct connection, keep backups outside version control, and validate `pg_restore` periodically |
| External image URL failure | Store source and attribution metadata, use stable HTTPS URLs, validate URLs during collection, and define a replacement or archival policy when sources disappear |

Detailed operational risks are maintained in `../risks/database-and-data-operational-risks.md`.

## Validation Evidence

The decision is supported by:

- Terraform provisioning evidence;
- Entity Framework Core migration evidence;
- application read and write evidence;
- cold and warm latency measurements;
- pooled runtime connectivity;
- secret and repository checks;
- monitoring and usage visibility;
- portable backup export;
- isolated backup restoration;
- repeated Azure SQL Free Serverless resume tests.

See:

- `../decisions/managed-database-provider-proof-of-concept.md`;
- `../risks/database-and-data-operational-risks.md`;
- the managed database benchmark and provider research documents.

## Review Conditions

Review this ADR when any of the following occurs:

- Neon storage reaches 70% of the current Free-plan allowance;
- compute or network limits affect application availability;
- the provider materially changes Free-plan limits, billing behavior, or service availability;
- the application becomes a production service with stronger availability requirements;
- historical-data requirements make the current retention model unsuitable;
- the community Terraform provider becomes unmaintained or unreliable;
- a paid database budget is approved.