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
| Render | Infrastructure provisioning | The Free Web Service should be created successfully through Terraform with Docker deployment, GitHub integration, and protected environment variables | The Render Web Service was created successfully through Terraform in approximately 5 seconds | Provisioning: 5 s | Passed | Service created on the Free plan and linked to the `develop` branch; deployment completion and application availability remained under validation |
| Render | Public API deployment and Neon integration | The ASP.NET Core API should deploy successfully, start in Production, connect securely to Neon PostgreSQL, and return persisted data through `GET /api/data-sources` | The Docker deployment completed successfully, the API became publicly available through managed HTTPS, connected to Neon, and returned the persisted data correctly | Not recorded | Passed | Deployment validated through the public Render URL; no provider-specific application architecture changes were required |
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
| Render | Terraform service update | The existing Free Web Service should accept configuration updates without unintended provider-side changes | Changing the deployment branch from `develop` to `main` through Terraform failed because the provider attempted to configure maintenance mode, which is unsupported on Free services. The branch was updated manually in the Render dashboard and the Terraform state was refreshed afterward | Not recorded | Partially passed | Initial provisioning succeeded, but provider behavior introduced an unsupported maintenance-mode update during a later change. Manual fallback was available |
| Render | Idle startup and first-request recovery | After more than 15 minutes without traffic, the suspended Free Web Service should resume automatically and complete the original API request without requiring a manual retry | After more than 15 minutes of inactivity, the first request completed successfully in 23,637.75 ms and returned the persisted Neon data without an error or manual retry. The following requests completed in 1,221.81 ms and 794.84 ms | Cold: 23,637.75 ms; subsequent average: 1,008.33 ms | Passed | Idle startup introduced noticeable latency but preserved the original request. The cold request was approximately 23.4× slower than the average of the next two requests. User-facing loading feedback is required to communicate progress during startup |
| Render | Warm-request latency | Requests made after the Free Web Service resumed should complete consistently and substantially faster than the idle-start request | Three warm requests completed successfully in 770.66 ms, 857.45 ms, and 355.96 ms | Warm average: 661.36 ms | Passed | Warm responses remained below one second on average. The idle-start request was approximately 35.7× slower than the measured warm average |
| Render | Secret and repository protection | Database credentials, Render credentials, Terraform variables, and Terraform state should remain outside the repository and should not be exposed in deployment or runtime logs | The Neon connection string and ASP.NET Core environment were configured as protected Render environment variables, sensitive values were masked in the dashboard, Terraform variables and state remained ignored, and no credential was observed in deployment or runtime logs | Not recorded | Passed | Validated through the Render Environment page, Git ignore checks, tracked-file inspection, deployment logs, and runtime logs |
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

## Final PoC Result

| Provider | Result | Main Strength | Main Risk |
|---|---|---|---|
| Render Free | Passed | Simple Docker-based deployment, GitHub continuous deployment, secure Neon integration, managed HTTPS, successful first-request recovery after inactivity, and low operational complexity | Noticeable cold-start latency after inactivity and partial Terraform update compatibility on the Free plan |

## Final Decision

Render Free is accepted as the API hosting platform for the GameMarketIntel zero-cost phase.

The provider met the critical requirements:

- the ASP.NET Core API was deployed successfully through Docker;
- the API connected securely to Neon PostgreSQL;
- public HTTPS connectivity worked correctly;
- GitHub-based continuous deployment was validated;
- the first request after inactivity completed successfully without requiring a manual retry;
- warm-request latency remained acceptable;
- sensitive configuration remained protected;
- deployment events, logs, network usage, and service status were visible;
- the Free instance type remained explicitly configured;
- initial Terraform provisioning completed successfully.

The observed cold-start latency is accepted because the original request completed successfully and can be supported through appropriate user-facing loading feedback.

Terraform compatibility is accepted with a documented limitation. Initial service provisioning succeeded, but a later Free-plan update attempted to apply unsupported maintenance-mode configuration. A manual dashboard update and Terraform state refresh were available as a fallback.

Render should be reevaluated when:

- cold-start behavior becomes unacceptable for production usage;
- Free-plan limits are approached;
- traffic growth requires higher availability;
- Terraform update limitations affect routine infrastructure management;
- operational requirements exceed the Free service capabilities.