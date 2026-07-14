# Database and Data Operational Risks

## Purpose

This register records the main database and data risks accepted after selecting Neon PostgreSQL for the GameMarketIntel zero-cost portfolio phase.

The register should be reviewed when storage or compute usage grows materially, provider limits change, new data sources are added, or the application moves beyond the portfolio phase.

## Risk Register

| ID | Risk | Impact | Likelihood | Mitigation | Trigger / Monitoring | Contingency |
|---|---|---|---|---|---|---|
| DB-01 | Neon Free storage becomes insufficient | New writes may fail and data collection may stop | Medium | Store normalized data only; store images as URLs; keep raw CSV, JSON, HTML, PDFs, and dumps outside PostgreSQL; review indexes and retention | Review at 50%; prepare action at 70%; execute capacity action before 80% | Aggregate or archive eligible history, migrate to another PostgreSQL provider, or approve a paid Neon plan |
| DB-02 | Historical snapshots grow faster than expected | Storage pressure and slower queries | Medium | Measure growth by table and index; retain only required granularity; add indexes only for demonstrated query needs | Monthly usage review and after major imports | Archive exportable history and introduce approved aggregation without changing required product semantics |
| DB-03 | Free compute or network limits affect availability | API requests or collection jobs may be interrupted | Low to Medium | Monitor usage, optimize queries, use pooled runtime connections, and avoid unnecessary data transfer | Provider usage alerts or recurring latency/failure patterns | Reduce workload, reschedule collectors, migrate plan/provider, or introduce caching where appropriate |
| DB-04 | Community Terraform provider becomes unmaintained or incompatible | Infrastructure changes may become unreliable | Medium | Pin versions, commit `.terraform.lock.hcl`, review releases, and keep manual provisioning steps documented | Failed upgrades, stale releases, unresolved provider issues | Provision manually through supported provider APIs or migrate infrastructure management without changing the database engine |
| DB-05 | Credentials or Terraform state are exposed | Unauthorized database access | Low | Keep secrets out of Git, use User Secrets and protected environment variables, ignore sensitive state and variable files, and review logs | Repository scans, CI checks, and credential review | Rotate affected credentials immediately and review access history |
| DB-06 | Backup or restoration procedure fails when needed | Data loss or extended recovery time | Low | Create portable `pg_dump` backups through a direct connection and periodically validate `pg_restore` in an isolated PostgreSQL database | Scheduled backup review and after schema changes | Correct the backup workflow, restore the latest valid backup, or migrate using standard PostgreSQL tools |
| DB-07 | Provider Free-plan limits, pricing, or policies change | Unexpected migration, cost, or availability pressure | Medium | Revalidate provider terms periodically and preserve provider-neutral PostgreSQL boundaries | Provider announcements and quarterly architecture review | Migrate using PostgreSQL-compatible tooling or approve a controlled paid plan |
| DATA-01 | External image URLs become unavailable or change | Broken images and degraded content quality | Medium | Store stable HTTPS URLs with source and attribution metadata; validate URLs during collection; provide a UI fallback image | Link validation failures or user reports | Replace the URL from an approved source, remove the unavailable asset, or adopt external object storage when justified |
| DATA-02 | Raw source data is lost after transformation | Reduced auditability or inability to reprocess a source | Low to Medium | Preserve important raw artifacts outside the operational database when licensing and storage allow; record source URL, collection date, and transformation metadata | New source onboarding and collector changes | Re-download from the source, restore archived artifacts, or document the source limitation |
| DATA-03 | Data-source definitions or values change over time | Misleading comparisons or incorrect market signals | Medium | Record source, period, units, region, reliability, limitations, and collection timestamp; validate transformations | Collector validation and anomaly review | Correct affected snapshots, recalculate derived signals, and document the change |

## Operating Rules

- Database capacity must be reviewed before importing a large new dataset.
- Images should be represented by URLs, not stored as database binary content.
- Raw source artifacts and backups must remain outside version control and outside the operational database.
- A direct database connection should be used for migration and backup operations; the pooled connection should be used for normal runtime access.
- A provider migration or paid-plan decision should happen before capacity becomes an incident.