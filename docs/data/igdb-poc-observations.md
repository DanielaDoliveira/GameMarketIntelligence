# IGDB Proof of Concept — Observations

## Status

In progress.

## Purpose

This document records technical and data-quality observations collected during
the IGDB proof of concept for Game Market Intelligence.

The proof of concept does not define the final domain model, ingestion strategy,
or persistence structure. Its purpose is to inspect real IGDB responses before
those decisions are made.

## Current scope

The Collector currently performs a one-time execution that:

1. Requests an OAuth access token from Twitch.
2. Calls the IGDB `/v4/games` endpoint.
3. Retrieves either:
   - a configurable sample of recently updated game records; or
   - a controlled collection of games selected by IGDB identifiers.
4. Deserializes the response into IGDB-specific contracts.
5. Writes selected fields to the application log.
6. Stops the application after the execution finishes.

No data is currently persisted.

## Fields currently evaluated

The current queries retrieve:

- `id`
- `name`
- `first_release_date`
- `updated_at`
- `game_type`
- `game_status`
- `version_parent`
- `parent_game`
- `platforms`
- `genres`
- `themes`
- `keywords`

## Technical observations

### Authentication

Authentication through Twitch OAuth succeeded using a confidential application.

Credentials are stored with .NET User Secrets during local development.

Access tokens and client secrets are not written to application logs.

### IGDB query language

The IGDB games endpoint receives a POST request containing an Apicalypse query.

The query can expand related resources, such as:

- `platforms.name`
- `genres.name`
- `themes.name`
- `keywords.name`

Not all related resources use a `name` property.

Observed examples:

- `game_type` exposes its textual value through `type`.
- `game_status` exposes its textual value through `status`.

For this reason, `game_type` and `game_status` require contracts different from
the generic named-reference contract.

### Error diagnostics

The IGDB client reads the response body when an HTTP request fails and includes
the returned message in the thrown exception.

This provides more useful diagnostics than relying only on
`EnsureSuccessStatusCode`, which reports the HTTP status but may omit the
source-specific error details.

Authentication headers and secrets must not be included in error messages.

### Worker execution model

The current Worker runs once and then requests application shutdown.

The current execution flow is:

```text
Host starts
→ Worker ExecuteAsync starts
→ authentication service obtains a token
→ IGDB client fetches the requested games
→ Worker formats the results for inspection
→ application shutdown is requested
```

This one-time execution model is suitable for the current proof of concept.

The production scheduling strategy has not yet been defined.

## Data observations

### Nullable fields

Some fields may be absent from a game record.

Observed nullable fields include:

- `first_release_date`
- `game_status`
- `version_parent`
- `parent_game`

The Collector contracts must preserve this nullability and must not assume that
all records contain these values.

### Game type

Observed game types include:

```text
Main Game
Bundle
```

The presence of `game_type` is relevant because the games endpoint may contain
different kinds of products or related content.

Observed examples show that `game_type` alone is not sufficient to determine
whether a record represents an original game, an edition, a bundle, or another
related version.

The final inclusion rules for the Game Market Intelligence catalogue have not
yet been defined.

### Game status

Some observed records returned no `game_status`.

A missing status must not currently be interpreted as a specific state, such as
released, cancelled, or in development.

It should remain unknown until the mapping rules are defined.

The recently updated sample also included records with explicit statuses:

```text
Rhythm Racer
- Game status: Beta
- First release date: null
```

```text
Aeon Wars: Maschinen Crisis
- Game status: Alpha
- First release date: null
```

These examples show that `game_status` can provide useful information for
unreleased or in-development records, but its absence remains common and must be
preserved as unknown.

### Parent relationships

The fields `version_parent` and `parent_game` were nullable in several observed
game records.

This confirms that relationship fields may be absent and that the Collector
contracts must preserve their nullability.

The controlled sample also confirmed that `version_parent` can relate an edition
to another game record.

Observed example:

```text
Kitaria Fables: Deluxe Edition
- IGDB ID: 166686
- Game type: Main Game
- Version parent: 144542 - Kitaria Fables
- Parent game: null
```

This shows that an edition may still be classified by IGDB as `Main Game` while
being connected to another record through `version_parent`.

Therefore, `game_type` alone is not sufficient to determine whether a record is
an original game, edition, or related version.

The controlled sample also included:

```text
Hook: Complete Edition
- IGDB ID: 340742
- Game type: Bundle
- Version parent: null
- Parent game: null
```

Although the name contains `Complete Edition`, IGDB classifies this record as a
bundle and does not provide `version_parent` or `parent_game` in the current
response.

