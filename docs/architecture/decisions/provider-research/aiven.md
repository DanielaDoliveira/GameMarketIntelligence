# Aiven Research

## Status

Research in progress.

Last verified: 2026-07-12.

## Overview

Aiven is a managed open-source data platform that provides hosted PostgreSQL and other managed data services.

For GameMarketIntel, Aiven is being evaluated primarily as a managed PostgreSQL provider.

The intended application flow is:

```text
ASP.NET Core API
        ↓
Entity Framework Core
        ↓
Npgsql
        ↓
Aiven for PostgreSQL
```

The application does not require an Aiven-specific SDK.

## Free Tier

At the verification date, the Aiven Free PostgreSQL plan provides:

- no monthly cost;
- no credit card requirement;
- no predefined expiration;
- one PostgreSQL service;
- one database node;
- 1 CPU;
- 1 GB of RAM;
- 1 GB of storage;
- metrics and logs;
- managed backups.

The Free plan also has important restrictions:

- only one Free service of each service type per organization;
- no cloud-provider selection;
- no region selection;
- no VPC;
- no static IP;
- no managed connection pooling;
- no support service;
- no uptime SLA.

The main capacity advantages are the comparatively larger storage allowance and included operational monitoring.

The main concerns are connection limits, region uncertainty, and inactivity-related service power-off.

Preliminary assessment: **positive**. :contentReference[oaicite:1]{index=1}

## Technical Compatibility

Aiven provides standard PostgreSQL connectivity.

GameMarketIntel can continue using:

- Npgsql;
- Entity Framework Core;
- `GameMarketIntelDbContext`;
- current entity mappings;
- current migrations;
- standard PostgreSQL connection strings;
- PostgreSQL roles and permissions;
- standard SQL;
- PostgreSQL export and restore tools.

No Aiven-specific application package is required.

The current `DataSource` entity and `SourceReliability` owned value object should remain compatible.

The main items to validate are:

- migration execution;
- persistence and retrieval;
- application-side connection pooling;
- reconnection after service power-on;
- compatibility with Render.

Preliminary assessment: **strongly positive**.

## Inactivity and Availability

Aiven Free services may be powered off after extended inactivity.

The provider sends a notification before inactivity-related shutdown, and the service can be powered on again through the Aiven Console.

This differs from Neon's automatic wake-on-connection model.

For GameMarketIntel, the main risk is public unavailability while the database service is powered off.

Potential impacts include:

- failed API requests;
- failed collector executions;
- manual operational intervention;
- a portfolio reviewer accessing the application while the database is unavailable;
- additional recovery time before normal service resumes.

The PoC should verify:

- how inactivity is measured;
- whether collector activity prevents shutdown;
- notification behavior;
- manual power-on steps;
- startup duration;
- application reconnection;
- data preservation.

Preliminary assessment: **negative**. :contentReference[oaicite:2]{index=2}

## Connections

The Free PostgreSQL plan has a relatively low native connection limit.

Managed PgBouncer pooling is not available on the Free plan.

GameMarketIntel may create database connections from:

- the Render API;
- Npgsql connection pools;
- Entity Framework Core migrations;
- the scheduled collector;
- administrative tools.

Application-side connection pooling will therefore require deliberate configuration.

The project should avoid relying on default pool sizes without validating them against the actual service limit.

The PoC should verify:

- API operation through Npgsql;
- migration execution;
- simultaneous API and collector access;
- connection cleanup;
- behavior near the connection limit;
- recovery after connection exhaustion.

Preliminary assessment: **neutral to negative**.

## Backup and Recovery

Aiven includes managed PostgreSQL backups.

The platform performs automated backups and supports recovery within the retention period available for the selected service plan. :contentReference[oaicite:3]{index=3}

Standard PostgreSQL logical-backup tools also remain available:

- `pg_dump`;
- `pg_restore`.

This provides both provider-managed recovery and a portable application-managed backup option.

The Free-plan recovery limits should still be validated before relying on them for valuable historical data.

Preliminary assessment: **neutral to positive**.

## Terraform Support

Aiven provides an official Terraform provider:

```text
aiven/aiven
```

The provider supports resources relevant to GameMarketIntel, including:

- PostgreSQL services;
- PostgreSQL databases;
- PostgreSQL users;
- projects;
- service configuration;
- selected networking and access resources.

Infrastructure changes can be reviewed through:

```text
terraform plan
```

and provisioned through:

```text
terraform apply
```

The provider can therefore support the planned Infrastructure as Code workflow.

Preliminary assessment: **strongly positive**. :contentReference[oaicite:4]{index=4}

## Provider and Dependency Trust

The Terraform provider is officially maintained within the Aiven ecosystem.

Positive indicators:

- official provider ownership;
- stable `4.x` release line;
- long release history;
- extensive resource coverage;
- public source code;
- large registry adoption;
- documented import and upgrade workflows.

At the verification date, the Terraform Registry shows more than 15 million downloads and more than 160 published versions. :contentReference[oaicite:5]{index=5}

Risks:

- broad provider scope increases upgrade complexity;
- new releases may still introduce behavior changes;
- not every operational action is represented in Terraform;
- service power-on and power-off remain operational actions outside the normal Terraform resource lifecycle. :contentReference[oaicite:6]{index=6}

