# Comparable Games MVP Refinement

> Review status: updated after IGDB PoC approval on August 14, 2026.

## Complete MVP

The complete MVP consists of three connected capabilities:

1. Comparable Games with basic and advanced filters supported by approved data.
2. Data Sources with provenance, reliability, limitations, attribution, update method, and original URLs.
3. Reliability modes: High confidence, Balanced, and Broad coverage.

## Comparable Games

For this IGDB MVP, Comparable Games filtering is the first product-value
delivery. The filter set is evidence-based: each included, detail-only,
deferred, or excluded field reflects the PoC's coverage, semantics, operational
behavior, and legal constraints. The experience must provide the highest
practical confidence available within zero-cost operation while identifying
the source and communicating missing or limited data to the producer.

Basic filters:

- name;
- genre;
- platform;
- release period/year.

For the first IGDB MVP, progressive disclosure may add source-qualified themes,
game modes, involved companies, and contextual keyword navigation. Current
source decisions are:

- game modes may be a public filter, with missing source data treated as
  unknown;
- player perspectives are optional detail data, not a public filter;
- clicking a displayed keyword may open related games with one removable
  contextual keyword criterion;
- manual keyword selection, multi-keyword `AND`, and autocomplete are deferred;
- covers are optional and non-dominant in results and details;
- screenshots are limited to details and on-demand galleries.

Future iterations may evaluate or add:

- subgenre;
- richer themes and tags;
- a perspective filter if coverage becomes sufficient or can be qualified by
  complementary sources;
- developer and publisher;
- release status;
- richer single-player, multiplayer, and cooperative traits;
- manual multi-keyword filtering and saved searches.

Results are candidate comparables, not automatically direct competitors.

## Data Sources

For each source, show:

- organization and official/independent status;
- supplied categories;
- reliability by category;
- limitations;
- collection/update method;
- attribution and restrictions;
- original URL.

## Reliability modes

- **High confidence:** official, primary, strongly verified, or reliable agreement; lower coverage is expected.
- **Balanced:** official, curated, and normalized data with acceptable provenance; likely default.
- **Broad coverage:** may include recognized estimates, community data, and conflicts, always labeled.

Reliability filtering precedes source filtering because producers usually need a quality threshold before provider-level auditing.

## Domain direction

The domain may require themes, modes, companies, releases, external references, provenance, conflicts, and observations. The first source must not dictate the model.

## Migration rule

No migration for the current IGDB increment until its producer needs, approved
fields, source permissions, compatibility boundaries, domain design, and
ingestion impact are reviewed. Detailed Wikidata and Steam decisions remain
gates for their own future integrations, not for all IGDB persistence.
