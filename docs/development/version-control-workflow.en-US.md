# Version Control Workflow

## Branches

- `main`: protected remote branch representing the code currently in production.
- `develop`: remote integration branch for completed increments.
- short-lived feature/fix/docs branches: local branches for focused work; they are not published to the remote.
- explicitly approved PoC branches: remote exceptions preserved as evidence of how a study was conducted; they do not automatically become part of the product.

The remote repository must remain intentionally small. Apart from PoC branches preserved as evidence, only `main` and `develop` are maintained on GitHub. Local branches should use a traceable pattern such as `feature/GMI-12-short-description` and should be deleted locally after their increment has been integrated and validated.

## Flow

1. update local `develop` from `origin/develop`;
2. create a short-lived local branch from `develop`;
3. implement one coherent increment;
4. update tests and documentation;
5. run local build and tests;
6. create focused commits on the local branch;
7. integrate the local branch into local `develop` only after validation;
8. validate the integrated `develop` again;
9. push only `develop`, triggering its CI workflow;
10. delete the local branch after integration is confirmed;
11. when the vertical deliverable is ready, open a Pull Request from `develop` into `main`;
12. require full Pull Request validation before merge;
13. deploy to production through the `main` pipeline.

## Rules

- no direct push to `main`;
- do not publish feature/fix/docs branches to the remote;
- do not open Pull Requests from local branches into `develop`, because local-only branches do not exist on GitHub;
- publish completed increments through `develop`;
- promote `develop` into `main` only when the integrated set is ready for production;
- do not merge PoC branches wholesale into the product; transfer only deliberately approved implementations or decisions;
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

The `develop` CI provides fast integration validation through build and tests. The Pull Request from `develop` into `main` runs the full validation required before production. After the merge into `main`, the CD pipeline publishes the approved version and validation continues in the public environment.