These observations show that relationship fields can be either absent or
populated depending on the record, and that catalogue rules must not infer
relationships only from the game name.

The following fields must be evaluated together:

- `game_type`
- `version_parent`
- `parent_game`
- the record name
- other relationship fields that may be evaluated later

Additional controlled examples of editions, ports, remakes, remasters,
expansions, DLCs, and bundles are still required before defining reconciliation
or catalogue-inclusion rules.

### Platforms

A game may contain multiple platforms.

Example:

```text
Advance Wars
- Wii U
- Game Boy Advance
```

The controlled sample also showed that related records may contain different
platform collections.

Example:

```text
Kitaria Fables
- Xbox Series X|S
- PlayStation 4
- PC
- PlayStation 5
- Xbox One
- Nintendo Switch
```

```text
Kitaria Fables: Deluxe Edition
- Xbox Series X|S
- Xbox One
- Nintendo Switch
```

This suggests that related versions or editions may not inherit the same
platform coverage as their parent record.

The relationship is represented by IGDB identifiers and names.

Platform reconciliation with the internal Game Market Intelligence taxonomy has
not yet been defined.

### Genres

Genres provide broad classifications.

Observed examples include:

- Strategy
- Tactical
- Simulator
- Turn-based strategy
- Role-playing
- Adventure
- Music
- Puzzle
- Racing
- Indie
- Arcade
- Visual Novel
- Platform
- Hack and slash

A single game may contain multiple genres.

The controlled sample also showed that related records may have different genre
coverage.

Example:

```text
Kitaria Fables
- Role-playing
- Simulator
- Adventure
```

```text
Kitaria Fables: Deluxe Edition
- Adventure
```

This indicates that an edition or related version may contain less complete
metadata than its parent record.

### Themes

Themes provide contextual or thematic classifications.

Observed examples include:

- Action
- Science fiction
- Warfare
- Fantasy
- Open world
- Party
- Sandbox
- Kids

Themes are distinct from genres and may be valuable for comparable-game
research.

Some records returned no themes.

The controlled sample also showed that a parent game may contain themes while a
related edition does not.

Example:

```text
Kitaria Fables
- Action
- Fantasy
- Sandbox
```

```text
Kitaria Fables: Deluxe Edition
- No themes returned
```

Their final role in filtering or ranking has not yet been decided.

### Keywords

Keywords provide substantially more detailed characteristics than genres and
themes.

Observed examples related to mechanics or gameplay include:

- turn-based
- turn-based tactics
- grid-based movement
- tactical turn-based combat
- class-based
- ability system
- naval warfare
- crafting
- farming
- metroidvania
- ragdoll physics
- online multiplayer
- character customization

Observed examples related to distribution, technical support, localization,
accessibility, or historical context include:

- digital distribution
- virtual console
- PlayStation Plus
- fan translation - Portuguese
- Wii U Virtual Console
- Game Boy Advance link cable support
- Steam achievements
- accessibility options
- keyboard-only option
- custom volume controls
- Twitch integration
- available on - Crunchyroll Game Vault

This indicates that keywords do not all represent the same semantic category.

Some records returned no keywords.

The controlled sample also showed that a parent game may contain keywords while
a related edition contains none.

Example:

```text
Kitaria Fables
- crafting
- farming
- farming simulator
- available on - Crunchyroll Game Vault
```

```text
Kitaria Fables: Deluxe Edition
- No keywords returned
```

The proof of concept currently preserves keywords as provided by IGDB.

No keyword classification, normalization, exclusion, or weighting rule has been
defined.

### Recently updated sample

The recently updated query sorts records by:

```text
updated_at desc
```

Therefore, the returned games may change between executions as IGDB records are
updated.

This behavior is expected and is not random.

The ordering is useful for studying future incremental synchronization, but it
does not produce a stable test sample.

This sample has exposed varied records, including:

- main games;
- bundles;
- editions connected through `version_parent`;
- Alpha and Beta games;
- games without release dates;
- games with missing genres, themes, or keywords;
- recently changed metadata for older games.

### Controlled sample by identifiers

A second client operation was added to retrieve a repeatable sample using a
defined collection of IGDB identifiers.

The request uses a filter equivalent to:

```text
where id = (144542,166686,340742);
sort id asc;
```

The controlled execution returned exactly the requested records, ordered by
identifier:

```text
144542 - Kitaria Fables
166686 - Kitaria Fables: Deluxe Edition
340742 - Hook: Complete Edition
```

This confirms that the Collector can currently support two distinct inspection
modes:

