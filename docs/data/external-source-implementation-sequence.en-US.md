# External Source Implementation Sequence

> Decision date: July 29, 2026
>
> Status review: August 14, 2026 — the IGDB PoC is completed and approved. The
> sequence now continues with the lightweight compatibility spike and the clean
> definitive Collector; future-source milestones remain unchanged.

## Purpose

Clarify the activation order of the already selected source strategy without changing the overall multi-source product scope.

## Decision

IGDB, Wikidata, and Steam remain planned sources.

They will be activated through separate vertical increments:

### Milestone 2

- IGDB is the first active source;
- IGDB proof of concept completed and approved;
- run a lightweight multi-source architecture compatibility spike;
- implement collection, mapping, persistence, deployment, API, frontend, provenance, and reliability presentation end to end.

### Milestone 3

- execute the Wikidata proof of concept and integration;
- execute the Steam proof of concept and integration;
- compare source observations;
- implement reconciliation and confidence;
- present convergence and divergence in the product.

## Architectural condition

The Milestone 2 IGDB implementation must preserve:

- canonical internal identifiers;
- multiple external identities;
- source-specific contracts and mappers;
- source-independent import boundaries;
- provenance;
- contextual release information;
- source-specific metadata;
- conservative reconciliation extension points.

## Non-goals of the Milestone 2 spike

The compatibility spike will not:

- authenticate against Wikidata or Steam;
- implement production clients;
- define final field mappings;
- define a complete reconciliation algorithm;
- anticipate every source-specific field.

Its purpose is only to identify structural incompatibilities before the
definitive canonical mapping and significant persistence changes are approved.
It does not reopen the completed IGDB source investigation.
