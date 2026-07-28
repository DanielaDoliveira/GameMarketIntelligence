# Implementation Roadmap

> Current focus: source-to-field mapping and ingestion design.

## Delivery principles

Deliver small vertical increments that answer a product question, preserve architectural boundaries, include tests and documentation, pass PR validation, remain deployable, and avoid premature modeling.

## Completed foundations

- solution, Clean Architecture boundaries, PostgreSQL, EF Core, Docker, Terraform, CI/CD, deployment;
- domain foundation for games, genres, platforms, data sources, and reliability;
- read API with filtering, pagination, validation, details, genre and platform endpoints;
- global `ProblemDetails` exception handling;
- responsive Blazor shell and Comparable Games read experience;
- visible search submission, URL state, feedback states, reusable components, typography, and responsive navigation;
- external-source research and final selection.

## Current milestone — Real-data preparation

### Goal

Transform the completed source research into an approved data and ingestion design.

### Outputs

1. source-to-product-question map;
2. allowed field inventory for IGDB, Wikidata, and Steam;
3. source attribution and legal checklist;
4. reconciliation and external-reference proposal;
5. reliability and provenance rules;
6. ingestion frequency and rate-limit plan;
7. small source-client proof of concept;
8. domain proposal and migration decision.

### Definition of done

- each imported field has a producer purpose;
- each field has a source, nature, permission status, and confidence rule;
- external IDs and reconciliation are specified;
- images and descriptions are explicitly included or deferred;
- worker flow, retries, checkpoints, and idempotency are defined;
- storage impact fits the free infrastructure;
- domain changes are approved before migrations.

## Next milestone — Initial ingestion

Potential scope:

- source adapters and DTOs;
- controlled workers;
- normalized mappings;
- external references;
- reconciliation;
- collection timestamps;
- idempotent import;
- integration tests and data-quality validation;
- GitHub Actions scheduling;
- deployment and representative-data validation.

## Following milestone — Refined MVP

- advanced filters supported by approved data;
- populated Data Sources page;
- High confidence, Balanced, and Broad coverage modes;
- clear candidate-comparable wording;
- complete real-data browser validation.

## Future

- side-by-side comparison;
- saved research;
- market observations and sales-first commercial evidence;
- historical metrics;
- agent-ready read-only interfaces;
- analytics and predictive work only after the core workflow is validated.
