# Database and Data Operational Risks

## Purpose

This register records the main database and data risks accepted after selecting Neon PostgreSQL for the GameMarketIntel zero-cost portfolio phase.

The register should be reviewed when storage or compute usage grows materially, provider limits change, new data sources are added, or the application moves beyond the portfolio phase.

## Risk Register

| ID | Risk | Impact | Likelihood | Mitigation | Trigger / Monitoring | Contingency |
|---|---|---|---|---|---|---|
| DB-01 | Neon Free storage becomes insufficient | New writes may fail and data collection may stop | Medium | Treat the current ~0.5 GB storage allowance as a hard product constraint; preserve catalog coverage while limiting unnecessary metadata depth; store normalized current-state data and minimum required provenance only; store image metadata rather than binaries or ready-made URLs; keep raw CSV, JSON, HTML, PDFs, dumps, and other reprocessable artifacts outside PostgreSQL; review table and index growth | Review at 50%; investigate dominant tables at 70%; begin controlled retention/capacity action before 80%; treat 90% as an ingestion-safety threshold | Preserve canonical catalog coverage, active external identities, and provenance that supports current product state first; remove or archive eligible historical/auxiliary data from oldest to newest according to explicit retention rules; reduce non-essential high-cardinality metadata before arbitrarily truncating the catalog; if pressure remains, migrate to another PostgreSQL provider or approve a paid Neon plan |
| DB-02 | Historical or auxiliary data grows faster than expected | Storage pressure and slower queries | Medium | Do not retain detailed change history unless a concrete product need requires it; when historical data is introduced, define its retention window at the same time; measure growth by table and index; retain only required granularity; add indexes only for demonstrated query needs | Monthly usage review, after major imports, and whenever database usage approaches 70% | Prune eligible historical data from oldest to newest while preserving the most recent useful window; archive externally only when licensing and operational value justify it; introduce approved aggregation without changing required current-state product semantics |
| DB-03 | High-cardinality relationships grow faster than the canonical catalog | Free-tier storage is consumed by relationship rows and supporting indexes even when the number of games remains moderate | Medium to High | Treat keywords, contextual releases, classifications, companies, images, and other multiplicative associations as separate capacity dimensions; persist only metadata that answers approved product questions; measure both table and index size; avoid speculative indexes; prefer catalog coverage over arbitrary game-count truncation | Review after representative imports and whenever a high-cardinality table becomes a dominant share of total storage | Reduce unnecessary metadata depth, cap or defer non-essential associations based on product value, revise retention where applicable, or increase capacity; do not interpret capacity-driven metadata reduction as evidence that the source lacks the omitted data |
| DB-04 | Free compute or network limits affect availability | API requests or collection jobs may be interrupted | Low to Medium | Monitor usage, optimize queries, use pooled runtime connections, and avoid unnecessary data transfer | Provider usage alerts or recurring latency/failure patterns | Reduce workload, reschedule collectors, migrate plan/provider, or introduce caching where appropriate |
| DB-05 | Community Terraform provider becomes unmaintained or incompatible | Infrastructure changes may become unreliable | Medium | Pin versions, commit `.terraform.lock.hcl`, review releases, and keep manual provisioning steps documented | Failed upgrades, stale releases, unresolved provider issues | Provision manually through supported provider APIs or migrate infrastructure management without changing the database engine |
| DB-06 | Credentials or Terraform state are exposed | Unauthorized database access | Low | Keep secrets out of Git, use User Secrets and protected environment variables, ignore sensitive state and variable files, and review logs | Repository scans, CI checks, and credential review | Rotate affected credentials immediately and review access history |
| DB-07 | Backup or restoration procedure fails when needed | Data loss or extended recovery time | Low | Create portable `pg_dump` backups through a direct connection and periodically validate `pg_restore` in an isolated PostgreSQL database | Scheduled backup review and after schema changes | Correct the backup workflow, restore the latest valid backup, or migrate using standard PostgreSQL tools |
| DB-08 | Provider Free-plan limits, pricing, or policies change | Unexpected migration, cost, or availability pressure | Medium | Revalidate provider terms periodically and preserve provider-neutral PostgreSQL boundaries | Provider announcements and quarterly architecture review | Migrate using PostgreSQL-compatible tooling or approve a controlled paid plan |
| DATA-01 | External image renditions become unavailable or change | Missing visual support and degraded recognition | Medium | Store approved image metadata, source, dimensions, and attribution; construct size-appropriate URLs; validate availability; keep the layout complete without requiring a placeholder | Link validation failures or user reports | Remove the unavailable optional asset, refresh metadata from the approved source, or adopt controlled image storage only when rights and cost justify it |
| DATA-02 | Raw source data is lost after transformation | Reduced auditability or inability to reprocess a source | Low to Medium | Preserve important raw artifacts outside the operational database when licensing and storage allow; record source URL, collection date, and transformation metadata | New source onboarding and collector changes | Re-download from the source, restore archived artifacts, or document the source limitation |
| DATA-03 | Data-source definitions or values change over time | Misleading comparisons or incorrect market signals | Medium | Record source, period, units, region, reliability, limitations, and collection timestamp; validate transformations | Collector validation and anomaly review | Correct affected snapshots, recalculate derived signals, and document the change |

