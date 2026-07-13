# Managed Database Provider Benchmark

## Status

Research in progress.

Last updated: 2026-07-12.

## Objective

Compare the shortlisted managed database solutions using consistent, documented, and weighted criteria.

The benchmark should support an evidence-based decision for the GameMarketIntel production database.

The evaluation should consider:

- technical compatibility;
- migration effort;
- Free-offer sustainability;
- billing predictability;
- infrastructure automation;
- security;
- operational behavior;
- long-term data growth;
- application portability;
- public portfolio reliability.

The benchmark does not require a specific database engine in advance.

PostgreSQL compatibility is an advantage for the current implementation, but alternative relational engines may remain eligible when they support the application requirements with acceptable migration effort.

## Candidate Providers

- Neon PostgreSQL;
- Supabase PostgreSQL;
- Aiven PostgreSQL;
- Azure SQL Database.

## Project Constraints

The selected database solution should:

- provide a sustainable Free offering;
- minimize the risk of unexpected charges;
- support a relational data model;
- be compatible with Entity Framework Core;
- support the current Clean Architecture and DDD-oriented design;
- support the expected historical market-data workload;
- integrate with the ASP.NET Core API;
- support secure deployment with Render;
- support Infrastructure as Code through Terraform;
- provide an acceptable operational experience for a public portfolio project;
- preserve reasonable application portability;
- provide sufficient storage and compute capacity for planned growth.

## Elimination Criteria

A provider may be eliminated before final scoring when it presents a critical limitation.

Possible elimination conditions include:

- no sustainable Free offering;
- mandatory paid usage for the expected workload;
- inability to prevent or adequately control unexpected charges;
- insufficient compatibility with Entity Framework Core;
- inability to support the required relational model;
- unacceptable migration complexity;
- inability to connect securely from the deployed API;
- insufficient storage or compute for the expected project phase;
- unacceptable inactivity or restoration behavior;
- unavailable or unusable Terraform automation;
- unacceptable security risk;
- unacceptable dependency-maintenance risk;
- critical operational limitations without a practical mitigation;
- failed technical PoC.

A provider should not be eliminated solely because it uses a different relational database engine.

## Evidence Requirements

Each score should be supported by evidence.

Preferred evidence order:

1. official provider documentation;
2. official pricing and Free-offer documentation;
3. official Terraform Registry documentation;
4. official provider source repositories;
5. official Entity Framework Core documentation;
6. observed technical PoC results;
7. operational measurements;
8. documented assumptions when direct evidence is unavailable.

Each researched fact should record:

- source;
- verification date;
- relevant limitation;
- interpretation;
- confidence level.

Marketing claims should not be treated as sufficient evidence when technical or pricing documentation is available.

## Evaluation Scale

Each criterion receives a score from `0` to `5`.

| Score | Meaning |
|---:|---|
| 0 | Unsupported, unavailable, or unacceptable |
| 1 | Major limitations or high risk |
| 2 | Significant limitations requiring substantial mitigation |
| 3 | Acceptable with manageable limitations |
| 4 | Strong fit with minor limitations |
| 5 | Excellent fit with minimal relevant limitations |

A score of `3` represents an acceptable result.

Scores below `3` require explicit risk analysis and mitigation.

## Evaluation Criteria

