# Version Control Workflow

## Purpose

This document defines the branch responsibilities, validation flow, and merge strategy used by GameMarketIntel.

The workflow is designed to provide fast feedback during active development while protecting the production branch with broader validation before deployment.

---

## Branches

### `develop`

The `develop` branch is the primary integration branch for ongoing development.

It may contain:

- features under active development;
- domain and application changes;
- infrastructure changes;
- documentation updates;
- completed work that has not yet been released.

Commits are integrated into `develop` frequently.

The Continuous Integration workflow for `develop` should remain relatively fast and provide early feedback through:

- dependency restoration;
- solution build;
- unit tests.

Integration tests are not required on every push to `develop` while the integration test suite depends on temporary infrastructure and has a higher execution cost.

---

### `main`

The `main` branch represents the stable and deployable version of GameMarketIntel.

Direct commits to `main` are not allowed.

Changes must enter `main` through a Pull Request, normally using:

```text
develop → main
```

The branch is protected and should only receive complete, validated features.

A successful merge into `main` may trigger the production deployment workflow.

---

## Validation Flow

The expected delivery flow is:

```text
Development
      ↓
Commits to develop
      ↓
Fast Continuous Integration
├── Restore
├── Build
└── Unit Tests
      ↓
Feature or release scope completed
      ↓
Pull Request
develop → main
      ↓
Release Validation
├── Restore
├── Build
├── Unit Tests
└── Critical Integration Tests
      ↓
Required checks approved
      ↓
Merge into main
      ↓
Continuous Deployment
```

---

## Continuous Integration on `develop`

The CI workflow executed for `develop` is intended to provide fast feedback during implementation.

It validates:

- package restoration;
- compilation of the complete solution;
- unit test results.

The workflow should fail when:

- the solution cannot be restored;
- the solution cannot be compiled;
- any unit test fails.

The objective is to identify problems early without requiring the heavier integration environment for every development commit.

---

## Pull Request Validation for `main`

Pull Requests targeting `main` must execute the broader validation suite before they can be merged.

The validation should include:

- package restoration;
- solution build;
- unit tests;
- critical integration tests.

Integration tests must run before the merge because executing them only after the code reaches `main` would allow unvalidated code to enter the production branch.

The Pull Request validation acts as the release quality gate.

---

## Integration Test Scope

Integration tests should focus on critical interactions that cannot be adequately validated through isolated unit tests.

The initial integration test scope should include:

1. Persisting and loading a `Game` with associated `Genre` and `Platform` entities.
2. Verifying the many-to-many relationships between games and genres.
3. Verifying the many-to-many relationships between games and platforms.
4. Ensuring that duplicate `Genre.NormalizedName` values are rejected.
5. Ensuring that duplicate `Platform.NormalizedName` values are rejected.

The integration test suite should remain focused on high-value persistence and infrastructure risks.

Business rules already covered by unit tests should not be unnecessarily duplicated in integration tests.

---

## Integration Test Infrastructure

Integration tests use a temporary PostgreSQL container managed by Testcontainers.

The expected lifecycle is:

```text
Integration test execution
          ↓
Temporary PostgreSQL container created
          ↓
Database connection configured
          ↓
EF Core migrations applied
          ↓
Integration tests executed
          ↓
Temporary container removed
```

The integration test database must remain isolated from:

- the local development database;
- the production Neon database;
- persistent project data.

This makes the tests repeatable and prevents test execution from modifying development or production information.

---

## Continuous Deployment

The deployment workflow must run only after validated code is merged into `main`.

Expected flow:

```text
Pull Request validation succeeds
              ↓
Pull Request approved
              ↓
Merge into main
              ↓
Production deployment
```

Deployment must not replace the Pull Request validation stage.

The `main` branch should already represent a validated and deployable state before the deployment workflow begins.

---

## Branch Protection

The `main` branch should require:

- Pull Requests before merging;
- successful required status checks;
- successful unit tests;
- successful critical integration tests;
- resolved review discussions when applicable.

Direct pushes to `main` should remain disabled.

---

## Current Strategy

The current GameMarketIntel strategy is:

| Stage | Validation |
|---|---|
| Push to `develop` | Restore, build, and unit tests |
| Pull Request to `main` | Restore, build, unit tests, and critical integration tests |
| Merge into `main` | Continuous Deployment |

This strategy may be revised if the integration test suite grows significantly or begins to affect delivery time.

Any future optimization should preserve the rule that critical integration validation occurs before code enters `main`.