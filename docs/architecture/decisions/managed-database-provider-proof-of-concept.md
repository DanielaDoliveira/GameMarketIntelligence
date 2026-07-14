# Managed Database Provider Proof of Concept

## Objective

Validate the shortlisted managed database providers using the current GameMarketIntel application and infrastructure requirements.

The PoC should produce practical evidence for the final provider decision.

The evaluation should focus on application compatibility, infrastructure automation, security, operational behavior, cost predictability, and long-term sustainability.

## Candidates

- Neon PostgreSQL;
- Supabase PostgreSQL;
- Aiven PostgreSQL;
- Azure SQL Database.

## Validation Scope

Each provider should be evaluated using the same core tests.

Provider-specific adaptations may be required when the database engine differs.

These adaptations should be recorded as part of the migration-effort evaluation rather than treated automatically as a failure.

### 1. Infrastructure Provisioning

Validate:

- Terraform provider installation;
- provider authentication;
- database provisioning;
- secure secret handling;
- successful `terraform init`;
- successful `terraform validate`;
- successful `terraform plan`;
- successful `terraform apply`;
- stable second `terraform plan`;
- absence of unintended infrastructure changes;
- visibility of Free-offer configuration;
- billing-safety configuration.

### 2. Application Integration

Validate:

- database connection;
- Entity Framework Core provider compatibility;
- required provider-package configuration;
- migration compatibility;
- migration execution;
- schema creation;
- insertion of a `DataSource`;
- persistence of `SourceReliability`;
- retrieval of persisted data;
- successful execution of `GET /api/data-sources`;
- preservation of current architectural boundaries.

Record any required changes to:

- Entity Framework Core provider packages;
- `DbContext` configuration;
- migrations;
- database type mappings;
- connection strings;
- provider-specific infrastructure code.

### 3. Deployment Integration

Validate:

- API deployment on Render;
- environment-variable configuration;
- secure connection;
- TLS behavior;
- read operations;
- write operations;
- migration behavior in the deployed environment;
- absence of credentials in application logs;
- network compatibility;
- regional latency.

### 4. Performance and Availability

Measure:

- first database connection after inactivity;
- first API request after inactivity;
- warm-request latency;
- database wake-up or restoration behavior;
- connection recovery;
- latency between Render and the database provider;
- behavior when both the API and database are inactive;
- operational impact of automatic pause or suspension.

### 5. Operational Validation

Validate:

- metrics and monitoring;
- storage visibility;
- compute-usage visibility;
- connection visibility;
- backup or export process;
- recovery or restoration process;
- credential rotation;
- operational complexity;
- Terraform provider ownership;
- Terraform provider maintenance;
- dependency maturity;
- release history;
- documentation quality;
- availability of manual fallback procedures.

### 6. Security Validation

Validate:

- encrypted database connectivity;
- credential separation;
- use of protected environment variables;
- absence of secrets in the repository;
- absence of secrets in logs;
- Terraform-state exposure risk;
- least-privilege access;
- network-access configuration;
- credential-rotation process.

### 7. Cost and Free-Offer Validation

Validate:

- Free-offer eligibility;
- requirement for a payment method;
- Free-offer expiration behavior;
- storage allowance;
- compute allowance;
- network-transfer limits;
- billing-overage behavior;
- available cost controls;
- ability to prevent unexpected paid usage;
- behavior when a Free limit is reached.

A provider should not be accepted for the zero-cost phase when unexpected paid usage cannot be prevented or controlled adequately.

## Evidence Record

