# Development and Delivery Strategy

## Purpose

GameMarketIntel is designed not only as a software product, but also as a practical learning environment for software architecture, .NET development, DevOps, infrastructure automation, continuous integration, continuous deployment, risk management, and production-oriented decision-making.

The project infrastructure was established before the main product-development phase so that future features can be developed, validated, and delivered through a stable and repeatable workflow.

## Learning Objectives

The project supports the development of practical experience with:

- ASP.NET Core and the .NET ecosystem;
- Blazor WebAssembly;
- Clean Architecture;
- Domain-Driven Design;
- automated testing;
- GitHub Actions;
- Continuous Integration;
- Continuous Deployment;
- Docker;
- Terraform;
- managed cloud services;
- infrastructure evaluation;
- architectural decision records;
- proof-of-concept validation;
- operational risk management;
- cost-aware architecture.

Terraform, CI/CD, cloud deployment, and infrastructure documentation are part of the project's learning journey rather than isolated implementation tasks.

The objective is to understand not only how to create infrastructure, but also how to evaluate its limitations, reproduce it, maintain it, and explain why each technology was selected.

## Stable Delivery Workflow

The project uses an automated delivery workflow:

```text
Feature branch
      ↓
Implementation
      ↓
Local validation
      ↓
Pull Request
      ↓
GitHub Actions
      ↓
Build and automated tests
      ↓
Merge into main
      ↓
Automatic deployment
      ↓
Public environment validation
```

Every deliverable version should pass through the same validation and deployment process.

This reduces uncertainty about whether changes will compile or behave differently only after deployment.

Deployment becomes part of the normal development workflow rather than a separate activity performed only near the end of the project.

## Continuous Integration

GitHub Actions validates changes through:

- dependency restoration;
- solution build;
- automated test execution.

The `main` branch is protected and changes must be introduced through Pull Requests.

Required CI checks must succeed before changes can be merged.

This provides:

- early detection of build failures;
- automated regression validation;
- safer integration;
- visible delivery history;
- reduced risk of introducing invalid changes into the production branch.

## Continuous Deployment

Deployable changes are delivered automatically after successful integration into the production branch.

Current deployment flow:

- the Blazor WebAssembly frontend is deployed through Azure Static Web Apps;
- the ASP.NET Core API is deployed through Render;
- Render monitors the `main` branch and automatically creates a new deployment after accepted changes.

Continuous Deployment reduces manual deployment effort and ensures that each approved deliverable version can be validated in a public environment.

The objective is not to eliminate deployment risk completely, but to make deployment frequent, repeatable, observable, and integrated into normal development.

## Infrastructure as Code

Terraform is used to document and automate infrastructure configuration where provider capabilities allow it.

Infrastructure as Code provides:

- reproducible resource definitions;
- documented provider configuration;
- faster environment provisioning;
- visible infrastructure changes;
- controlled resource destruction;
- reduced dependence on undocumented manual configuration.

Terraform does not guarantee complete automation.

Some infrastructure operations may still depend on:

- provider limitations;
- Free-plan restrictions;
- quotas;
- region availability;
- credentials;
- external configuration;
- documented manual fallback procedures.

The Render proof of concept demonstrated this limitation: initial service provisioning succeeded through Terraform, while a later Free-plan update required a manual configuration change followed by Terraform state synchronization.

This limitation is accepted because it does not significantly affect the current operational model.

## Architecture Before Feature Expansion

The initial project phase prioritized architectural and operational foundations before expanding the product feature set.

The following areas were evaluated and established:

- application architecture;
- database provider;
- API hosting provider;
- frontend hosting;
- CI pipeline;
- CD workflow;
- Docker deployment;
- infrastructure automation;
- backup and restoration;
- secret management;
- operational monitoring;
- cost limitations;
- infrastructure risks;
- user-experience impact of service inactivity.

This foundation allows future development to focus primarily on product value and domain behavior rather than repeatedly revisiting basic deployment and infrastructure decisions.

The architecture was not designed under the assumption that every future requirement can be predicted.

Instead, it was designed to address the main known requirements and risks while preserving the ability to evolve.

## Risk-Aware Development

The infrastructure-selection process improved the project's risk-management approach.

Technical decisions are evaluated not only by whether a technology works, but also by:

- user impact;
- availability behavior;
- cost predictability;
- storage limitations;
- operational complexity;
- recoverability;
- provider dependency;
- infrastructure reproducibility;
- long-term sustainability.

Known limitations are treated as managed risks.

### Render Cold Start

Render Free may suspend the API after inactivity.

The first request may experience significant startup latency.

This behavior is accepted because the original request remains active and completes successfully without requiring manual retry.

The primary mitigation is user-facing loading feedback.

The product principle is:

> The user needs to believe that the application will load.

### Neon Storage Limit

The Neon Free plan provides limited storage.

Mitigation includes:

- storing structured and processed data;
- keeping raw datasets outside the operational database;
- storing image references as URLs;
- monitoring database growth;
- defining retention policies;
- consolidating historical information when necessary;
- maintaining portable backups.

Infrastructure limitations may indirectly affect product scope.

Large datasets, high historical granularity, or storage-intensive features may need to be optimized, postponed, consolidated, or redesigned.

### Terraform Provider Limitations

Infrastructure automation coverage may differ between providers and service plans.

Mitigation includes:

- reviewing every Terraform plan;
- documenting manual fallback procedures;
- synchronizing Terraform state after approved manual changes;
- reevaluating providers when automation limitations become operationally significant.

## Cost Constraint

The project currently operates under a zero-cost infrastructure requirement.

Fixing cost at zero limits infrastructure capacity and provider options.

Accepted trade-offs include:

- cold starts;
- limited storage;
- limited compute;
- reduced availability guarantees;
- Free-plan usage limits;
- partial infrastructure automation;
- possible manual operational procedures.

These limitations may also influence product decisions.

The project may need to:

- reduce data granularity;
- consolidate historical records;
- prioritize high-value datasets;
- defer storage-intensive features;
- revise retention strategies.

Zero infrastructure cost does not mean the absence of trade-offs.

The cost may be expressed through reduced capacity, waiting time, additional operational effort, or constrained scope.

## Evolution Strategy

Current infrastructure decisions are not permanent.

Neon and Render were selected for the current zero-cost phase and current product requirements.

They should be reevaluated when:

- storage usage approaches operational limits;
- traffic increases significantly;
- cold-start latency affects user retention;
- availability requirements increase;
- project funding becomes available;
- infrastructure automation limitations create excessive operational effort;
- Free-plan terms change;
- product requirements exceed current provider capabilities.

The architecture should support evolution rather than attempt to predict every future need.

## Development Principle

The project follows this principle:

> A stable delivery process allows product development to focus on value while infrastructure risks remain visible, documented, and manageable.

The infrastructure foundation was created to support the application rather than restrict future development unnecessarily.

Future features can now be developed through a validated CI/CD workflow, with automated deployment and public-environment verification integrated into each deliverable version.