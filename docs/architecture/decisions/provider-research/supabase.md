# Supabase Research

## Status

Research in progress.

Last verified: 2026-07-12.

## Overview

Supabase is a managed PostgreSQL development platform.

Each project includes a PostgreSQL database and may also provide additional services such as:

- Authentication;
- generated APIs;
- Realtime;
- file storage;
- Edge Functions;
- database extensions;
- monitoring and development tools.

For GameMarketIntel, Supabase is being evaluated primarily as a managed PostgreSQL provider.

The current architecture does not require the Supabase client SDK or its additional backend services.

The intended application flow is:

```text
ASP.NET Core API
        ↓
Entity Framework Core
        ↓
Npgsql
        ↓
Supabase PostgreSQL
```

## Free Tier

At the verification date, the Supabase Free plan provides:

- no monthly cost;
- up to 2 active projects;
- 500 MB of database storage per project;
- shared CPU;
- 500 MB of RAM;
- 5 GB of egress;
- 5 GB of cached egress;
- 1 GB of file storage;
- community support.

Free projects may be paused after one week of low activity.

The database enters read-only mode when the Free-plan database-size limit is exceeded.

The main capacity concerns for GameMarketIntel are:

- historical snapshot growth;
- shared compute performance;
- project pausing;
- the limited number of active Free projects.

Preliminary assessment: **neutral to positive**. :contentReference[oaicite:0]{index=0}

## Technical Compatibility

Supabase provides a full PostgreSQL database.

GameMarketIntel can continue using:

- Npgsql;
- Entity Framework Core;
- `GameMarketIntelDbContext`;
- current mappings;
- current migrations;
- standard PostgreSQL connection strings;
- PostgreSQL roles and permissions;
- standard SQL.

No Supabase-specific application SDK is required.

The current `DataSource` entity and `SourceReliability` owned value object should remain compatible.

The main items to validate are:

- migration execution;
- persistence and retrieval;
- connection-mode selection;
- reconnection after project restoration;
- compatibility with Render.

Preliminary assessment: **strongly positive**. :contentReference[oaicite:1]{index=1}

## Inactivity and Availability

Supabase may pause Free-plan projects after one week of low activity.

A paused project must be restored before normal application access resumes.

This differs from Neon, where compute automatically wakes when a new connection arrives.

For GameMarketIntel, the risk is not limited to additional latency.

The API may become unavailable until the project is restored.

Potential impacts include:

- temporary public unavailability;
- manual operational intervention;
- a portfolio reviewer encountering a failed application;
- connection failures in the API;
- collector executions failing while the project is paused.

The PoC should verify:

- how activity is measured;
- whether scheduled collector activity prevents pausing;
- restoration steps;
- restoration duration;
- application reconnection after restoration.

Preliminary assessment: **negative**. :contentReference[oaicite:2]{index=2}

## Connections

Supabase provides:

- direct PostgreSQL connections;
- Session Pooler connections;
- Transaction Pooler connections.

The shared pooler uses Supavisor and is available to Free projects.

Direct database connections use IPv6 by default.

The Session Pooler provides an IPv4-compatible option and preserves session behavior.

For GameMarketIntel, the Session Pooler may be suitable for migrations and normal API access when direct IPv6 connectivity is unavailable.

The PoC should validate:

- Npgsql compatibility;
- Entity Framework Core migrations;
- runtime queries;
- Render connectivity;
- application-side pooling;
- reconnection after project restoration.

Preliminary assessment: **positive**, with additional connection-mode complexity. :contentReference[oaicite:3]{index=3}

## Backup and Recovery

Supabase provides database backup capabilities, but advanced recovery features are limited on the Free plan.

Point-in-Time Recovery is a paid capability.

The current Free plan does not include the same managed backup availability as paid plans.

Standard PostgreSQL export workflows remain available through tools such as:

- `pg_dump`;
- Supabase CLI database dump;
- restore into another PostgreSQL environment.

If Supabase is selected, GameMarketIntel should define an independent logical-backup strategy for important historical data.

Preliminary assessment: **negative to neutral**. :contentReference[oaicite:4]{index=4}

## Terraform Support

Supabase provides an official Terraform provider:

```text
supabase/supabase
```

At the verification date, the provider is published by Supabase and is available in a stable `1.x` version.

The provider supports resources such as:

- projects;
- project settings;
- branches;
- API keys;
- Edge Functions;
- selected platform configuration.

This allows part of the Supabase infrastructure to be represented as code.

Preliminary assessment: **strongly positive**. :contentReference[oaicite:5]{index=5}

## Provider and Dependency Trust

The Terraform provider is officially published by Supabase.

Positive indicators:

- official provider ownership;
- stable `1.x` release;
- public source code;
- multiple released versions;
- documented resources;
- significant registry usage;
- direct alignment with the platform owner.

Risks:

- official ownership does not guarantee complete feature coverage;
- new platform features may not be available immediately;
- provider upgrades may introduce behavior changes;
- unsupported settings may still require manual configuration;
- sensitive values may appear in Terraform state.