| Provider | Test | Expected Result | Observed Result | Duration | Status | Notes |
|---|---|---|---|---:|---|---|
| Neon | Infrastructure provisioning | Project created successfully through Terraform | Project, branch, database, role, and endpoint were created successfully | Not recorded | Passed | Second `terraform plan` returned no changes |
| Neon | EF Core migration execution | Existing migrations applied successfully to the Neon database | The application connected through the direct Neon endpoint and created the schema successfully | Not recorded | Passed | Connection string stored in .NET User Secrets |
| Neon | API read operation | `GET /api/data-sources` should connect to Neon and return a successful response | The API connected successfully and returned an empty collection from the new database | Not recorded | Passed | Validated through the application using the Neon connection stored in .NET User Secrets |
| Neon | Database write operation | A representative `DataSource` record should be persisted successfully | A `DataSource` record with `SourceReliability` data was inserted successfully into the Neon database | Not recorded | Passed | Insert validated through the Neon SQL Editor |
| Neon | End-to-end persistence and retrieval | Persisted data should be returned by `GET /api/data-sources` | The API returned the inserted `DataSource` successfully from Neon | Not recorded | Passed | Validated through the current repository, service, DTO mapping, and Minimal API endpoint |
| Neon | Inactivity and warm-request latency | The database should resume automatically after inactivity, with subsequent requests completing faster | First request completed in 1826.01 ms; subsequent requests completed in 163.96 ms, 140.98 ms, and 140.51 ms | Cold: 1826.01 ms; Warm average: 148.49 ms | Passed | Automatic wake-up succeeded without manual intervention; cold request was approximately 12.3× slower than the warm average |
| Neon | Pooled runtime connection | The API should access Neon successfully through the pooled endpoint | The API connected through the Neon PgBouncer endpoint and returned the persisted `DataSource` successfully | Not recorded | Passed | Runtime access validated with Npgsql, Entity Framework Core, repository, service, and Minimal API endpoint |
| Neon | Secret and repository protection | Database and infrastructure credentials should remain outside the repository and application logs | Terraform variables and state remained ignored, the database connection stayed in .NET User Secrets, and no credential was observed in application logs | Not recorded | Passed | Validated with Git ignore checks, tracked-file inspection, repository search, and runtime-log review |
| Neon | Monitoring and usage visibility | Compute, storage, history, and network usage should be visible | The Neon dashboard displayed 0.02 of 100 CU-hours, 0.03 of 0.5 GB storage, 0.01 GB of history, and network-transfer usage | Not recorded | Passed | Free-plan quotas and current consumption were visible directly in the project dashboard |
| Neon | Portable backup export | The database should be exportable using standard PostgreSQL tooling | A custom-format backup was generated successfully with `pg_dump` executed from a temporary PostgreSQL 17 Docker container | Not recorded | Passed | Backup created without installing PostgreSQL locally; output stored outside version control |
| Neon | Portable backup restoration | The exported backup should restore successfully into an isolated PostgreSQL database | The Neon dump was restored into a local PostgreSQL 17 database, and the persisted `Newzoo` record was retrieved successfully | Not recorded | Passed | Restoration validated with `pg_restore` and direct SQL verification |
| Azure SQL Database | Infrastructure provisioning | Resource group and Azure SQL logical server should be provisioned successfully | The resource group and Azure SQL logical server were created successfully through Terraform after changing the SQL region from East US to Central US | Not recorded | Passed | East US returned `ProvisioningDisabled`; Central US succeeded after generating a new globally unique server name |
| Azure SQL Database | Free-offer and billing-safety configuration | The database should use the Free limit and pause instead of generating overage charges | The database was created as General Purpose Serverless with `useFreeLimit = true` and `freeLimitExhaustionBehavior = AutoPause` | Not recorded | Passed | Validated after provisioning through Azure CLI; SKU `GP_S_Gen5_1`, 1 vCore maximum, 0.5 vCore minimum, 60-minute inactivity pause, and 32 GiB maximum storage |
| Azure SQL Database | EF Core migration execution | A SQL Server-specific migration should create the application schema successfully | A new SQL Server migration was generated from the existing domain model and applied successfully to Azure SQL | Not recorded | Passed | Existing PostgreSQL migrations could not be reused directly because provider-specific database types differed; the SQL Server migration used `uniqueidentifier` and `nvarchar` |
| Azure SQL Database | End-to-end persistence and retrieval | A representative `DataSource` should be persisted and returned by `GET /api/data-sources` | A `Newzoo` record was inserted into Azure SQL and returned successfully through the current repository, service, DTO mapping, and Minimal API endpoint | Not recorded | Passed | Initial manual enum value was invalid and was corrected to a valid `ReliabilityLevel`; API retrieval then completed successfully |
| Azure SQL Database | Serverless resume behavior | The database should resume automatically and allow the first valid API request to complete without requiring user intervention | Across repeated tests, the first API request failed with an internal server error because the API could not connect while the database was resuming. A second request completed successfully | First request: approximately 30 s before failure; second request: successful | Failed | The behavior repeatedly exposed infrastructure recovery to the user and required a manual retry. This was classified as a critical availability and user-experience failure for the zero-cost phase |

