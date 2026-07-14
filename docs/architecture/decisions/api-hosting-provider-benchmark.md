# API Hosting Provider Benchmark

## Objective

Select a managed hosting platform for the GameMarketIntel ASP.NET Core API.

The selected platform must support the project's zero-cost phase while preserving application compatibility, deployment reliability, infrastructure reproducibility, security, operational simplicity, and acceptable user experience.

The evaluation should prioritize practical evidence over feature availability alone.

## Current Architecture

GameMarketIntel currently uses:

- ASP.NET Core;
- .NET 10;
- Entity Framework Core;
- Npgsql;
- Neon PostgreSQL;
- GitHub Actions for continuous integration;
- Azure Static Web Apps for the frontend;
- Terraform for infrastructure automation;
- GitHub as the source-code repository.

The selected API-hosting platform should preserve the current application architecture and avoid unnecessary provider-specific changes.

## Critical Requirements

The selected provider must support:

- ASP.NET Core deployment;
- Docker-based deployment;
- secure HTTPS connectivity;
- protected environment variables;
- secure connectivity to Neon PostgreSQL;
- GitHub-based continuous deployment;
- predictable zero-cost operation;
- no automatic paid overage without explicit user action;
- successful completion of the first request after inactivity;
- no manual retry requirement after idle startup;
- application logs without credential exposure;
- infrastructure automation through Terraform or another documented reproducible mechanism.

A provider may be rejected without completing all tests when a critical requirement fails.

## Candidates

| Provider | Initial Status | Reason |
|---|---|---|
| Render Free | Selected for PoC | Free Web Service, GitHub integration, Docker compatibility, managed HTTPS, environment variables, low operational complexity, and Terraform provider availability |
| Azure App Service F1 | Documented fallback | Strong .NET and Terraform support, but previous quota friction and no clear advantage over Render for the current project |
| Google Cloud Run | Screened out | Free usage depends on a pay-as-you-go billing model without a simple native hard spending cap |
| AWS | Screened out | No sufficiently simple and predictable permanent zero-cost managed API-hosting option was identified |
| Koyeb | Not shortlisted | No clear project advantage over Render and additional evaluation effort without a demonstrated benefit |

## Evaluation Criteria

| Category | Criterion | Priority |
|---|---|---|
| Cost | Sustainable zero-cost option | Critical |
| Cost | No automatic paid overage | Critical |
| Application | ASP.NET Core compatibility | Critical |
| Application | Docker compatibility | High |
| Database | Secure Neon PostgreSQL connectivity | Critical |
| Deployment | GitHub continuous deployment | Critical |
| Deployment | Deployment-status visibility | High |
| Availability | First request after inactivity succeeds | Critical |
| Availability | No manual user retry is required | Critical |
| Performance | Acceptable cold-start behavior | High |
| Performance | Acceptable warm-request latency | High |
| Security | Protected environment variables | Critical |
| Security | No credentials in application or deployment logs | Critical |
| Infrastructure | Terraform compatibility | High |
| Infrastructure | Provider ownership and maintenance | High |
| Operations | Runtime and deployment logs | High |
| Operations | Metrics and usage visibility | High |
| Operations | Simple rollback or redeployment | Medium |
| Networking | Managed HTTPS | Critical |
| Networking | Custom-domain support | Medium |

## Preliminary Assessment

| Criterion | Render Free |
|---|---|
| Zero-cost Web Service | Available |
| Payment required for initial deployment | Not required |
| ASP.NET Core compatibility | Supported through Docker |
| GitHub integration | Supported |
| Automatic deployment | Supported |
| Environment variables | Supported |
| Managed HTTPS | Supported |
| Neon PostgreSQL connectivity | Passed in local Docker validation; remote validation pending |
| Terraform provider | Available; PoC validation pending |
| Idle suspension | Present |
| First-request behavior after inactivity | Pending PoC |
| Cold-start latency | Pending measurement |
| Warm-request latency | Pending measurement |
| Runtime logs | Pending validation |
| Usage visibility | Pending validation |
| Credential protection | Pending validation |
| Unexpected-cost protection | Pending validation |
| Resource destruction | Pending validation |
| Final result | Pending |

