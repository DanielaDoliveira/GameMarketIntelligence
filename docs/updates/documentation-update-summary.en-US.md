# Documentation Update Summary

> Updated: July 27, 2026

## Purpose

Record the documentation changes completed after the external-source benchmarking and the first frontend milestone.

## Main updates

- Added bilingual documentation using the `.en-US.md` and `.pt-BR.md` naming convention.
- Consolidated the external-source benchmark and removed the outdated assumption that the MVP required exactly three general sources.
- Confirmed the selected MVP source set:
  - IGDB as the primary general catalog;
  - Wikidata for reconciliation and enrichment;
  - Steam as an official specialized source.
- Classified SteamDB as an external manual-research reference with no ingestion.
- Recorded the reasons for not selecting RAWG, MobyGames, Nintendo, Microsoft/Xbox, PlayStation, and general marketplaces for MVP ingestion.
- Updated the Comparable Games domain direction to preserve provenance, external references, reconciliation confidence, conflicts, and temporal observations.
- Reinforced the migration gate: no significant migration before field mapping, permissions, reconciliation rules, domain review, and a small ingestion proof of concept.
- Updated the implementation roadmap so the current focus is source-to-field mapping and ingestion design.
- Updated the frontend brief to reflect:
  - the visible `Search` button;
  - unified form submission;
  - URL-backed applied state;
  - responsive navigation;
  - reusable components;
  - current pending validation with representative data.
- Removed duplicated product explanations and kept each document focused on its own responsibility.

## Current documentation structure

| Area | Purpose |
|---|---|
| `docs/data` | Source evaluation rules, benchmark, selection, and detailed assessments |
| `docs/design` | Frontend interaction and visual direction |
| `docs/development` | Version-control and delivery workflow |
| `docs/domain` | Comparable Games domain foundation and modeling rules |
| `docs/planning` | Implementation milestones and next work |
| `docs/product` | Product vision, value proposition, producer needs, and MVP scope |
| `docs/updates` | Documentation change summaries |

## Current project direction

The source-research stage is complete.

The next documentation and implementation work should define:

1. which producer question each selected source supports;
2. the permitted and required fields;
3. provenance and reliability rules;
4. external-reference and reconciliation models;
5. ingestion cadence, retries, checkpoints, and idempotency;
6. domain changes;
7. migrations only after model approval.

## Maintenance rule

Whenever a documented decision changes, both language versions should be updated in the same pull request.
