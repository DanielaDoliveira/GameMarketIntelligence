# Azure SQL Database Research

## Status

Research in progress.

Last verified: 2026-07-12.

## Overview

Azure SQL Database is a fully managed relational database service based on the SQL Server engine.

For GameMarketIntel, Azure SQL Database is being evaluated as an alternative to the PostgreSQL providers currently under consideration.

The intended application flow would be:

```text
ASP.NET Core API
        ↓
Entity Framework Core
        ↓
Microsoft.EntityFrameworkCore.SqlServer
        ↓
Azure SQL Database
```

Adopting Azure SQL would require changing the Entity Framework Core database provider and generating SQL Server-compatible migrations.

The Domain, Application, Shared, API contracts, services, and most repository logic should remain unchanged.

## Free Tier

At the verification date, the Azure SQL Database free offer provides, per eligible database:

- no expiration date;
- 100,000 vCore-seconds of serverless compute each month;
- up to 32 GB of data storage;
- up to 32 GB of backup storage;
- up to 10 free-offer databases per Azure subscription.

The service is a fully managed platform database. Azure manages infrastructure responsibilities such as patching, platform maintenance, backups, and monitoring.

The offer provides two possible behaviors when the free monthly limit is reached:

- pause the database until the following calendar month;
- continue operating with additional paid usage.

For GameMarketIntel, only the pause behavior is acceptable during the zero-cost portfolio phase.

The configuration that permits additional paid usage should not be enabled without a documented architectural and financial decision.

Preliminary assessment: **strongly positive**, provided the no-overage behavior is explicitly validated.

## Technical Compatibility

Azure SQL Database uses the SQL Server database engine.

GameMarketIntel would need to replace:

```text
Npgsql.EntityFrameworkCore.PostgreSQL
```

with:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

The current application architecture should otherwise remain largely unchanged.

Expected reusable components include:

- Domain entities;
- value objects;
- Application services;
- repository abstractions;
- API endpoints;
- DTOs;
- business rules;
- dependency direction;
- most Entity Framework Core mappings.

The following areas would require validation or adaptation:

- SQL Server-compatible migrations;
- database type mappings;
- identifier and naming conventions;
- provider-specific SQL;
- connection configuration;
- retry behavior;
- local development configuration.

The current model uses standard Entity Framework Core features, including:

- `DbContext`;
- entity configurations;
- `OwnsOne`;
- migrations;
- asynchronous queries;
- repository patterns.

These features are supported by the Microsoft SQL Server Entity Framework Core provider.

Preliminary assessment: **positive**.

## Local Development Compatibility

GameMarketIntel can use a local SQL Server container for development and provider-specific validation.

The expected local flow would be:

```text
Docker SQL Server
        ↓
Microsoft.EntityFrameworkCore.SqlServer
        ↓
GameMarketIntel.Infrastructure
```

The existing PostgreSQL development environment may remain available while the comparison is in progress.

A provider switch should not occur in the main application branch until the PoC demonstrates that:

- migrations can be regenerated safely;
- current mappings remain valid;
- tests remain green;
- local development remains practical;
- production deployment behavior is acceptable.

Preliminary assessment: **positive**.

## Inactivity and Availability

The free offer uses the Azure SQL serverless compute model.

Serverless databases can automatically pause after a configured period without activity and resume when a new connection is received.

Paused periods do not consume active compute in the same way as running periods.

For GameMarketIntel, the main availability consideration is the delay of the first request after the database resumes.

The expected behavior is closer to Neon's automatic wake-up model than to a provider that requires manual restoration.

The PoC should measure:

- first connection after inactivity;
- first API request after database resume;
- warm-request latency;
- behavior when Render and Azure SQL are both inactive;
- connection recovery through Entity Framework Core.

Preliminary assessment: **neutral to positive**.

## Storage and Growth

The Free offer provides up to 32 GB of database storage.

This is substantially larger than the current Free allowances of the PostgreSQL candidates under evaluation.

For GameMarketIntel, database growth is expected to come mainly from:

- historical metric snapshots;
- price history;
- review history;
- player-count history;
- ranking history;
- collector execution data;
- indexes.

Reference data such as games, genres, publishers, platforms, and data sources is expected to consume comparatively little storage.

The long-term storage requirement depends primarily on:

- number of monitored games;
- number of metrics;
- collection frequency;
- average snapshot size;
- index size;
- retention policy;
- whether raw source payloads are stored.

The project should avoid duplicating stable game metadata in every historical snapshot.

A retention strategy may include:

```text
Recent data
→ detailed snapshots

Older data
→ weekly or monthly aggregates

Expired detail
→ removal after successful aggregation
```

Deleting old snapshots can release logical space for database reuse.

The PoC should estimate storage using representative records rather than relying only on theoretical calculations.

Suggested validation:

- insert a representative snapshot dataset;
- measure database growth;
- calculate average storage per snapshot;
- project monthly and annual growth;
- evaluate retention and aggregation effects.

Preliminary assessment: **strongly positive**.