```text
Recently updated sample
→ dynamic records ordered by updated_at descending

Controlled identifier sample
→ repeatable records selected by known identifiers
```

The recently updated sample is useful for observing current changes in the IGDB
catalogue.

The controlled sample is useful for comparing specific classifications,
relationships, nullability, and field behavior across repeated executions.

## Current architectural boundaries

### `IgdbClient`

Responsible for:

- knowing the IGDB endpoint;
- creating the HTTP request;
- adding required headers;
- building the IGDB query;
- sending the request;
- handling HTTP response failures;
- deserializing the response into IGDB contracts;
- retrieving a recently updated sample;
- retrieving a controlled set of games by IGDB identifiers.

The client currently exposes separate operations for:

```text
GetGamesSampleAsync
→ retrieves recently updated records

GetGamesByIdsAsync
→ retrieves a controlled collection of identifiers
```

These operations represent different retrieval intentions while sharing common
HTTP request and response handling.

### `Worker`

Responsible for:

- coordinating the proof-of-concept execution;
- requesting authentication;
- choosing which client operation to execute;
- invoking the IGDB client;
- inspecting and logging the results;
- requesting application shutdown.

The formatting currently performed by the Worker exists only for
proof-of-concept inspection.

The Worker should remain an execution coordinator and should not accumulate
domain mapping, filtering, reconciliation, or persistence rules.

### Future jobs

The current `Worker.ExecuteAsync` method temporarily contains the complete
proof-of-concept task.

A future structure may separate concrete tasks into jobs, such as:

- inspecting recently updated IGDB games;
- inspecting controlled game identifiers;
- importing recently updated games;
- refreshing external metadata;
- reconciling records across sources.

In that structure, the Worker would trigger or schedule a job, while the job
would represent the complete unit of work.

### Future mapping component

The Worker should not become responsible for mapping IGDB data into the internal
Game Market Intelligence model.

A future mapper or import service should be responsible for:

- converting external timestamps;
- normalizing external values;
- applying catalogue inclusion rules;
- reconciling identifiers and relationships;
- filtering records according to import rules;
- converting external contracts into internal models.

The future flow may resemble:

```text
Worker
→ Job or import use case
→ IGDB client
→ IGDB response contracts
→ Mapper or import service
→ Internal Game Market Intelligence model
→ Repository
```

## Conclusions so far

The current proof of concept confirms that:

- Twitch OAuth authentication works.
- The IGDB games endpoint can be queried successfully.
- Related platform, genre, theme, and keyword data can be expanded.
- The client can retrieve both dynamic and controlled samples.
- The source contains useful data for comparable-game research.
- Several fields are nullable.
- Related resource schemas are not completely uniform.
- Keywords are rich but semantically heterogeneous.
- Some records contain substantially more complete metadata than related
  editions or versions.
- A record classified as `Main Game` may still represent an edition connected
  through `version_parent`.
- A record name containing `Edition` does not reliably determine its IGDB game
  type or relationships.
- A record classified as `Bundle` may have no `version_parent` or `parent_game`.
- `game_type`, `version_parent`, and `parent_game` must be evaluated together.
- Catalogue and reconciliation rules must not be based only on names.
- Missing `game_status` must remain unknown.
- Explicit Alpha and Beta statuses can appear without release dates.
- Game types and parent relationships require further evaluation before domain
  or persistence decisions.
- Real-source inspection is necessary before defining the final mapping.
- The Worker should coordinate execution, while future jobs, mappers, import
  services, and repositories should contain specialized responsibilities.

## Next investigations

The following points still require investigation:

1. Continue evaluating controlled examples for main games, editions, remakes,
   remasters, expansions, DLCs, bundles, and ports. Initial edition and bundle
   examples have been inspected.
2. Compare `version_parent` and `parent_game` behavior using records where each
   relationship is populated.
3. Determine whether additional IGDB relationship fields are needed to interpret
   editions, remakes, remasters, expansions, DLCs, bundles, and ports.
4. Evaluate release-date coverage and platform-specific release information.
5. Evaluate covers, screenshots, involved companies, franchises, and alternative
   names.
6. Measure nullability and field coverage using a larger sample.
7. Compare metadata completeness between parent records and related editions or
   versions.
8. Study rate limits and an appropriate synchronization strategy.
9. Define which external fields are candidates for the MVP.
10. Define which findings affect product decisions and which require an ADR.
11. Define the mapping boundary between IGDB contracts and the internal model.
12. Define the future boundary between the Worker, jobs, import services,
    mappers, and repositories.
13. Evaluate attribution and source-identification requirements in the user
    interface.