## GMI-30 Persistence and Storage Validation Evidence

GMI-30 validated the consolidated MVP persistence model locally against PostgreSQL 17 before production ingestion.

### Migration and compatibility validation

The following checks passed:

- the EF Core migration list was reviewed and remained coherent through `RemoveGameImageUrl`;
- `dotnet ef migrations has-pending-model-changes` reported no model changes since the last migration;
- the current Neon-backed database reported no pending migrations;
- the local Docker PostgreSQL database successfully accepted all pending migrations when the connection was explicitly provided with `--connection`;
- a separate empty PostgreSQL validation database successfully applied the complete migration chain from zero;
- the full solution build succeeded;
- the complete automated test suite passed with 460 of 460 tests successful;
- current public image contracts remained compatible: the backend still supplies nullable `ImageUrl` values while persisted image data remains source-aware metadata.

Development note:

- the normal `DefaultConnection` may resolve from .NET User Secrets and can therefore point to Neon;
- local persistence validation must use an explicit connection, such as a PowerShell variable passed through `dotnet ef ... --connection`, so the target database is unambiguous.

### Representative query-plan validation

A synthetic local dataset with 10,000 games was used to inspect query plans after `ANALYZE`.

Observed query behavior:

| Query path | Observed plan | Approximate local execution time | Decision |
|---|---|---:|---|
| Name search using `ILIKE '%Synthetic%'` | Sequential scan of `Games`; 9,600 of 10,000 rows removed by filter | 3.7 ms | Accept for current measured volume; the existing B-tree `NormalizedName` index does not support this substring search pattern |
| Genre filter | `IX_GameGenres_GenreId` used through bitmap index/heap scan, followed by hash join | 2.8 ms | Current index is justified; no additional index required |
| Platform filter | `IX_GamePlatforms_PlatformId` used through bitmap index/heap scan, followed by hash join | 3.2 ms | Current index is justified; no additional index required |
| Release-year filter using `EXTRACT(YEAR FROM FirstReleaseDate)` | Sequential scan of `Games`; 9,730 of 10,000 rows removed by filter | 1.8 ms | Accept for current measured volume; do not add a functional index without demonstrated need |
| Batch cover lookup | `IX_game_images_GameId` used with index scans for the requested game IDs | 3.0 ms for a 20-game page query shape | Current image index is justified |

These measurements are local validation evidence, not production service-level guarantees.

A sequential scan is not automatically considered a defect. New indexes must be supported by measured query cost and product need because indexes consume material storage and increase write cost.

### Baseline synthetic volume

The first representative dataset contained:

- 10,000 games;
- 20,000 game-genre associations;
- 20,000 game-platform associations;
- 10,000 external game records;
- 16,666 image metadata rows.

Measured storage after `ANALYZE`:

- table data: approximately 7.4 MB;
- indexes: approximately 11 MB;
- total database objects: approximately 19 MB.

Indexes already consumed more space than table data in this baseline.

### High-cardinality pressure scenario

A second synthetic phase preserved the same 10,000-game catalog and added:

- 30,000 contextual releases;
- 30,000 theme associations;
- 20,000 game-mode associations;
- 20,000 player-perspective associations;
- 80,000 keyword associations;
- 20,000 company associations;
- 5,000 collection associations;
- 2,500 product relations;
- the required canonical and external provenance records for those associations.

Measured storage after `ANALYZE`:

- table data: approximately 29 MB;
- indexes: approximately 33 MB;
- total database objects: approximately 63 MB.

The increase from the baseline was approximately 44 MB without increasing the number of games.

Largest observed relations in this pressure scenario:

| Relation | Rows | Approximate total size | Approximate total bytes per row |
|---|---:|---:|---:|
| `game_keywords` | 80,000 | 14 MB | 181 B |
| `game_releases` | 30,000 | 10.1 MB | 344 B |
| `game_images` | 16,666 | 5.3 MB | 328 B |
| `game_themes` | 30,000 | 5.3 MB | 182 B |
| `game_companies` | 20,000 | 5.3 MB | 271 B |
| `GameGenres` | 20,000 | 3.5 MB | 181 B |
| `GamePlatforms` | 20,000 | 3.5 MB | 181 B |
| `Games` | 10,000 | 3.2 MB | 327 B |

The measured bytes-per-row values include table and index storage through `pg_total_relation_size`; they are approximate budgeting evidence, not fixed storage guarantees.