| Criterion | Weight | Evaluation Focus |
|---|---:|---|
| Free-tier sustainability | 5 | Long-term availability of the Free offering and suitability for continued portfolio use |
| Billing predictability | 5 | Risk of unexpected charges, required payment methods, overage behavior, and cost controls |
| Application and relational-model compatibility | 5 | Compatibility with the current domain model, relational requirements, persistence architecture, and required migration effort |
| Entity Framework Core compatibility | 5 | Support for required EF Core features, mappings, migrations, and provider behavior |
| Storage capacity | 3 | Available storage and expected support for historical-data growth |
| Compute capacity | 3 | Available processing capacity and suitability for API, collector, and analytical workloads |
| Connection capacity | 4 | Connection limits, pooling behavior, API and collector concurrency, and recovery |
| Inactivity behavior | 4 | Automatic pause, wake-up, restoration requirements, and impact on public availability |
| Backup and recovery | 3 | Backup availability, retention, restoration, portability, and recovery limitations |
| Terraform support | 4 | Ability to provision and configure the required infrastructure through Terraform |
| Terraform provider maturity | 4 | Release maturity, adoption, stability, resource coverage, and upgrade history |
| Provider ownership and dependency trust | 4 | Official ownership, maintenance continuity, release health, public source code, security posture, and dependency risk |
| Render integration | 5 | Connectivity, TLS, regional latency, environment configuration, and deployment compatibility |
| Security | 5 | Credential handling, encryption, access control, network security, and secret-exposure risks |
| Operational complexity | 4 | Monitoring, maintenance, recovery, provider workflow, and ongoing operational effort |
| Vendor lock-in | 3 | Portability of application code, schema, data, infrastructure, and operational workflows |
| Portfolio suitability | 3 | Reliability, demonstrability, professional value, and suitability for public access |

## Weight Summary

| Priority | Weight |
|---|---:|
| Critical | 5 |
| High | 4 |
| Moderate | 3 |

The total criterion weight is:

```text
69
```

The maximum possible weighted score is:

```text
69 × 5 = 345
```

## Weighted Score

Each criterion score is multiplied by its weight.

```text
Weighted criterion score
=
criterion score × criterion weight
```

The total provider score is:

```text
Total weighted score
=
sum of all weighted criterion scores
```

The normalized result is:

```text
Normalized score
=
total weighted score ÷ 345 × 100
```

Example:

```text
Provider weighted score
=
276

Normalized score
=
276 ÷ 345 × 100

Normalized score
=
80%
```

## Preliminary Score Matrix

Scores should remain `Pending` until sufficient research evidence is documented.

| Criterion | Weight | Neon | Supabase | Aiven | Azure SQL |
|---|---:|---:|---:|---:|---:|
| Free-tier sustainability | 5 | 4 | 3 | 4 | 5 |
| Billing predictability | 5 | 5 | 5 | 5 | 4 |
| Application and relational-model compatibility | 5 | 5 | 5 | 5 | 3 |
| Entity Framework Core compatibility | 5 | 5 | 5 | 5 | 5 |
| Storage capacity | 3 | 2 | 2 | 3 | 5 |
| Compute capacity | 3 | 4 | 3 | 4 | 4 |
| Connection capacity | 4 | 5 | 4 | 2 | 4 |
| Inactivity behavior | 4 | 5 | 2 | 2 | 4 |
| Backup and recovery | 3 | 3 | 2 | 4 | 5 |
| Terraform support | 4 | 4 | 5 | 5 | 5 |
| Terraform provider maturity | 4 | 3 | 4 | 5 | 5 |
| Provider ownership and dependency trust | 4 | 3 | 5 | 5 | 5 |
| Render integration | 5 | 5 | 4 | 3 | 4 |
| Security | 5 | 4 | 4 | 4 | 5 |
| Operational complexity | 4 | 5 | 3 | 3 | 3 |
| Vendor lock-in | 3 | 5 | 4 | 5 | 3 |
| Portfolio suitability | 3 | 5 | 3 | 3 | 4 |

## Weighted Results

| Provider | Weighted Score | Maximum Score | Normalized Score | Preliminary Position |
|---|---:|---:|---:|---:|
| Neon PostgreSQL | 297 | 345 | 86.09% | 1 |
| Azure SQL Database | 297 | 345 | 86.09% | 1 |
| Aiven PostgreSQL | 275 | 345 | 79.71% | 3 |
| Supabase PostgreSQL | 264 | 345 | 76.52% | 4 |

