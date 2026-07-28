# Game Market Intelligence — External Source Selection

> Language: English (United States)  
> Reviewed on: July 27, 2026

## Purpose

This document records the research, comparison, and final external-source decision for the **Game Market Intelligence (GMI)** MVP.

Selection was driven by producer value and the following eliminatory criteria:

- mandatory operating cost of **R$ 0**;
- authorized use compatible with each source's terms;
- no unauthorized scraping;
- support for provenance, attribution, and limitation disclosure;
- reliability assessed by data category;
- distinct complementary value, avoiding redundant integrations;
- compatibility with storage, normalization, and future exposure through GMI contracts.

> GMI only presents data it can justify, trace, and use with authorization. Missing reliable data is preferable to presenting doubtful data.

## Evaluated-source comparison

| Source | Evaluated role | Main value | Decisive limitation | MVP decision |
|---|---|---|---|---|
| **IGDB** | Primary general catalog | Broad structure for games, genres, themes, modes, perspectives, platforms, companies, releases, and external identifiers | Non-commercial use, attribution, and contractual limits must remain satisfied | **Selected** |
| **Wikidata** | Reconciliation and enrichment | External IDs, aliases, entity relationships, franchises, companies, and references | Coverage and quality vary by item and statement | **Selected** |
| **Steam** | Official specialized source | App ID, official Steam presence, product page, and Steam-specific facts | Scope and permission must be reviewed per endpoint; it does not represent the full market | **Selected** |
| **RAWG** | Complementary general catalog | Broad coverage and a straightforward API | Significant overlap with IGDB; free-plan terms and downstream exposure are not sufficiently clear for the current design | **Not selected; conditional reconsideration** |
| **MobyGames** | Curated historical catalog | Historical depth, releases, platforms, credits, and detailed curation | Suitable access depends on a paid plan or discretionary approval; additional AI restrictions apply | **Not selected** |
| **SteamDB** | Independent Steam research reference | Price history, player charts, changes, and Steam ecosystem signals | No public API; scraping and crawling are prohibited | **Manual reference only** |
| **Nintendo** | Official Nintendo validation | High authority for its own products and platforms | The available portal is focused on development and publishing; no authorized third-party catalog API was identified | **No MVP ingestion** |
| **Microsoft/Xbox** | Official Microsoft ecosystem validation | Official Microsoft Store/Xbox IDs, pages, and facts | Identified APIs are product-, publisher-, user-, and Partner Center-oriented rather than a public general catalog | **No MVP ingestion** |
| **PlayStation** | Official PlayStation validation | Potential confirmation of official pages and availability | No suitable public and authorized mechanism was identified | **No MVP ingestion** |
| **Amazon and general marketplaces** | Secondary retail evidence | Confirmation of retail listings and editions | Commercial purpose, weak canonical identity, and reuse restrictions | **Not selected** |

## Selected sources

### 1. IGDB — primary general catalog

**GMI role**

- comparable-game discovery;
- basic and advanced filters;
- initial platform, release, company, and classification structure;
- identifiers for cross-source reconciliation.

**Rationale**

IGDB provides the strongest combination of coverage, structured data, and usefulness for Comparable Games. Its API is free for non-commercial use under the published terms. Integration will use a scheduled worker, local storage, normalization, and attribution.

**Controls**

- do not treat every classification as official;
- preserve the curated nature of genres, themes, and keywords;
- review image and description rights separately;
- do not leak the IGDB schema into the GMI domain or public API.

### 2. Wikidata — open reconciliation and enrichment

**GMI role**

- connect records from different sources;
- preserve external IDs;
- retrieve alternate and multilingual names;
- enrich relationships among games, franchises, companies, and other entities;
- support references and deduplication.

**Rationale**

Wikidata structured data is released under **CC0**, providing strong compatibility with storage, transformation, and structured exposure. Its main value is not replacing the primary catalog, but complementing IGDB with an open reconciliation layer.

**Controls**

- assess reliability per statement;
- prefer referenced statements;
- do not assume uniform coverage;
- use targeted enrichment for known games instead of over-depending on the public service.

### 3. Steam — official specialized source

**GMI role**

- confirm official Steam presence;
- preserve Steam App IDs;
- provide official product pages;
- represent Steam-specific facts;
- potentially record authorized temporal observations such as price or availability.

**Rationale**

Steam is the primary authority for facts about its own ecosystem and complements IGDB with official platform evidence. It is not a general catalog replacement and must not be used to infer sales or overall commercial performance.

**Controls**

- review each endpoint and its permitted scope;
- treat price, availability, and reviews as temporal or contextual data;
- do not equate reviews, concurrent players, or rankings with sales;
- obtain Steam-origin data from Steam rather than copying SteamDB.

## Sources not selected

### RAWG

RAWG was excluded because its coverage substantially overlaps IGDB and its free-plan permissions do not provide enough clarity for normalized storage, downstream exposure through the GMI API, and future agent access. It may be reconsidered only if it demonstrates meaningful complementary coverage and compatible authorization.

### MobyGames

MobyGames offers strong historical and editorial value, but sustainable access for the intended scope depends on a subscription or discretionary approval. The MVP will not depend on an uncertain exception or a mandatory paid source.

### SteamDB

SteamDB is a valuable independent manual-research tool, but it offers no public API and explicitly disallows scraping and crawling. GMI will not create a worker for it, copy its database, or redistribute its data. It may only be documented as an external resource for human investigation.

### Nintendo, Microsoft/Xbox, and PlayStation

These organizations are official authorities for their ecosystems, but no public and authorized general-catalog mechanisms suitable for GMI were identified. They remain potential future validation sources for official facts and links, without automated MVP ingestion.

## Final MVP source set

| Source | Classification | Responsibility in GMI |
|---|---|---|
| **IGDB** | Primary general source | Comparable Games catalog and filters |
| **Wikidata** | Reconciliation and enrichment | Open IDs, aliases, relationships, and references |
| **Steam** | Official specialized source | Official Steam ecosystem facts and references |
| **SteamDB** | External reference, no ingestion | Optional manual research outside automated flows |

## Implementation consequences

- Sources will be consumed through **controlled workers**, never directly by the frontend.
- The domain will use GMI-owned concepts rather than copying external API schemas.
- Source, collection date, data nature, reliability, and conflicts must be preserved.
- Temporal data will be modeled as observations rather than permanent `Game` properties.
- Selection does not automatically authorize images, descriptions, or unrestricted redistribution; these require separate review.
- No significant migration will be created before the required and permitted fields are finalized.

## Official references reviewed

- [IGDB API Documentation](https://api-docs.igdb.com/)
- [Wikidata Licensing](https://www.wikidata.org/wiki/Wikidata:Licensing)
- [Steamworks Web API Documentation](https://partner.steamgames.com/doc/webapi_overview)
- [RAWG API Documentation](https://rawg.io/apidocs)
- [RAWG API Terms of Service](https://rawg.io/tos_api)
- [MobyGames API](https://www.mobygames.com/info/api/)
- [MobyGames API Subscription](https://www.mobygames.com/api/subscribe/)
- [MobyGames Terms of Use](https://www.mobygames.com/info/terms/)
- [SteamDB FAQ](https://steamdb.info/faq/)
- [Nintendo Developer Portal — The Process](https://developer.nintendo.com/the-process)
- [Microsoft Store Service APIs](https://learn.microsoft.com/en-us/gaming/gdk/docs/store/commerce/service-to-service/microsoft-store-apis/xstore-nav)