### Capacity interpretation

The GMI-30 result must not be interpreted as a rule to ingest only a fixed number of games.

The preferred capacity strategy is:

1. preserve coverage of games that are relevant to the approved product scope;
2. control metadata depth and multiplicative associations according to actual product questions;
3. avoid retaining provider fields merely because the provider exposes them;
4. monitor high-cardinality relationships separately from the canonical `Games` table;
5. reduce or defer non-essential metadata before arbitrarily discarding relevant catalog entries.

For example, if a source exposes 20,000 games relevant to the MVP, the target is not automatically to keep only 10,000. The safer approach is to preserve the relevant catalog while being deliberate about how many keywords, screenshots, contextual releases, classifications, and other associations need to be persisted for each product use case.

The pressure scenario averaged roughly 6.3 KB of total measured database objects per game when all synthetic associations were included. This is a budgeting ratio only. It must not be extrapolated as a linear provider-capacity guarantee because real cardinalities, PostgreSQL page allocation, index structure, VACUUM behavior, bloat, future schema changes, and source distributions will differ.

## Operating Rules

- Database capacity must be reviewed before importing a large new dataset.
- Catalog coverage and metadata depth are separate decisions. Do not truncate a relevant game catalog solely to satisfy an arbitrary record count while non-essential high-cardinality metadata can be reduced or deferred.
- Persist source data because it supports an approved product question, required provenance, reconciliation, attribution, or operational need—not merely because the provider exposes it.
- The operational database should represent the treated current state plus the minimum provenance required to explain that state; it is not intended to be an indefinite history warehouse.
- The current approximately 0.5 GB storage allowance must be treated as a product constraint, not merely an infrastructure detail.
- Capacity actions should be staged: normal operation below 70%, investigation and growth analysis from 70%, controlled retention or capacity action before 80%, and protection of essential writes / reduction of non-essential ingestion if usage approaches 90%.
- Retention must be explicit and value-based. When historical or auxiliary data is eligible for pruning, remove the oldest eligible data first and preserve the most recent useful window.
- Retention must never reinterpret capacity-driven deletion as a source-domain fact. Synchronization/reconciliation decisions and storage-retention decisions are separate concerns.
- Current canonical entities, active external identities, and provenance that still supports the current product state have higher retention priority than historical, superseded, or auxiliary data.
- Capacity must be measured by table and index as well as at database level so that a dominant category such as images, keywords, releases, or future historical data can be constrained specifically instead of applying indiscriminate cleanup.
- High-cardinality tables must be monitored as independent capacity drivers. Current GMI-30 evidence identifies keywords and contextual releases as the strongest synthetic pressure points.
- Indexes must be created for demonstrated access paths and measurable need. Do not add indexes solely to eliminate every sequential scan.
- New historical features must define a retention policy when introduced rather than defaulting to permanent storage.
- The first MVP should store image metadata and approved source references, not database binary content or persisted rendition URLs; visible attribution and rights constraints remain mandatory.
- Raw source artifacts and backups must remain outside version control and outside the operational database.
- A direct database connection should be used for migration and backup operations; the pooled connection should be used for normal runtime access.
- Development and validation commands must make the intended database target explicit when User Secrets can override local defaults.
- A provider migration or paid-plan decision should happen before capacity becomes an incident.

## API Hosting Risks

| Risk | Impact | Mitigation | Trigger | Contingency |
|---|---|---|---|---|
| Render Free cold start | The first request after inactivity may take more than 20 seconds | Provide persistent loading feedback, communicate startup progress, and avoid presenting the delay as an application failure | Repeated startup latency affects user experience or abandonment | Evaluate paid always-on hosting or another provider |
| Render Free usage limits | Service availability may be affected when Free-plan allowances are exhausted | Monitor instance usage, outbound bandwidth, build usage, and workspace limits | Sustained growth approaches the available Free allowance | Reduce unnecessary traffic, optimize responses, or evaluate a paid hosting plan |
| Terraform Free-plan update limitation | Some infrastructure updates may fail because unsupported properties are sent by the provider | Review every Terraform plan, document manual fallback procedures, and refresh state after approved manual changes | Routine infrastructure changes repeatedly require manual intervention | Reevaluate Render Blueprints, provider versions, or another hosting platform |
| External provider dependency | API availability depends on both Render and Neon | Monitor both services, preserve portable deployment and database configurations, and document recovery procedures | Repeated provider outages or service degradation | Redeploy the Dockerized API to another compatible provider and restore PostgreSQL data from portable backups |
| Public image rendition availability | External images may become unavailable or change | Store source metadata and attribution, request size-appropriate renditions, validate links, and ensure cards/details remain complete without an image | Broken image rates become operationally relevant | Remove or refresh the optional asset; introduce controlled hosting only when rights, terms, and cost justify it |
