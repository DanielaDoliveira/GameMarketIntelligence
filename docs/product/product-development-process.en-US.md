# Product-Oriented Software Development Process

> Initial version: July 29, 2026

## Purpose

Document the practical development process used by Game Market Intelligence as the project evolves from a learning exercise into an evidence-driven product.

## Core cycle

```text
Product problem
→ hypothesis
→ research
→ legal and source eligibility
→ technical spike
→ proof of concept
→ evidence record
→ decision
→ vertical implementation
→ automated and manual validation
→ deployment
→ product review
→ documentation update
```

## Work item levels

### Research

Used to understand the product question, available evidence, legal constraints, and candidate approaches.

Output:

- research notes;
- source comparison;
- open questions;
- initial recommendation.

### Spike

A time-bounded investigation that reduces one architectural or technical risk without delivering production behavior.

Output:

- question;
- assumptions;
- findings;
- risk assessment;
- recommendation;
- exit criterion.

### Proof of concept

Uses real technology or real source data to test feasibility and behavior.

Output:

- executable experiment;
- observations;
- limitations;
- approval criteria;
- decision input.

A PoC does not automatically define the final architecture.

### Decision

Converts evidence into an approved direction.

Possible records:

- roadmap update;
- domain decision;
- source decision;
- ADR when architecture is materially affected;
- deferred decision with explicit trigger.

### Vertical implementation

Delivers a usable path across the necessary layers.

Typical path:

```text
external source
→ Collector
→ persistence
→ API
→ frontend
→ deployment
```

### Validation

Includes:

- unit tests;
- integration tests;
- endpoint tests;
- browser tests;
- production smoke tests;
- data-quality review;
- product usefulness review.

### Closure

An increment closes only after:

- code;
- tests;
- documentation;
- deployment evidence;
- known limitations;
- next-step record.

## Decision records

Use the smallest appropriate document:

- roadmap for sequencing and milestone scope;
- product document for user value and product rules;
- domain document for modeling rules;
- source assessment for legal, quality, and field evidence;
- PoC observation document for experiments;
- ADR for durable architecture decisions with meaningful alternatives;
- update summary for cross-document changes.

## Definition of Ready for implementation

A data-source increment is ready when:

- product question is defined;
- legal use is acceptable;
- source role is defined;
- required fields are identified;
- PoC evidence is sufficient;
- architectural compatibility is reviewed;
- domain and migration impacts are understood;
- Definition of Done is written.

## Definition of Done for an increment

- intended product outcome works;
- boundaries are preserved;
- tests pass;
- source and reliability context are retained;
- deployment succeeds;
- failure and rerun behavior are known;
- documentation is current;
- remaining risks are explicit.

## Suggested repository structure

```text
docs/
├── architecture/
│   ├── decisions/
│   └── explorations/
├── data/
│   ├── assessments/
│   ├── benchmarks/
│   └── proofs-of-concept/
├── development/
│   ├── product-development-process.en-US.md
│   ├── product-development-process.pt-BR.md
│   └── workflows/
├── domain/
├── planning/
├── product/
└── updates/
```

## Lightweight tracking template

For each increment, record:

```text
Problem:
User/product outcome:
Hypothesis:
Scope:
Out of scope:
Risks:
Evidence required:
Implementation layers:
Validation:
Deployment:
Documentation:
Exit criterion:
Next dependency:
```

## Continuous improvement

At the end of each milestone, review:

- what was learned;
- what caused rework;
- what assumptions were wrong;
- what documentation was missing;
- what process step should be added, removed, or simplified.

The process is intentionally adaptive. It should support the product rather than become bureaucracy.
