# Comparable Games MVP Refinement

## Complete MVP

The complete MVP consists of three connected capabilities:

1. Comparable Games with basic and advanced filters supported by approved data.
2. Data Sources with provenance, reliability, limitations, attribution, update method, and original URLs.
3. Reliability modes: High confidence, Balanced, and Broad coverage.

## Comparable Games

Basic filters:

- name;
- genre;
- platform;
- release period/year.

Advanced filters should use progressive disclosure and may include:

- subgenre;
- themes and tags;
- game modes;
- perspective;
- developer and publisher;
- release status;
- single-player, multiplayer, and cooperative traits.

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

No migration until producer needs, approved fields, source permissions, reconciliation, domain design, and ingestion impact are reviewed.