Mitigations:

- pin the provider version;
- commit `.terraform.lock.hcl`;
- review release notes;
- validate upgrades outside production;
- use `terraform plan` before every apply;
- document manual configuration;
- treat Terraform state as confidential.

Preliminary assessment: **strongly positive**. :contentReference[oaicite:6]{index=6}

## Render Integration

Supabase can be used as an external PostgreSQL provider by an ASP.NET Core API deployed on Render.

The expected flow is:

```text
Render API
        ↓
ConnectionStrings__DefaultConnection
        ↓
Npgsql
        ↓
Supabase PostgreSQL
```

The connection string should remain outside the repository and be stored as a protected Render environment variable.

The current application configuration can remain unchanged.

Important validation points:

- selected connection mode;
- IPv4 or IPv6 compatibility;
- TLS connection;
- Entity Framework Core migrations;
- network latency;
- reconnection after project restoration;
- absence of credentials in logs.

Preliminary assessment: **positive**. :contentReference[oaicite:7]{index=7}

## Security

Supabase provides:

- encrypted PostgreSQL connectivity;
- PostgreSQL roles and permissions;
- platform access tokens;
- managed API keys;
- Row Level Security;
- project-level access controls.

GameMarketIntel should continue using its ASP.NET Core API as the public backend.

The project does not currently need to expose Supabase-generated APIs directly.

Recommended separation:

```text
Local development
→ .NET User Secrets

Render runtime
→ protected database connection string

Terraform
→ protected Supabase access token

Repository
→ no real credentials
```

The broader Supabase platform creates additional security considerations because generated APIs and platform services may expose data when configured incorrectly.

Unused services should not be enabled without a documented need.

Terraform state must be treated as sensitive.

Preliminary assessment: **positive**.

## Operational Complexity

Supabase manages:

- PostgreSQL infrastructure;
- database hosting;
- connection pooling;
- platform monitoring;
- integrated development tools;
- additional backend services.

GameMarketIntel remains responsible for:

- schema;
- migrations;
- connection-mode selection;
- credentials;
- application monitoring;
- data retention;
- backup strategy;
- project-pause recovery.

The platform is polished and feature-rich, but it provides much more than the current project needs.

This creates a trade-off:

```text
More integrated capabilities
        ↓
greater future flexibility
        ↓
more platform concepts and configuration
```

The main operational concern is Free-project pausing.

Preliminary assessment: **neutral to positive**.

## Vendor Lock-in

Database-level lock-in is expected to remain low because GameMarketIntel can use:

- PostgreSQL;
- Npgsql;
- Entity Framework Core;
- standard connection strings;
- standard SQL;
- PostgreSQL export tools.

Lock-in may increase if the project adopts:

- Supabase Auth;
- generated Data APIs;
- Realtime;
- Storage;
- Edge Functions;
- Supabase-specific client libraries;
- platform-managed workflows.

The safest initial strategy is:

```text
Supabase
→ managed PostgreSQL only

GameMarketIntel
→ ASP.NET Core remains the public API
```

Preliminary assessment: **positive**, provided optional platform services remain outside the core architecture.

## Main Advantages

- no mandatory monthly cost;
- standard PostgreSQL;
- strong Npgsql compatibility;
- expected Entity Framework Core compatibility;
- official Terraform provider;
- stable provider major version;
- flexible connection options;
- integrated dashboard;
- strong future platform capabilities;
- low database-level vendor lock-in;
- strong portfolio value.

## Main Risks

- automatic pausing after one week of low activity;
- possible manual restoration;
- weaker Free-plan recovery capabilities;
- shared compute;
- connection-mode complexity;
- direct IPv6 connectivity;
- limited number of active Free projects;
- broader platform complexity;
- possible lock-in if optional services are adopted;
- sensitive information in Terraform state.

## Preliminary Assessment

Supabase is a strong managed PostgreSQL candidate for GameMarketIntel.

Its main strengths are:

- strong PostgreSQL compatibility;
- official and mature Terraform support;
- flexible managed connection options;
- integrated operational tooling;
- future access to additional backend capabilities.

Its main weaknesses are:

- Free-project pausing;
- weaker managed recovery on the Free plan;
- additional connection and platform complexity.

No eliminatory condition has been confirmed.

However, the project-pause behavior is a significant operational risk for a public portfolio application.

Supabase should remain eligible for the technical PoC.

## Official Sources

| Source | Purpose | Verification Date |
|---|---|---|
| Supabase Pricing | Free-plan limits, storage, compute, egress, pausing, and backup availability | 2026-07-12 |
| Supabase Project Pausing | Inactivity and restoration behavior | 2026-07-12 |
| Supabase Database Documentation | PostgreSQL capabilities | 2026-07-12 |
| Supabase Connection Documentation | Direct, Session Pooler, and Transaction Pooler connections | 2026-07-12 |
| Supabase Backup Documentation | Backup and PITR capabilities | 2026-07-12 |
| Terraform Registry — `supabase/supabase` | Provider ownership, version, and supported resources | 2026-07-12 |