## Compute and Free-Limit Behavior

The Free offer provides 100,000 vCore-seconds of serverless compute per month.

The practical duration represented by this allowance depends on the number of active vCores used by the database.

For GameMarketIntel, compute consumption may be influenced by:

- API traffic;
- database wake-ups;
- Entity Framework Core migrations;
- collector executions;
- analytical queries;
- indexing;
- aggregation jobs;
- connection activity that prevents auto-pause.

The database should be configured to pause when the free allowance is exhausted.

This means the main zero-cost risk is service unavailability until the next monthly reset rather than an unexpected charge.

The PoC should measure:

- compute usage during migrations;
- compute usage during collector execution;
- compute usage during repeated API calls;
- time spent active after requests;
- whether connection behavior prevents auto-pause;
- expected monthly compute consumption.

Preliminary assessment: **positive**, pending real usage measurements.

## Connections and Resiliency

Azure SQL Database uses standard SQL Server connectivity.

The application would use:

```text
Microsoft.Data.SqlClient
```

through:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

Azure SQL may experience transient connection failures during events such as:

- service resume;
- maintenance;
- failover;
- temporary network interruption.

Entity Framework Core supports connection resiliency through retry strategies.

The application should evaluate enabling retry behavior for Azure SQL.

The PoC should validate:

- normal API connectivity;
- migration execution;
- simultaneous API and collector access;
- connection recovery after resume;
- retry behavior;
- timeout behavior;
- absence of duplicate side effects during retries.

Preliminary assessment: **positive**.

## Backup and Recovery

The Free offer includes up to 32 GB of backup storage per database.

Azure SQL Database provides managed backup capabilities as part of the platform service.

The exact recovery behavior, retention period, and restore options available under the selected Free configuration should be validated before the provider is accepted.

GameMarketIntel should not assume that provider-managed backup alone is sufficient for long-term historical preservation.

A portable logical export strategy may still be useful.

The PoC should validate:

- available restore points;
- restore workflow;
- restore duration;
- backup visibility;
- database export;
- restoration into an isolated environment;
- preservation of migration history.

Preliminary assessment: **positive**, pending recovery validation.

## Terraform Support

Azure SQL Database can be provisioned through Terraform using the AzureRM provider.

Relevant resources include:

```text
azurerm_resource_group
azurerm_mssql_server
azurerm_mssql_database
```

Terraform can be used to define:

- resource group;
- logical SQL server;
- SQL database;
- region;
- database configuration;
- network access;
- selected security settings;
- tags;
- outputs.

The free-offer configuration and free-limit behavior must be validated carefully before `terraform apply`.

No infrastructure should be applied until the Terraform plan confirms that the intended database configuration remains within the Free offer.

Preliminary assessment: **strongly positive**.

## Provider and Dependency Trust

The Azure Terraform ecosystem uses official Microsoft-supported Azure infrastructure APIs and the widely adopted AzureRM provider.

The Entity Framework Core SQL Server provider is officially maintained as part of the Entity Framework Core project.

Positive indicators:

- official Azure platform support;
- mature Terraform ecosystem;
- broad production adoption;
- stable provider release history;
- extensive official documentation;
- official Entity Framework Core provider;
- public source code;
- long-term vendor support.

Risks:

- AzureRM is a large provider with a broad upgrade surface;
- provider upgrades may change behavior;
- Azure resource APIs may evolve;
- not every portal configuration is exposed identically through Terraform;
- incorrect infrastructure configuration may create billable resources;
- provider-generated or resource-derived secrets may appear in Terraform state.

Mitigations:

- pin provider versions;
- commit `.terraform.lock.hcl`;
- review release notes;
- validate upgrades outside production;
- inspect every `terraform plan`;
- use cost-safe settings;
- treat Terraform state as confidential;
- prohibit paid-overage configuration without explicit approval.

Preliminary assessment: **strongly positive**.

## Render Integration

Azure SQL Database can be used as an external database by the ASP.NET Core API deployed on Render.

The expected flow is:

```text
Render API
        ↓
ConnectionStrings__DefaultConnection
        ↓
Microsoft.Data.SqlClient
        ↓
Azure SQL Database
```

The production connection string should remain outside the repository and be stored as a protected Render environment variable.

The current configuration key can remain unchanged:

```text
ConnectionStrings__DefaultConnection
```

The main integration considerations are:

- TLS connectivity;
- firewall and network access;
- database region;
- Render-to-Azure latency;
- connection retry behavior;
- database resume latency;
- migration execution;
- credential protection.

The Azure SQL region should be selected as close as practical to the Render region.

Preliminary assessment: **positive**.

## Security

Azure SQL Database provides managed security capabilities such as:

- encrypted database connections;
- logical-server access controls;
- firewall rules;
- Microsoft Entra authentication options;
- SQL authentication;
- database roles and permissions;
- auditing and monitoring capabilities;
- platform-managed security controls.

GameMarketIntel should separate credentials by responsibility:

```text
Local development
→ .NET User Secrets

Render runtime
→ protected database connection string

Terraform
→ Azure authentication outside the repository

Repository
→ no real credentials
```

The runtime application should use only the permissions required for normal reads and writes.

Migration permissions should be separated when practical.

Important risks include:

- overly broad firewall rules;
- leaked SQL credentials;
- leaked Azure credentials;
- secrets stored in Terraform state;
- connection strings appearing in logs;
- accidental creation of paid resources.

Mitigations:

- use least-privilege database users;
- protect Terraform state;
- avoid logging connection strings;
- review firewall configuration;
- rotate credentials;
- use protected environment variables;
- inspect all planned resources before apply;
- use Azure Cost Management alerts as additional monitoring.

Preliminary assessment: **strongly positive**.

## Operational Complexity

Azure manages:

- database infrastructure;
- patching;
- platform maintenance;
- backups;
- serverless compute;
- monitoring capabilities;
- database availability mechanisms.

GameMarketIntel remains responsible for:

- schema;
- migrations;
- credentials;
- application monitoring;
- query performance;
- data retention;
- compute usage;
- free-limit monitoring;
- deployment sequencing;
- recovery validation.

The project already uses Azure for the frontend, which reduces the number of new provider ecosystems that must be learned.

However, the API remains planned for Render, so the final architecture would still span:

```text
GitHub
+
Terraform
+
Azure Static Web Apps
+
Render
+
Azure SQL Database
+
GameMarketIntel
```

The main operational concern is cost-safe configuration.

Preliminary assessment: **positive**.

## Vendor Lock-in

Azure SQL Database uses the SQL Server engine.

GameMarketIntel would depend on:

- Microsoft SQL Server behavior;
- Microsoft Entity Framework Core SQL Server provider;
- SQL Server-compatible migrations;
- Azure SQL operational configuration;
- AzureRM Terraform resources.

Application-level portability remains supported by the existing architecture because the Domain and Application layers do not directly depend on the database provider.

However, switching back to PostgreSQL would require:

- replacing the EF Core provider;
- generating or adapting migrations;
- validating type mappings;
- migrating data;
- replacing Azure-specific infrastructure resources.

This creates greater database-engine lock-in than moving between PostgreSQL providers.

Mitigations:

- keep provider-specific code in Infrastructure;
- avoid provider-specific SQL where practical;
- preserve repository abstractions;
- keep business rules outside persistence;
- document migration differences;
- maintain export and migration procedures.

Preliminary assessment: **neutral**.

## Main Advantages

- no expiration date for the Free offer;
- 32 GB of data storage;
- 32 GB of backup storage;
- 100,000 vCore-seconds monthly;
- up to 10 Free-offer databases per subscription;
- official Terraform support;
- official Entity Framework Core provider;
- strong .NET integration;
- mature Azure ecosystem;
- configurable region;
- serverless auto-pause and resume;
- managed backups and monitoring;
- existing project experience with Azure;
- substantial growth capacity.

## Main Risks

- incorrect configuration may allow paid usage;
- monthly compute allowance may be reached;
- the database may pause until the next month when the free allowance is exhausted;
- migration from PostgreSQL to SQL Server requires provider and migration changes;
- greater database-engine lock-in;
- Render-to-Azure network latency;
- firewall configuration complexity;
- Terraform state may contain sensitive information;
- persistent connections may affect auto-pause behavior;
- provider-specific migration differences may appear.

## Preliminary Assessment

Azure SQL Database is a strong candidate for GameMarketIntel.

Its principal strengths are:

- substantially larger Free storage capacity;
- strong .NET and Entity Framework Core integration;
- official and mature Terraform support;
- configurable region;
- serverless operation;
- long-term capacity for historical data.

Its principal concerns are:

- strict cost-safety requirements;
- monthly compute limits;
- database-engine migration effort;
- higher lock-in than PostgreSQL-based alternatives.

No eliminatory condition has been confirmed.

Azure SQL should remain eligible for the technical PoC only if the infrastructure configuration can guarantee that additional paid usage is not enabled.

The PoC should confirm:

- Free-offer eligibility;
- pause behavior when the free limit is reached;
- Terraform support for the required configuration;
- SQL Server-compatible migrations;
- persistence and retrieval;
- Render connectivity;
- latency;
- retry behavior;
- secure credential handling;
- storage-growth assumptions.

## Official Sources

| Source | Purpose | Verification Date |
|---|---|---|
| Azure SQL Database Free Offer | Free compute, storage, backup, limits, and behavior | 2026-07-12 |
| Azure SQL Free Offer FAQ | Duration, database count, monthly limits, and billing behavior | 2026-07-12 |
| Azure SQL Terraform Quickstart | Terraform provisioning | 2026-07-12 |
| EF Core SQL Server Provider | Official Entity Framework Core provider | 2026-07-12 |
| Azure SQL Serverless Documentation | Auto-pause and compute behavior | 2026-07-12 |
| EF Core Connection Resiliency | Retry behavior and transient-failure handling | 2026-07-12 |