## Evidence Record

| Provider | Criterion | Score | Evidence | Limitation | Confidence |
|---|---|---:|---|---|---|
| Neon | Free-tier sustainability | 4 | Free plan without predefined expiration, with serverless compute and automatic suspension | Limited storage and monthly usage limits | High |
| Neon | Billing predictability | 5 | Free-plan limits do not automatically generate paid overage | Service may become unavailable after limits are reached | High |
| Neon | Application and relational-model compatibility | 5 | Preserves PostgreSQL, Npgsql, current mappings, and expected migration compatibility | Provider-specific behavior still requires PoC validation | High |
| Neon | Entity Framework Core compatibility | 5 | Uses standard PostgreSQL through the Npgsql EF Core provider | Migration execution must still be validated | High |
| Neon | Storage capacity | 2 | Free storage supports the initial phase | 0.5 GB may become restrictive for long-term historical data | High |
| Neon | Compute capacity | 4 | Serverless compute and autoscaling suit intermittent workloads | Monthly compute limits and cold starts require measurement | Medium |
| Neon | Connection capacity | 5 | Managed PgBouncer pooling supports high client concurrency | Runtime and migration connection modes should be validated | High |
| Neon | Inactivity behavior | 5 | Compute automatically suspends and wakes on a new connection | First-request latency may increase | High |
| Neon | Backup and recovery | 3 | Built-in point-in-time recovery is available | Free recovery history is limited | High |
| Neon | Terraform support | 4 | Community provider covers the main required resources | Some configuration may still require manual work | Medium |
| Neon | Terraform provider maturity | 3 | Public provider with multiple releases and relevant adoption | Still below version 1.0 | High |
| Neon | Provider ownership and dependency trust | 3 | Public source code, active releases, and documented usage | Provider is maintained by the community rather than Neon | High |
| Neon | Render integration | 5 | Standard PostgreSQL connection, TLS, Npgsql, and managed pooling | Combined Render and database cold-start latency requires measurement | Medium |
| Neon | Security | 4 | TLS, PostgreSQL roles, API credentials, and network controls | Advanced controls may depend on configuration or plan | Medium |
| Neon | Operational complexity | 5 | Automatic wake-up, pooling, and no application-provider migration | Terraform provider trust and storage growth require monitoring | Medium |
| Neon | Vendor lock-in | 5 | Standard PostgreSQL, Npgsql, and portable database tooling | Neon-specific infrastructure remains in Terraform | High |
| Neon | Portfolio suitability | 5 | Simple serverless architecture with low migration effort | Limited storage may require earlier retention policies | Medium |
| Supabase | Free-tier sustainability | 3 | Permanent Free plan with managed PostgreSQL | Projects may pause after inactivity | High |
| Supabase | Billing predictability | 5 | Free-plan usage does not automatically continue as paid usage | Plan limits may interrupt service | Medium |
| Supabase | Application and relational-model compatibility | 5 | Preserves PostgreSQL, Npgsql, mappings, and current architecture | Connection-mode behavior requires validation | High |
| Supabase | Entity Framework Core compatibility | 5 | Standard PostgreSQL supported through Npgsql | Migration and pooler behavior must be tested | High |
| Supabase | Storage capacity | 2 | 500 MB supports the initial application | Limited margin for historical data and possible read-only behavior | High |
| Supabase | Compute capacity | 3 | Shared compute is adequate for a small workload | Performance may be less predictable under growth | Medium |
| Supabase | Connection capacity | 4 | Direct, Session Pooler, and Transaction Pooler options are available | Selecting the correct mode adds complexity | High |
| Supabase | Inactivity behavior | 2 | Paused projects can be restored | Restoration may require manual intervention | High |
| Supabase | Backup and recovery | 2 | PostgreSQL exports and CLI backup workflows are available | Strong managed recovery features are limited on Free | High |
| Supabase | Terraform support | 5 | Official Terraform provider supports relevant platform resources | Coverage of all required settings still requires PoC validation | High |
| Supabase | Terraform provider maturity | 4 | Official stable provider with active development | Shorter history than larger providers | High |
| Supabase | Provider ownership and dependency trust | 5 | Provider is officially maintained by Supabase | Official ownership does not guarantee complete feature coverage | High |
| Supabase | Render integration | 4 | Standard PostgreSQL and managed connection poolers | IPv4, IPv6, and connection-mode choices increase complexity | Medium |
| Supabase | Security | 4 | PostgreSQL roles, RLS, network restrictions, and SSL controls | Stronger SSL behavior requires explicit configuration | Medium |
| Supabase | Operational complexity | 3 | Rich dashboard and integrated platform capabilities | Project pausing, connection choices, and broader platform scope add work | Medium |
| Supabase | Vendor lock-in | 4 | Low when used only as PostgreSQL | Lock-in increases if Auth, Realtime, Storage, or Edge Functions are adopted | High |
| Supabase | Portfolio suitability | 3 | Recognized platform with strong development tooling | Manual restoration risk may affect public availability | Medium |
| Aiven | Free-tier sustainability | 4 | Permanent Free PostgreSQL with compute, storage, metrics, logs, and backups | Service may be powered off after inactivity | High |
| Aiven | Billing predictability | 5 | Free plan does not require automatic paid overage | Service limitations may require later upgrade | High |
| Aiven | Application and relational-model compatibility | 5 | Preserves PostgreSQL, Npgsql, mappings, and migrations | Connection limits require validation | High |
| Aiven | Entity Framework Core compatibility | 5 | Standard PostgreSQL through Npgsql | Real connection behavior must be tested | High |
| Aiven | Storage capacity | 3 | 1 GB provides more margin than Neon and Supabase | Still limited for long-term historical growth | High |
| Aiven | Compute capacity | 4 | 1 CPU and 1 GB RAM are suitable for moderate initial workloads | Fixed Free resources and no SLA | Medium |
| Aiven | Connection capacity | 2 | Native PostgreSQL access is available | Approximately 20 connections and no managed pooling on Free | High |
| Aiven | Inactivity behavior | 2 | Service can be powered on again | Recovery may require manual intervention | High |
| Aiven | Backup and recovery | 4 | Managed encrypted backups and recovery capabilities are included | Free-plan retention and restore behavior need PoC validation | Medium |
| Aiven | Terraform support | 5 | Official provider supports PostgreSQL services, databases, and users | Some lifecycle actions remain operational | High |
| Aiven | Terraform provider maturity | 5 | Long release history, broad adoption, and extensive resource coverage | Major upgrades still require review | High |
| Aiven | Provider ownership and dependency trust | 5 | Official provider with public source and long-term maintenance | Broad scope increases upgrade surface | High |
| Aiven | Render integration | 3 | Standard PostgreSQL with TLS | No region choice, low connection limit, and no managed pooling | Medium |
| Aiven | Security | 4 | TLS, roles, certificates, allowlists, and modern password encryption | Strong certificate validation adds configuration work | Medium |
| Aiven | Operational complexity | 3 | Managed monitoring, logs, and backups | Manual power-on, connection limits, and region restrictions add effort | Medium |
| Aiven | Vendor lock-in | 5 | Standard PostgreSQL and portable tooling | Aiven-specific Terraform and monitoring remain provider-bound | High |
| Aiven | Portfolio suitability | 3 | Demonstrates managed PostgreSQL, Terraform, monitoring, and backups | Public availability may be affected by inactivity behavior | Medium |
| Azure SQL Database | Free-tier sustainability | 5 | Long-term Free offer with large storage and monthly serverless compute | Eligibility and usage behavior must remain monitored | High |
| Azure SQL Database | Billing predictability | 4 | Can be configured to pause when the Free allowance is exhausted | Incorrect configuration can allow paid overage | High |
| Azure SQL Database | Application and relational-model compatibility | 3 | Current architecture remains compatible with a relational EF Core provider | Requires provider replacement and SQL Server-specific migrations | High |
| Azure SQL Database | Entity Framework Core compatibility | 5 | Official Microsoft EF Core provider for SQL Server and Azure SQL | Current PostgreSQL migrations cannot be assumed portable | High |
| Azure SQL Database | Storage capacity | 5 | 32 GB provides substantial capacity for historical data | Retention is still required for indefinite growth | High |
| Azure SQL Database | Compute capacity | 4 | Serverless compute is suitable for intermittent API and collector workloads | Monthly allowance and active-time consumption require measurement | Medium |
| Azure SQL Database | Connection capacity | 4 | Mature client pooling and resiliency support | Persistent connections may affect auto-pause | Medium |
| Azure SQL Database | Inactivity behavior | 4 | Automatic pause and resume are supported | Resume latency and Free-limit pause behavior require validation | High |
| Azure SQL Database | Backup and recovery | 5 | Managed automated backups, PITR, and substantial backup allowance | Restore behavior and cost implications must be validated | High |
| Azure SQL Database | Terraform support | 5 | Official AzureRM resources support server and database provisioning | Free-offer and no-overage settings must be confirmed in Terraform | High |
| Azure SQL Database | Terraform provider maturity | 5 | AzureRM has extensive adoption, long history, and broad coverage | Large provider scope increases upgrade complexity | High |
| Azure SQL Database | Provider ownership and dependency trust | 5 | Maintained through the Microsoft and HashiCorp ecosystem | Platform and provider changes still require release review | High |
| Azure SQL Database | Render integration | 4 | External .NET connectivity, official driver, and configurable region | Requires firewall configuration and provider migration | Medium |
| Azure SQL Database | Security | 5 | TLS, firewall, identity, auditing, roles, and mature platform controls | Misconfiguration can create excessive access or cost risk | High |
| Azure SQL Database | Operational complexity | 3 | Azure manages backups, patching, and platform operations | More resources, firewall rules, billing controls, and migration work | Medium |
| Azure SQL Database | Vendor lock-in | 3 | Clean Architecture limits application-layer coupling | SQL Server migrations, type mappings, and Azure resources increase exit effort | High |
| Azure SQL Database | Portfolio suitability | 4 | Strong alignment with .NET, Azure, Terraform, security, and growth | Higher migration and operational complexity | Medium |

