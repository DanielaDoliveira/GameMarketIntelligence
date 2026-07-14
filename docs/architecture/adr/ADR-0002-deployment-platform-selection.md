# ADR-0002: Deployment Platform Selection

## Status

Accepted

## Context

GameMarketIntel requires a managed platform for hosting its ASP.NET Core API.

The selected platform must support the project's zero-cost phase while preserving application compatibility, deployment reliability, security, infrastructure reproducibility, and acceptable user experience.

The current architecture includes:

- ASP.NET Core;
- .NET 10;
- Neon PostgreSQL;
- Entity Framework Core with Npgsql;
- GitHub Actions for continuous integration;
- Azure Static Web Apps for the frontend;
- Terraform for infrastructure automation.

The hosting platform should avoid unnecessary application changes and should provide predictable zero-cost operation.

## Decision Drivers

The main decision drivers are:

- permanent or sustainable zero-cost availability;
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
- logs and monitoring.

## Considered Options

### Render Free

Strengths:

- zero-cost Web Service;
- GitHub integration;
- automatic deployment;
- Docker compatibility;
- managed HTTPS;
- environment variables;
- low operational complexity;
- suitable integration model for Neon PostgreSQL.

Risks:

- Free services may suspend after inactivity;
- cold-start behavior must be validated;
- first-request success must be proven;
- Terraform provider maturity must be evaluated;
- Free-plan resource limits may affect long-term operation.

### Azure App Service F1

Strengths:

- native .NET support;
- mature Azure integration;
- official Terraform provider;
- managed HTTPS;
- existing project experience.

Risks:

- previous quota friction;
- limited Free-tier compute;
- additional operational dependency on Azure;
- no clear advantage over Render for the current phase.

### Google Cloud Run

Strengths:

- strong container support;
- official Terraform provider;
- mature cloud platform;
- scalable serverless architecture.

Reason for screening out:

- Free usage is based on a pay-as-you-go billing model;
- no simple native hard spending cap was identified;
- billing-protection automation would add unnecessary complexity;
- financial risk conflicts with the project's zero-cost requirement.

### AWS

Strengths:

- mature cloud ecosystem;
- strong Terraform support;
- broad .NET compatibility.

Reason for screening out:

- no sufficiently simple and predictable permanent zero-cost managed API-hosting option was identified;
- previous experience demonstrated risk of unexpected infrastructure costs;
- available alternatives would add unnecessary operational complexity.

### Koyeb

Strengths:

- Docker support;
- GitHub deployment;
- Free instance;
- managed platform.

Reason for not shortlisting:

- no clear project advantage over Render;
- lower familiarity;
- smaller operational track record;
- additional evaluation effort without a demonstrated benefit.

## Decision

## Decision

Render Free is selected as the API hosting platform for the GameMarketIntel zero-cost phase.

The proof of concept validated:

- ASP.NET Core deployment through Docker;
- secure connectivity to Neon PostgreSQL;
- managed HTTPS;
- protected environment variables;
- GitHub-based continuous deployment from the `main` branch;
- successful first-request recovery after inactivity;
- acceptable warm-request latency;
- deployment and runtime observability;
- visible network usage;
- explicit Free-instance configuration;
- Terraform-based initial provisioning.

The first request after more than 15 minutes of inactivity completed successfully in approximately 23.64 seconds without requiring a manual retry.

Three measured warm requests completed in:

- 770.66 ms;
- 857.45 ms;
- 355.96 ms.

The measured warm-request average was 661.36 ms.

The cold-start delay is accepted because the original request remained successful and can be communicated through appropriate loading feedback in the frontend.

Azure App Service F1 remains a documented fallback.

Terraform support is accepted with a known limitation: initial provisioning succeeded, but a later Free-plan service update triggered unsupported maintenance-mode behavior in the provider. Manual configuration and Terraform state refresh remain documented fallback procedures.

## Consequences

Positive consequences:

- the first request after inactivity remained successful;
- infrastructure recovery did not require manual user retry;
- public API documentation can remain available through Scalar;
- deployment events and usage metrics are visible in the Render dashboard.

Negative consequences:

### Negative

- idle suspension introduces noticeable cold-start latency;
- user-facing loading feedback is required during service startup;
- Free-plan limits require monitoring;
- Terraform updates may require a documented manual fallback for unsupported Free-plan properties;
- long-term availability depends on the continued suitability of the Render Free plan;
- API availability also depends on Neon PostgreSQL availability.

## Validation

The decision will be finalized after completion of the API hosting provider benchmark and Render proof of concept.

The PoC must validate:

- service provisioning;
- application deployment;
- Neon connectivity;
- read and write operations;
- continuous deployment;
- first request after inactivity;
- warm-request latency;
- security;
- logs and metrics;
- Free-plan limitations;
- Terraform provider reliability;
- successful resource destruction.