| Provider | Application Changes | Migration Changes | Infrastructure Changes | Operational Changes |
|---|---|---|---|---|
| Neon | No application architecture changes required; existing Npgsql and EF Core integration were preserved | Existing PostgreSQL migrations were applied successfully without modification | Community Terraform provider used to create the project, branch, database, role, and endpoint | Direct connection used for migrations; pooled connection validated for runtime; monitoring and backup workflows documented |
| Azure SQL Database | Existing domain entities, repositories, services, DTO mapping, and endpoint behavior were preserved; runtime configuration changed from Npgsql to the EF Core SQL Server provider | Existing PostgreSQL migrations were not reusable directly; a clean SQL Server-specific migration was generated because provider-specific types differed | Terraform used for the resource group and Azure SQL logical server; Azure CLI was required for explicit Free-limit and `AutoPause` database configuration | Region availability must be validated before provisioning; post-provisioning verification of `FreeLimit = True` and `ExhaustionBehavior = AutoPause` is mandatory; repeated first-request failures after serverless pause made the Free Serverless configuration unsuitable for the zero-cost phase |


| Provider | Test | Expected Result | Observed Result | Duration | Status | Notes |
|---|---|---|---|---:|---|---|
| Neon | ... | ... | ... | ... | ... | ... |
| Azure SQL Database | Serverless resume behavior | ... | ... | ... | Failed | ... |
| Azure SQL Database | Infrastructure destruction | All PoC resources should be removed reproducibly and no Azure SQL infrastructure should remain active | `terraform destroy` removed all Terraform-managed resources. The Terraform state became empty, the resource group returned `ResourceGroupNotFound`, and the SQL logical server was no longer listed by Azure CLI | Not recorded | Passed | The database created through Azure CLI was removed together with the SQL logical server and resource group |

### Azure SQL Free Database Provisioning Template

The Azure SQL logical server is provisioned through Terraform. The database is created separately through Azure CLI because the AzureRM resource does not expose the free-offer configuration with the same explicit controls used by the Azure CLI.

The database creation command must explicitly enable the free limit and configure automatic suspension when the monthly allowance is exhausted.

```powershell
$ResourceGroupName = "rg-gamemarketintel-sql-poc"
$SqlServerName = terraform output -raw sql_server_name
$DatabaseName = "gamemarketintel"

az sql db create --resource-group $ResourceGroupName --server $SqlServerName --name $DatabaseName --edition GeneralPurpose --family Gen5 --capacity 1 --compute-model Serverless --min-capacity 0.5 --auto-pause-delay 60 --max-size 32GB --use-free-limit true --free-limit-exhaustion-behavior AutoPause
```

## Early Termination Decision

The remaining Azure SQL Database tests were intentionally discontinued after the Serverless resume behavior failed a critical availability and user-experience requirement.

The failure was reproduced across repeated tests: the first valid API request after database inactivity returned an internal server error because the API could not connect while the database was resuming, while a second manual request succeeded.

This behavior was considered unacceptable for the GameMarketIntel public application because infrastructure recovery would be exposed directly to the user and normal access would depend on a manual retry.

Further deployment, operational, security, backup, and cost tests would not reverse this critical result. Continuing those tests would add implementation effort without changing the provider decision for the zero-cost phase.