Mitigations:

- pin the provider version;
- commit `.terraform.lock.hcl`;
- review release notes;
- validate upgrades outside production;
- execute `terraform plan` before every apply;
- document manual operations.

Preliminary assessment: **strongly positive**.

## Render Integration

Aiven PostgreSQL can be used as an external database for the ASP.NET Core API deployed on Render.

The expected flow is:

```text
Render API
        ↓
ConnectionStrings__DefaultConnection
        ↓
Npgsql
        ↓
Aiven PostgreSQL
```

The connection string should remain outside the repository and be stored in a protected Render environment variable.

Aiven requires encrypted PostgreSQL connectivity through TLS. :contentReference[oaicite:7]{index=7}

The main integration concern is region placement.

The Free plan does not provide full region-selection flexibility, so latency between Render and Aiven should be measured rather than assumed.

Important validation points:

- TLS connectivity;
- migration execution;
- network latency;
- connection-pool configuration;
- reconnection after service power-on;
- absence of credentials in logs.

Preliminary assessment: **neutral to positive**.

## Security

Aiven provides:

- encrypted PostgreSQL connections;
- PostgreSQL users and roles;
- managed service credentials;
- API authentication for infrastructure management;
- provider-managed platform security controls.

GameMarketIntel should separate credentials by responsibility:

```text
Local development
→ .NET User Secrets

Render runtime
→ protected database connection string

Terraform
→ protected Aiven API token

Repository
→ no real credentials
```

For stronger TLS verification, the application can validate the Aiven certificate authority rather than relying only on encrypted transport. :contentReference[oaicite:8]{index=8}

Terraform state must be treated as sensitive because provider resources can expose connection information and sensitive service attributes. :contentReference[oaicite:9]{index=9}

Preliminary assessment: **positive**.

## Operational Complexity

Aiven manages:

- PostgreSQL infrastructure;
- server maintenance;
- metrics;
- logs;
- backups;
- service lifecycle;
- platform operations.

GameMarketIntel remains responsible for:

- schema;
- Entity Framework Core migrations;
- credentials;
- application-side connection pooling;
- data retention;
- application monitoring;
- query performance;
- backup validation;
- inactivity recovery.

The provider offers a mature operational environment.

The main Free-plan complexities are:

- possible service power-off;
- no support service;
- no SLA;
- no managed connection pooling;
- only one Free PostgreSQL service;
- inability to optimize region placement.

Preliminary assessment: **positive**.

## Vendor Lock-in

Application-level lock-in is expected to remain low because GameMarketIntel uses:

- PostgreSQL;
- Npgsql;
- Entity Framework Core;
- standard SQL;
- standard connection strings;
- `pg_dump`;
- `pg_restore`.

Provider-specific dependencies are mainly isolated to:

- Aiven Terraform resources;
- Aiven service configuration;
- platform monitoring;
- backup workflows;
- service lifecycle operations.

The Domain and Application layers should remain independent from Aiven.

Preliminary assessment: **strongly positive**.

## Main Advantages

- no mandatory monthly cost;
- no credit card requirement;
- 1 GB of storage;
- 1 CPU and 1 GB of RAM;
- standard PostgreSQL;
- strong Npgsql compatibility;
- expected Entity Framework Core compatibility;
- metrics and logs included;
- managed backups;
- official Terraform provider;
- highly mature provider ecosystem;
- low application-level vendor lock-in.

## Main Risks

- possible inactivity-related service power-off;
- manual power-on may be required;
- no region selection;
- no managed connection pooling on Free;
- relatively low native connection capacity;
- only one Free PostgreSQL service;
- no support on Free;
- no uptime SLA;
- Terraform state may contain sensitive service information;
- Render-to-Aiven latency cannot be optimized confidently before provisioning.

## Preliminary Assessment

Aiven is a strong managed PostgreSQL candidate for GameMarketIntel.

Its main strengths are:

- comparatively generous Free-plan resources;
- standard PostgreSQL compatibility;
- included metrics, logs, and backups;
- official and mature Terraform support;
- low application-level vendor lock-in.

Its main weaknesses are:

- connection limitations;
- lack of managed pooling on Free;
- inactivity-related service power-off;
- inability to select the region;
- limited environment separation.

No eliminatory condition has been confirmed.

However, connection capacity and inactivity behavior are significant risks for a public portfolio application.

Aiven should remain eligible for the technical PoC.

## Official Sources

| Source | Purpose | Verification Date |
|---|---|---|
| Aiven PostgreSQL Free Tier | Free resources, limits, inactivity behavior, and plan restrictions | 2026-07-12 |
| Aiven PostgreSQL Backups | Automated backup behavior | 2026-07-12 |
| Aiven PostgreSQL Connection Documentation | TLS and connection configuration | 2026-07-12 |
| Aiven TLS Documentation | Certificate and verification behavior | 2026-07-12 |
| Aiven Terraform Documentation | Official provider usage | 2026-07-12 |
| Terraform Registry — `aiven/aiven` | Provider version, adoption, resources, and maturity | 2026-07-12 |