## Risk Adjustment

The weighted score should support the decision but should not determine it automatically.

A provider with a high score may still be rejected when:

- a critical PoC test fails;
- billing risk cannot be controlled;
- secure deployment cannot be achieved;
- provider inactivity makes the public application unreliable;
- required infrastructure cannot be represented safely in Terraform;
- migration effort becomes disproportionate;
- dependency-maintenance risk is unacceptable.

A provider with a lower score may remain eligible when:

- the limitation has a practical mitigation;
- the PoC demonstrates acceptable real behavior;
- the provider offers a major strategic advantage;
- the operational trade-off is documented and accepted.

## Final Comparison

The current comparison is preliminary.

The benchmark reflects documented research and should not be treated as the final architectural decision.

The technical PoC may confirm, reduce, or increase the current scores.

| Provider | Main Strength | Main Weakness | Critical Risk | PoC Result | Preliminary Position |
|---|---|---|---|---|---|
| Neon PostgreSQL | Low migration effort, automatic wake-up, simple PostgreSQL integration, and strong Render compatibility | Limited Free storage and community-maintained Terraform provider | Historical-data growth or Terraform-provider maintenance may become limiting | Pending | Joint 1st |
| Azure SQL Database | Large Free storage capacity, mature Terraform ecosystem, strong security, backups, and .NET integration | Requires migration from PostgreSQL to SQL Server and increases operational complexity | Incorrect billing configuration or disproportionate migration effort | Pending | Joint 1st |
| Aiven PostgreSQL | Mature official Terraform provider, standard PostgreSQL, managed backups, and low vendor lock-in | Low connection limit, no managed pooling, no region selection, and possible manual power-on | Public application availability may be affected by inactivity behavior | Pending | 3rd |
| Supabase PostgreSQL | Official Terraform provider, standard PostgreSQL, flexible connectivity, and broad platform capabilities | Free-project pausing, limited storage, and weaker Free recovery capabilities | Manual restoration may make the public portfolio unavailable | Pending | 4th |