## Proof-of-Concept Scope

### 1. Infrastructure Provisioning

Validate:

- Render account and workspace access;
- Terraform provider installation;
- provider authentication;
- API-key protection;
- Web Service provisioning;
- successful `terraform init`;
- successful `terraform validate`;
- successful `terraform plan`;
- successful `terraform apply`;
- stable second `terraform plan`;
- absence of unintended infrastructure changes;
- successful resource destruction.

### 2. Application Deployment

Validate:

- Docker image build;
- ASP.NET Core startup;
- public API availability;
- managed HTTPS;
- environment-variable configuration;
- secure Neon PostgreSQL connectivity;
- successful execution of `GET /api/data-sources`;
- successful read operation;
- successful write operation;
- preservation of the current architectural boundaries.

### 3. Continuous Deployment

Validate:

- GitHub repository integration;
- deployment from the selected branch;
- automatic deployment after a new commit;
- deployment-status visibility;
- build-log visibility;
- failed-build behavior;
- redeployment behavior.

### 4. Performance and Availability

Measure:

- first API request after Render inactivity;
- first database request after Neon inactivity;
- behavior when both Render and Neon are inactive;
- warm-request latency;
- API-to-database latency;
- first-request success;
- manual retry requirement;
- application recovery behavior.

Cold-start latency is acceptable when the original request completes successfully and the frontend can communicate the loading state appropriately.

A cold start becomes a critical failure when the user receives an error or must manually repeat the request.

### 5. Security Validation

Validate:

- encrypted public connectivity;
- protected environment variables;
- absence of credentials in the repository;
- absence of credentials in deployment logs;
- absence of credentials in runtime logs;
- Terraform-state exposure risk;
- Render API-key protection;
- database credential separation.

### 6. Operational Validation

Validate:

- deployment logs;
- runtime logs;
- service metrics;
- usage visibility;
- redeployment process;
- rollback availability;
- provider documentation;
- Terraform provider ownership;
- Terraform provider maintenance;
- resource deletion;
- manual fallback procedure.

### 7. Cost Validation

Validate:

- Free-instance eligibility;
- payment-method requirement;
- monthly usage limitations;
- bandwidth limitations;
- build limitations;
- behavior when Free limits are reached;
- possibility of automatic paid usage;
- upgrade behavior;
- available usage monitoring.

## Evidence Record

| Provider | Test | Expected Result | Observed Result | Duration | Status | Notes |
|---|---|---|---|---:|---|---|
| Render | Local Docker deployment validation | The production Docker image should build successfully, start on Linux, receive the Neon connection string through an environment variable, and execute `GET /api/data-sources` | The Linux image built successfully, the ASP.NET Core API started in the Production environment on port 10000, connected securely to Neon, executed the EF Core query, and returned the persisted data successfully | Database command: 148 ms | Passed | Local validation completed before Render provisioning; non-blocking container warnings were identified for port configuration, local HTTPS redirection, and optional GSSAPI library availability |

## Decision Rule

Render will be accepted when:

- the ASP.NET Core API deploys successfully;
- secure Neon connectivity works;
- GitHub continuous deployment works;
- the first request after inactivity succeeds without manual retry;
- no critical credential exposure is identified;
- zero-cost operation remains predictable;
- infrastructure provisioning is reproducible;
- resource destruction is validated.

Render will be rejected when any critical requirement fails, including:

- repeated first-request failure after inactivity;
- requirement for manual user retry;
- uncontrolled billing risk;
- failed Neon integration;
- unreliable deployment behavior;
- unacceptable infrastructure-automation dependency risk;
- credential exposure.
