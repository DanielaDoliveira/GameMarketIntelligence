# Version Control Workflow

## Branches

- `main`: protected, production-ready history.
- `develop`: integration branch for completed increments.
- short-lived feature/fix/docs branches: focused work only.

## Flow

1. create a branch from `develop`;
2. implement one coherent increment;
3. update tests and documentation;
4. run local build and tests;
5. commit with a focused message;
6. open a Pull Request into `develop` or, for release integration, from `develop` into `main`;
7. require CI success and review before merge;
8. deploy through the existing pipeline.

## Rules

- no direct push to `main`;
- keep commits reviewable;
- separate documentation-only changes when that improves history;
- do not mix unrelated refactoring with feature delivery;
- record product, domain, architecture, UX, and source decisions during development;
- update status documents without marking work complete before validation;
- preserve migration and infrastructure changes in reviewable commits.

## Validation

Every merge must preserve:

- successful build;
- automated tests;
- formatting and compile correctness;
- valid deployment configuration;
- updated OpenAPI and relevant documentation;
- no exposed secrets.