## Preliminary Interpretation

The benchmark currently identifies two leading candidates with different architectural profiles.

### Neon PostgreSQL

Neon currently represents the lower-change option.

Its main advantages are:

- preservation of the current PostgreSQL implementation;
- continued use of Npgsql;
- expected reuse of the current migrations;
- automatic wake-up after inactivity;
- managed connection pooling;
- simple integration with Render;
- low application-level vendor lock-in;
- comparatively low operational complexity.

Its main trade-offs are:

- limited Free storage;
- shorter Free recovery history;
- reliance on a community-maintained Terraform provider.

Neon is currently the strongest candidate when implementation simplicity and low migration effort are prioritized.

### Azure SQL Database

Azure SQL currently represents the higher-capacity and more infrastructure-mature option.

Its main advantages are:

- substantially larger Free storage capacity;
- mature Terraform support;
- official Entity Framework Core integration;
- strong security capabilities;
- managed backup and recovery;
- configurable region;
- strong alignment with the .NET and Azure ecosystem.

Its main trade-offs are:

- replacement of the current PostgreSQL provider;
- SQL Server-specific migrations;
- higher database-engine lock-in;
- increased infrastructure and operational complexity;
- strict requirement for billing-safe configuration.

Azure SQL is currently the strongest candidate when long-term storage capacity, platform maturity, and .NET ecosystem alignment are prioritized.

