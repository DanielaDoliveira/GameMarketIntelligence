# Game Market Intelligence — External Source Benchmark

> Reviewed: July 27, 2026

## Purpose

Compare external sources for the GMI MVP using producer value first, followed by reliability, legal eligibility, zero-cost sustainability, coverage, complementary value, and technical feasibility.

## Eliminatory rules

- Mandatory operating cost: R$ 0.
- No unauthorized scraping or use of undocumented mechanisms.
- Storage, normalization, public display, attribution, and downstream exposure must be compatible with the source terms.
- Reliability is evaluated by data category.
- A new source must add distinct value rather than duplicate the selected set.

## Comparison

| Source | Best role | Main strength | Main limitation | Decision |
|---|---|---|---|---|
| IGDB | Primary general catalog | Broad structured metadata and advanced filters | Attribution and contractual boundaries remain applicable | Selected |
| Wikidata | Reconciliation and enrichment | CC0 data, IDs, aliases, and semantic relationships | Uneven statement-level quality and coverage | Selected |
| Steam | Official specialized source | Authoritative Steam IDs, pages, and ecosystem facts | Platform-specific; endpoint permissions must be reviewed | Selected |
| RAWG | Complementary catalog | Broad and accessible API | Strong overlap with IGDB and unclear downstream boundaries | Not selected; conditional reconsideration |
| MobyGames | Curated historical catalog | Historical depth, releases, and credits | Suitable access is paid or discretionary | Not selected |
| SteamDB | Independent Steam research | Price, player, and change history for manual research | No public API; scraping and crawling prohibited | Referenced only |
| Nintendo | Official validation | Authority over Nintendo products | No authorized public third-party catalog API identified | No MVP ingestion |
| Microsoft/Xbox | Official validation | Authority over Microsoft Store/Xbox facts | APIs are publisher-, user-, or Partner Center-oriented | No MVP ingestion |
| PlayStation | Official validation | Authority over PlayStation facts | No suitable public authorized catalog mechanism identified | No MVP ingestion |
| General marketplaces | Retail evidence | Listing and edition confirmation | Poor canonical fit and restrictive commercial terms | Not selected |

## Final source set

| Source | Responsibility |
|---|---|
| IGDB | Comparable Games catalog and filters |
| Wikidata | Reconciliation, aliases, IDs, and relationships |
| Steam | Official Steam-specific facts and references |

SteamDB may be mentioned as an optional external manual-research tool but will not feed GMI workers, storage, API contracts, or agents.

## Consequence

The benchmarking stage is complete. The next work is field selection, source mapping, reconciliation rules, ingestion proof of concept, domain review, and only then migrations.