The rejection applies specifically to the Azure SQL Database Free Serverless configuration with automatic pause. It should not be interpreted as a rejection of Azure SQL Database as a managed database service or of provisioned compute tiers that remain continuously available.

## Neon Storage Sustainability Strategy

Neon was selected despite the Free-plan storage constraint because the risk is measurable, monitorable, and can be mitigated without changing the current application architecture.

The production database should store structured and application-ready data rather than raw source artifacts.

The following rules apply during the zero-cost phase:

- store normalized entities, source metadata, reliability information, metric snapshots, and derived market signals in PostgreSQL;
- store image references as URLs rather than binary image data;
- keep source CSV files, raw JSON payloads, HTML captures, PDFs, and database backup files outside the operational database;
- retain only the historical granularity required by product and analytical requirements;
- review indexes based on demonstrated query needs rather than creating speculative indexes;
- use the Neon dashboard and PostgreSQL size queries to monitor database and index growth;
- create portable backups with `pg_dump` using a direct connection and validate restoration periodically with `pg_restore`;
- preserve PostgreSQL portability so the application can move to another compatible provider or a paid Neon plan if required.

The following internal thresholds will guide operational action:

| Storage Utilization | Operational Response |
|---:|---|
| Below 50% | Normal operation and routine monitoring |
| 50% to 70% | Review growth rate, largest tables, indexes, and retention assumptions |
| 70% to 80% | Implement approved cleanup, aggregation, or archival actions and evaluate migration or plan-upgrade options |
| Above 80% | Treat storage as an active capacity risk and execute the selected capacity plan before importing additional large datasets |

The thresholds are internal decision points rather than provider guarantees. Current provider limits must be revalidated before production deployment and during periodic operational reviews.

## Provider Selection Decision

Neon PostgreSQL is selected as the managed database provider for the GameMarketIntel zero-cost portfolio phase.

The decision is based on the successful proof-of-concept results, preservation of the existing PostgreSQL and Entity Framework Core implementation, automatic recovery without exposing failed requests to the user, pooled runtime connectivity, visible usage metrics, portable backup and restore, and lower migration effort.

Azure SQL Database Free Serverless is not selected for this phase because repeated tests exposed an internal server error on the first API request after database inactivity and required a second manual request. This behavior violated a critical availability and first-use experience requirement for the public application.

The selection does not remove the need to monitor Neon's Free-plan storage and compute limits. Those constraints are accepted as managed operational risks with documented thresholds, retention rules, backup procedures, and a migration path.

## Final PoC Result

| Provider | Result | Main Strength | Main Risk |
|---|---|---|---|
| Neon | Passed | Low migration effort, automatic wake-up, standard PostgreSQL compatibility, pooled runtime connectivity, and successful portable backup and restore | Limited Free storage and reliance on a community-maintained Terraform provider |
| Azure SQL Database | Rejected for the zero-cost phase | Strong Azure integration, explicit Free-limit controls, official tooling for core infrastructure, and successful EF Core integration | Free Serverless automatic pause repeatedly caused the first API request to fail during database resume, requiring a second manual request |

## Decision Rule

A provider may be rejected even with a high benchmark score when a critical requirement fails.

When a critical failure is reproducible and sufficient to determine that a provider is unsuitable for the target phase, the remaining provider-specific tests may be discontinued. The reason for early termination must be documented in the Evidence Record and final result.

Critical failures may include:

- inability to connect securely;
- inability to support the application model;
- failed Entity Framework Core integration;
- unacceptable migration complexity;
- inability to provision reliably;
- uncontrolled billing risk;
- unacceptable availability behavior;
- unacceptable dependency-maintenance risk;
- insufficient operational sustainability.

The final decision should consider:

- benchmark score;
- PoC evidence;
- application compatibility;
- migration effort;
- operational risk;
- billing predictability;
- security;
- Terraform reliability;
- dependency trust;
- long-term sustainability.