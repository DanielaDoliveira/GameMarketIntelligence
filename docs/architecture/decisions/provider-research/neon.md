# Neon Research

## Status

Research in progress.

Last verified: 2026-07-12.

## Overview

Neon is a managed serverless PostgreSQL platform.

Its architecture separates compute from storage and provides capabilities such as:

- autoscaling;
- scale to zero;
- database branching;
- read replicas;
- managed connection pooling;
- point-in-time recovery.

For GameMarketIntel, Neon is being evaluated as a production database for:

- the ASP.NET Core API;
- Entity Framework Core persistence;
- scheduled collector jobs;
- historical market data;
- Terraform-based infrastructure provisioning.

The intended application flow is:

```text
ASP.NET Core API
        ↓
Entity Framework Core
        ↓
Npgsql
        ↓
Neon PostgreSQL
```

## Free Tier

At the verification date, the Neon Free plan provides:

- no monthly cost;
- no credit card requirement;
- no predefined expiration;
- 0.5 GB of storage per project;
- 100 compute unit hours per project each month;
- compute sizes up to 2 compute units;
- 5 GB of public network transfer per month;
- database branching;
- read replicas;
- limited point-in-time recovery history.

The plan is financially predictable because it does not require a payment method.

The main capacity concern is storage growth caused by historical market snapshots.

## Technical Compatibility

Neon provides standard PostgreSQL connectivity.

GameMarketIntel can continue using:

- Npgsql;
- Entity Framework Core;
- `GameMarketIntelDbContext`;
- current entity mappings;
- current migrations;
- standard PostgreSQL connection strings;
- PostgreSQL roles and permissions.

The existing `DataSource` entity and `SourceReliability` owned value object should remain compatible.

No Neon-specific application SDK is required.

The main technical items to validate are:

- migration execution;
- persistence and retrieval;
- pooled and direct connection behavior;
- reconnection after inactivity;
- compatibility with the Render deployment.

## Inactivity and Availability

On the Free plan, Neon suspends compute after a period without active queries.

A new database connection automatically reactivates compute.

Persisted data remains stored while compute is suspended.

This behavior supports the zero-cost objective but may increase latency on the first request after inactivity.

For GameMarketIntel, the main risk is the combined effect of:

```text
Render cold start
        +
Neon compute wake-up
        =
higher first-request latency
```

This behavior should be measured during the PoC.

Preliminary assessment: **neutral**.

## Connections

Neon provides:

- direct PostgreSQL connections;
- pooled connections through PgBouncer.

Pooled connections are suitable for applications with multiple concurrent clients and reduce pressure on native PostgreSQL connections.

The expected GameMarketIntel workload is small, but the API and collector may access the database simultaneously.

The PoC should validate:

- normal API access;
- Entity Framework Core migrations;
- simultaneous API and collector access;
- reconnection after inactivity.

Preliminary assessment: **positive**.

## Backup and Recovery

Neon provides built-in point-in-time recovery.

The Free plan currently offers a limited recovery-history window.

This is useful for short-term recovery but should not be treated as a complete long-term backup strategy.

If the project begins storing valuable historical data, an additional logical-backup process may be required.

Preliminary assessment: **neutral to positive**.

## Terraform Support

Neon can be managed through the community-maintained provider:

```text
kislerdm/neon
```

The provider supports resources required by the initial architecture, including:

- projects;
- databases;
- endpoints;
- roles;
- branches;
- project permissions.

This allows database infrastructure to be represented as code.

However, the provider is not officially maintained by Neon.

Preliminary assessment: **neutral to positive**.

## Provider and Dependency Trust

The Terraform provider is maintained by the community rather than by Neon.

Positive indicators:

- published in the Terraform Registry;
- multiple released versions;
- documented resource support;
- public source code;
- evidence of active usage and maintenance.

Risks:

- maintenance depends on external contributors;
- Neon API changes may not be supported immediately;
- the provider remains below version `1.0`;
- maintenance continuity is not guaranteed.

Mitigations:

- pin the provider version;
- commit `.terraform.lock.hcl`;
- review releases and open issues before upgrades;
- validate changes with `terraform plan`;
- isolate Neon-specific Terraform resources;
- retain a documented manual provisioning fallback.

Preliminary assessment: **neutral**.

## Render Integration

Neon can be used as an external PostgreSQL provider by an API deployed on Render.

The expected flow is:

```text
Render API
        ↓
ConnectionStrings__DefaultConnection
        ↓
Npgsql
        ↓
Neon PostgreSQL
```

The connection string should remain outside the repository and be stored as a protected Render environment variable.

The current application configuration can remain unchanged.

Important validation points:

- TLS connection;
- migration execution;
- network latency;
- first-request latency;
- recovery after temporary connection failure.

Preliminary assessment: **positive**.

## Security

Relevant security capabilities include:

- encrypted PostgreSQL connections;
- PostgreSQL roles;
- Neon API keys;
- project-level access controls;
- managed credentials;
- optional network-access restrictions.

GameMarketIntel should separate:

```text
Local development
→ .NET User Secrets

Render runtime
→ protected environment variable

Terraform
→ protected API credential

Repository
→ no real secrets
```

Terraform state must be treated as sensitive because it may contain infrastructure information or generated credentials.

Preliminary assessment: **positive**.

## Operational Complexity

Neon manages:

- PostgreSQL infrastructure;
- storage;
- compute;
- scale to zero;
- connection endpoints;
- platform monitoring.

GameMarketIntel remains responsible for:

- schema;
- migrations;
- data retention;
- application monitoring;
- credentials;
- query performance;
- backups beyond the Free recovery window.

The operational model is relatively simple for a managed database.

The main complexity comes from coordinating:

```text
GitHub
+
Terraform
+
Render
+
Neon
+
GameMarketIntel
```

Preliminary assessment: **positive**.

## Vendor Lock-in

Application-level lock-in is expected to remain low because the project uses:

- PostgreSQL;
- Npgsql;
- Entity Framework Core;
- standard connection strings;
- standard migrations.

Provider-specific dependencies are mostly isolated to infrastructure:

- Terraform resources;
- branching;
- scale-to-zero behavior;
- Neon monitoring;
- Neon recovery tools.

The core application should avoid depending directly on Neon-specific APIs.

Preliminary assessment: **positive**.

## Main Advantages

- no mandatory cost;
- no credit card requirement;
- standard PostgreSQL;
- strong Npgsql compatibility;
- expected Entity Framework Core compatibility;
- automatic wake-up after inactivity;
- managed connection pooling;
- built-in recovery capability;
- simple Render integration;
- low application-level vendor lock-in;
- suitable for a serverless portfolio workload.

## Main Risks

- limited Free-plan storage;
- combined Render and Neon cold-start latency;
- short recovery-history window;
- community-maintained Terraform provider;
- sensitive information in Terraform state;
- long-term growth of historical snapshots;
- dependency on provider-specific infrastructure behavior.

## Preliminary Assessment

Neon is a strong candidate for GameMarketIntel.

Its main strengths are:

- simple PostgreSQL integration;
- predictable Free-plan operation;
- automatic recovery from inactivity;
- compatibility with the current .NET persistence architecture.

Its main weaknesses are:

- limited storage;
- lower Terraform-provider maturity;
- possible first-request latency.

No eliminatory condition has been confirmed.

Neon should remain eligible for the technical PoC.

## Official Sources

| Source | Purpose | Verification Date |
|---|---|---|
| Neon Pricing | Plans, storage, compute, and transfer limits | 2026-07-12 |
| Neon Plans Documentation | Free-plan characteristics | 2026-07-12 |
| Neon Scale to Zero Documentation | Inactivity and wake-up behavior | 2026-07-12 |
| Neon Connection Documentation | Direct and pooled connectivity | 2026-07-12 |
| Neon Backup Documentation | Recovery capabilities | 2026-07-12 |
| Terraform Registry — `kislerdm/neon` | Terraform resources and provider version | 2026-07-12 |