### Aiven PostgreSQL

Aiven remains technically viable and demonstrates strong infrastructure maturity.

Its main advantages are:

- official and mature Terraform support;
- PostgreSQL compatibility;
- managed backups;
- operational metrics and logs;
- low application-level vendor lock-in.

Its main limitations are:

- low native connection capacity;
- lack of managed pooling on the Free plan;
- inability to select the region;
- possible manual intervention after inactivity.

Aiven remains eligible for the PoC but currently presents more operational constraints than the leading candidates.

### Supabase PostgreSQL

Supabase remains a technically capable PostgreSQL platform.

Its main advantages are:

- official Terraform support;
- strong PostgreSQL and Entity Framework Core compatibility;
- flexible connection modes;
- extensive platform capabilities.

Its main limitations are:

- project pausing after inactivity;
- limited Free database storage;
- weaker Free backup and recovery capabilities;
- additional platform and connection complexity.

Supabase remains eligible for the PoC but currently presents the greatest public-availability concern among the candidates.

## Decision Relationship

This benchmark provides quantitative decision support.

The weighted score should not determine the final provider automatically.

Neon PostgreSQL and Azure SQL Database currently share the highest preliminary score but represent different architectural strategies.

The technical PoC should therefore focus especially on the trade-off between:

```text
Neon
→ lower migration effort
→ simpler operation
→ lower storage capacity
→ community-maintained Terraform provider
- long-term sustainability.


Azure SQL Database
→ greater storage capacity
→ stronger infrastructure maturity
→ greater migration effort
→ stricter billing-safety requirements

```
The final architectural decision should also consider:

provider research documents;
technical PoC evidence;
measured performance;
migration effort;
security findings;
operational risks;
cost safeguards;
historical-data growth;
long-term sustainability.

The final decision will be recorded in:

```text
ADR-0001 — Managed Database Provider Selection
```