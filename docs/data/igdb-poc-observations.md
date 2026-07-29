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
3. Retrieves one of the following:
   - a configurable sample of recently updated game records;
   - a controlled collection of games selected by IGDB identifiers;
   - a sample of records where `parent_game` is populated.
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
→ Worker chooses an IGDB inspection operation
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
Expansion
Bundle
Standalone Expansion
Mod
Remaster
Expanded Game
Port
```

The presence of `game_type` is relevant because the games endpoint may contain
different kinds of products or related content.

Observed examples show that `game_type` alone is not sufficient to determine
whether a record represents an original game, an edition, a bundle, a remaster,
a port, an expansion, or another related version.

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

The parent-game sample also included:

```text
Doom 3: Phobos
- Game type: Mod
- Game status: Early Access
- Parent game: Doom 3
```

These examples show that `game_status` can provide useful information for
unreleased or in-development records, but its absence remains common and must be
preserved as unknown.

### Parent relationships

The fields `version_parent` and `parent_game` were nullable in several observed
game records.

This confirms that relationship fields may be absent and that the Collector
contracts must preserve their nullability.

The controlled sample confirmed that `version_parent` can relate an edition to
another game record.

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

A separate sample filtered by:

```text
where parent_game != null;
```

returned ten records where `parent_game` was populated and `version_parent` was
null.

Observed `game_type` values in this sample included:

- `Mod`
- `Remaster`
- `Port`
- `Expanded Game`
- `Expansion`
- `Standalone Expansion`

Observed examples:

```text
Doom 3: Phobos
- Game type: Mod
- Parent game: Doom 3
- Version parent: null
```

```text
Grand Theft Auto: Vice City - The Definitive Edition
- Game type: Remaster
- Parent game: Grand Theft Auto: Vice City
- Version parent: null
```

```text
Eggconsole Zodiac PC-8801
- Game type: Port
- Parent game: Space Adventure Zodiac
- Version parent: null
```

```text
Guild Wars 2: Heart of Thorns
- Game type: Expansion
- Parent game: Guild Wars 2
- Version parent: null
```

```text
Jagged Alliance 2: Unfinished Business
- Game type: Standalone Expansion
- Parent game: Jagged Alliance 2
- Version parent: null
```

```text
Wadanohara and the Great Blue Sea -Reboot-
- Game type: Expanded Game
- Parent game: Wadanohara and the Great Blue Sea
- Version parent: null
```

These observations suggest that `parent_game` is used as a broader relationship
to a source or originating game for several kinds of related records.

However, the current sample is not sufficient to define a universal rule that
every record of these types will always use `parent_game`.

The observations also show that names containing `Edition` can be represented in
different ways:

```text
Kitaria Fables: Deluxe Edition
→ Main Game + version_parent

Grand Theft Auto: Vice City - The Definitive Edition
→ Remaster + parent_game

Hook: Complete Edition
→ Bundle + no version_parent or parent_game
```

Therefore, catalogue rules must not infer relationships only from the game name.

The following fields must be evaluated together:

- `game_type`
- `version_parent`
- `parent_game`
- the record name
- other relationship fields that may be evaluated later

Additional controlled examples of editions, ports, remakes, remasters,
expansions, DLCs, bundles, and mods are still required before defining
reconciliation or catalogue-inclusion rules.

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

The parent-game sample also showed platform differences between a related record
and its source game, including ports and remasters.

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
- Shooter
- Fighting

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
- Horror
- Comedy
- Business
- Romance

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
- gliding
- martial arts
- special attacks

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
- original soundtrack release
- PAX East 2015
- PAX South 2017

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

This confirms that the Collector can currently support a controlled inspection
mode with repeatable records selected by known identifiers.

This mode is useful for comparing specific classifications, relationships,
nullability, and field behavior across repeated executions.

### Sample with `parent_game`

A third client operation was added to retrieve records where `parent_game` is
populated.

The request uses a filter equivalent to:

```text
where parent_game != null;
sort updated_at desc;
```

The execution returned ten records, all with:

```text
Parent game: populated
Version parent: null
```

The sample included:

- mods;
- remasters;
- ports;
- expanded games;
- expansions;
- standalone expansions.

This mode is useful for investigating how IGDB represents relationships between
a related record and an originating game.

The current sample supports a provisional distinction:

```text
version_parent
→ observed for a version or edition relationship

parent_game
→ observed for several broader source-game relationships
```

This distinction remains provisional and must be tested with more controlled
examples.

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
- retrieving a controlled set of games by IGDB identifiers;
- retrieving records where `parent_game` is populated.

The client currently exposes separate operations for:

```text
GetGamesSampleAsync
→ retrieves recently updated records

GetGamesByIdsAsync
→ retrieves a controlled collection of identifiers

GetGamesWithParentAsync
→ retrieves records where parent_game is populated
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
- inspecting records with parent relationships;
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
- The client can retrieve recently updated, controlled, and parent-related
  samples.
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
- `parent_game` was observed for mods, remasters, ports, expanded games,
  expansions, and standalone expansions.
- In the current `parent_game` sample, `version_parent` was null for every
  returned record.
- `game_type`, `version_parent`, and `parent_game` must be evaluated together.
- Catalogue and reconciliation rules must not be based only on names.
- Missing `game_status` must remain unknown.
- Explicit Alpha, Beta, and Early Access statuses can appear with missing or
  independent release-date information.
- Game types and parent relationships require further evaluation before domain
  or persistence decisions.
- Real-source inspection is necessary before defining the final mapping.
- The Worker should coordinate execution, while future jobs, mappers, import
  services, and repositories should contain specialized responsibilities.

## Next investigations

The following points still require investigation:

1. Continue evaluating controlled examples for main games, editions, remakes,
   remasters, expansions, DLCs, bundles, ports, and mods. Initial edition,
   bundle, remaster, port, mod, expansion, standalone-expansion, and
   expanded-game examples have been inspected.
2. Compare `version_parent` and `parent_game` behavior using controlled records
   where each relationship is populated.
3. Find and inspect DLC examples with `parent_game`.
4. Find and inspect remake examples.
5. Clarify the practical distinction among `Expansion`, `Standalone Expansion`,
   and `Expanded Game`.
6. Determine whether additional IGDB relationship fields are needed to interpret
   editions, remakes, remasters, expansions, DLCs, bundles, ports, and mods.
7. Evaluate release-date coverage and platform-specific release information.
8. Evaluate covers, screenshots, involved companies, franchises, and alternative
   names.
9. Measure nullability and field coverage using a larger sample.
10. Compare metadata completeness between parent records and related editions or
    versions.
11. Study rate limits and an appropriate synchronization strategy.
12. Define which external fields are candidates for the MVP.
13. Define which findings affect product decisions and which require an ADR.
14. Define the mapping boundary between IGDB contracts and the internal model.
15. Define the future boundary between the Worker, jobs, import services,
    mappers, and repositories.
16. Evaluate attribution and source-identification requirements in the user
    interface.
