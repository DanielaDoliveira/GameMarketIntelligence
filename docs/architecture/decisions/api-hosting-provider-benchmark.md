# ADR-0002: Deployment Platform Selection

## Status

Proposed

## Context

GameMarketIntel requires a managed platform for hosting its ASP.NET Core API.

The selected platform must support the project's zero-cost phase while preserving application compatibility, deployment reliability, infrastructure reproducibility, security, operational simplicity, and acceptable user experience.

The current architecture includes:

- ASP.NET Core;
- .NET 10;
- Entity Framework Core with Npgsql;
- Neon PostgreSQL;
- GitHub Actions for continuous integration;
- Azure Static Web Apps for the frontend;
- Terraform for infrastructure automation;
- GitHub as the source-code repository.

The selected hosting platform should preserve the current application architecture and avoid unnecessary provider-specific changes.

## Decision Drivers

The main decision drivers are:

- sustainable zero-cost availability;
- no uncontrolled automatic paid usage;
- ASP.NET Core compatibility;
- Docker support;
- GitHub-based continuous deployment;
- secure environment-variable management;
- secure connectivity to Neon PostgreSQL;
- managed HTTPS;
- successful first request after inactivity;
- no manual retry requirement;
- Terraform compatibility;
- provider maturity;
- operational simplicity;
- logs, metrics, and usage visibility.

## Considered Options

### Render Free

Strengths:

- Free Web Service;
- GitHub integration;
- automatic deployment;
- Docker compatibility;
- managed HTTPS;
- protected environment variables;
- low operational complexity;
- suitable integration model for Neon PostgreSQL;
- Terraform provider availability.

Risks:

- Free services may suspend after inactivity;
- cold-start behavior must be validated;
- first-request success must be proven;
- Free-plan resource limits require monitoring;
- Terraform provider maturity and resource coverage must be evaluated.

### Azure App Service F1

Strengths:

- native .NET support;
- mature Azure integration;
- strong Terraform support;
- managed HTTPS;
- existing project experience.

Risks:

- previous quota friction;
- limited Free-tier compute;
- additional dependency on Azure;
- no clear advantage over Render for the current phase.

### Google Cloud Run

Strengths:

- strong container support;
- mature cloud platform;
- official Terraform provider;
- scalable serverless architecture.

Reason for screening out:

- Free usage depends on a pay-as-you-go billing model;
- no simple native hard spending cap was identified;
- billing-protection automation would add unnecessary complexity;
- potential financial risk conflicts with the project's zero-cost requirement.

### AWS

Strengths:

- mature cloud ecosystem;
- strong Terraform support;
- broad .NET compatibility.

Reason for screening out:

- no sufficiently simple and predictable permanent zero-cost managed API-hosting option was identified;
- available alternatives would introduce additional operational complexity;
- previous experience demonstrated the importance of avoiding unexpected infrastructure costs.

### Koyeb

Strengths:

- Docker support;
- GitHub deployment;
- Free instance;
- managed hosting platform.

Reason for not shortlisting:

- no clear project advantage over Render;
- lower familiarity;
- smaller operational track record;
- additional evaluation effort without a demonstrated benefit.

## Decision

Render Free is selected as the primary candidate for the API-hosting proof of concept.

Final acceptance depends on successful validation of:

- ASP.NET Core deployment;
- Docker-based deployment;
- secure Neon PostgreSQL connectivity;
- GitHub continuous deployment;
- managed HTTPS;
- protected environment variables;
- first-request success after inactivity;
- acceptable cold-start and warm-request latency;
- absence of credential exposure;
- predictable zero-cost operation;
- Terraform provisioning and destruction.

Azure App Service F1 remains a documented fallback.

## Consequences

### Positive

- reduced operational complexity;
- straightforward GitHub-based deployment;
- compatibility with the current ASP.NET Core architecture;
- no required migration away from Neon PostgreSQL;
- faster proof-of-concept execution;
- opportunity to validate continuous deployment separately from continuous integration.

### Negative

- idle suspension may introduce cold-start latency;
- Free-plan limits require monitoring;
- final provider acceptance remains dependent on PoC evidence;
- Terraform support must be validated before infrastructure automation is accepted;
- user-facing loading feedback will be required when cold starts occur.

## Validation

The decision will be finalized after completion of the API Hosting Provider Benchmark and Render proof of concept.

The PoC must validate:

- service provisioning;
- Docker deployment;
- ASP.NET Core startup;
- Neon connectivity;
- read and write operations;
- GitHub continuous deployment;
- first request after inactivity;
- behavior when both Render and Neon are inactive;
- warm-request latency;
- security;
- logs and usage visibility;
- Free-plan limitations;
- Terraform provider reliability;
- successful resource destruction.