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
   - a sample of records where `parent_game` is populated;
   - a sample filtered by `game_type` for DLCs;
   - a sample filtered by `game_type` for remakes;
   - samples filtered by additional `game_type` values;
   - the complete IGDB `game_types` reference list;
   - a controlled release-date sample comparing first release, platform,
     region, precision, and later releases;
   - a controlled sample comparing commercial and community-origin products
     through company, external-distribution, and website evidence;
   - a fixed 100-game sample used to inspect `alternative_names`,
     `version_title`, and `game_localizations`.
4. Deserializes the response into IGDB-specific contracts.
5. Writes selected fields to the application log.
6. Stops the application after the execution finishes.

No data is currently persisted.

## Fields currently evaluated

The current queries retrieve:

- `id`
- `name`
- `alternative_names.name`
- `alternative_names.comment`
- `version_title`
- `game_localizations.name`
- `game_localizations.region`
- `first_release_date`
- `release_dates.date`
- `release_dates.human`
- `release_dates.date_format`
- `release_dates.platform`
- `release_dates.region`
- `release_dates.status`
- `updated_at`
- `game_type`
- `game_status`
- `version_parent`
- `parent_game`
- `platforms`
- `genres`
- `themes`
- `keywords`
- `involved_companies`
- `external_games`
- `websites`

The company, external-game, and website fields were added for a controlled
investigation of catalogue-eligibility signals. They remain source evidence and
do not independently prove commercial authorization.

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
- `release_dates`
- release-date component fields such as `day`
- `release_dates.status`
- `game_status`
- `version_parent`
- `parent_game`

The Collector contracts must preserve this nullability and must not assume that
all records contain these values.

### Game type

The `/v4/game_types` endpoint returned the following complete reference list:

```text
0  - Main Game
1  - DLC
2  - Expansion
3  - Bundle
4  - Standalone Expansion
5  - Mod
6  - Episode
7  - Season
8  - Remake
9  - Remaster
10 - Expanded Game
11 - Port
12 - Fork
13 - Pack / Addon
14 - Update
```

Types directly inspected so far include:

```text
DLC
Expansion
Bundle
Standalone Expansion
Mod
Remake
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


### DLC observations

A sample filtered by:

```text
where game_type = 1;
```

returned ten records classified as `DLC`.

Observed coverage in this sample:

| Field | Coverage |
|---|---:|
| `game_type = DLC` | 10/10 |
| `parent_game` | 10/10 |
| `version_parent` | 0/10 |
| `game_status` | 0/10 |
| `platforms` | 10/10 |
| `genres` | 9/10 |
| `themes` | 8/10 |
| `keywords` | 3/10 |
| `first_release_date` | 10/10 |

Observed examples:

```text
The Callisto Protocol: Final Transmission
- Game type: DLC
- Parent game: The Callisto Protocol
- Version parent: null
```

```text
Hearthstone: Ashes of Outland - Trial by Felfire
- Game type: DLC
- Parent game: Hearthstone: Ashes of Outland
- Version parent: null
```

```text
Invincible Vs.: Universa - Additional Fighter
- Game type: DLC
- Parent game: Invincible Vs.
- Genres: none
- Themes: none
- Keywords: none
```

In this sample, `parent_game` consistently identified the base product, while
`version_parent` was always null.

The sample also showed that a DLC record can remain useful even when editorial
metadata such as genres, themes, or keywords is missing. Those fields must
therefore remain optional in the ingestion boundary.

Keyword coverage was substantially lower than platform and release-date
coverage. Some returned keywords were also technical or storefront-related,
such as `steam achievements` and `steam families`. Keywords should not be
treated as a uniform editorial taxonomy.

The query was sorted by `updated_at desc`, so this result is suitable for
structural inspection but not sufficient for statistically representative
coverage measurements.

### Remake observations

A sample filtered by:

```text
where game_type = 8;
```

returned ten records classified as `Remake`.

Observed coverage in this sample:

| Field | Coverage |
|---|---:|
| `game_type = Remake` | 10/10 |
| `parent_game` | 10/10 |
| `version_parent` | 0/10 |
| `game_status` | 0/10 |
| `platforms` | 10/10 |
| `genres` | 10/10 |
| `themes` | 10/10 |
| `keywords` | 9/10 |
| `first_release_date` | 10/10 |

Observed examples:

```text
Resident Evil 4
- Game type: Remake
- Parent game: Resident Evil 4
- Version parent: null
```

```text
The Last of Us Part I
- Game type: Remake
- Parent game: The Last of Us
- Version parent: null
```

```text
Black Mesa
- Game type: Remake
- Parent game: Half-Life
- Keywords include: mod origin, fangame
```

```text
GoldenEye 007
- Remake IGDB ID: 1647
- Parent game IGDB ID: 1638
- Both records use the same name
```

```text
The Catapult
- Remake IGDB ID: 247118
- Parent game IGDB ID: 316516
- Both records use the same name
```

The remake sample showed the same relationship pattern observed for DLCs:
`parent_game` was populated in every record and `version_parent` was always
null. The meaning of the relationship therefore depends on evaluating
`game_type` together with `parent_game`.

The same-name examples confirm that names cannot be used as product identity.
External identifiers and explicit source relationships must be preserved.

The `Black Mesa` example also shows that the current source classification may
coexist with historical or contextual keywords. The Collector should preserve
the source-provided classification and provenance rather than attempting to
correct or reinterpret it during extraction.

As with the DLC query, sorting by `updated_at desc` introduces recency bias and
does not replace a larger coverage and nullability study.

### DLC and remake comparison

| Observation | DLC sample | Remake sample |
|---|---:|---:|
| Records | 10 | 10 |
| `parent_game` populated | 10/10 | 10/10 |
| `version_parent` populated | 0/10 | 0/10 |
| `game_status` populated | 0/10 | 0/10 |
| Platforms present | 10/10 | 10/10 |
| Genres present | 9/10 | 10/10 |
| Themes present | 8/10 | 10/10 |
| Keywords present | 3/10 | 9/10 |
| First release date present | 10/10 | 10/10 |

These samples support the following provisional interpretation:

```text
game_type
→ what kind of product the record represents

parent_game
→ which originating or base product the record is related to

version_parent
→ an edition or version relationship observed separately
```

This interpretation remains provisional and must not be converted into a
universal domain rule before broader controlled and coverage tests are complete.

### Commercial catalogue eligibility signals

A controlled sample compared three records with different production and
distribution contexts:

```text
6739   - Black Mesa
132181 - Resident Evil 4
294763 - Mario Party: Love Land
```

The query expanded:

- `involved_companies`
- `external_games`
- `websites`

#### Black Mesa

Observed evidence:

```text
Game type: Remake
Parent game: Half-Life
Developer and publisher: Crowbar Collective
External distribution: Steam
Trusted website link: Steam
```

Black Mesa has a community and mod origin, but the current IGDB record contains
clear evidence of a separately distributed market product.

This example shows that historical mod origin does not necessarily describe the
current commercial status of a product.

#### Resident Evil 4

Observed evidence:

```text
Game type: Remake
Parent game: Resident Evil 4
Developer: Capcom Development Division 1
Supporting company: M-TWO
Publisher: Capcom
External distribution: Microsoft, Steam, PlayStation Store, App Store, and
other retail references
```

This record provides strong and redundant evidence of an official market
product.

#### Mario Party: Love Land

Observed evidence:

```text
Game type: Mod
Parent game: Mario Party 3
Developer: narwhal88
Publisher field: Mario Party Legacy
External games: none
Website: project website, trusted = false
```

This case demonstrates that an IGDB company marked as `publisher` does not by
itself prove that the product is commercially published or authorized by the
holder of third-party intellectual property.

Likewise, a website typed as `Official Website` means that it is presented as
the official site of that project. It does not prove that the project is an
officially licensed product.

#### Interpretation of `trusted`

The `trusted` field belongs to an IGDB website reference.

For the current proof of concept, it is treated as source-provided confidence in
the website link or its classification. It must not be interpreted as proof
that:

- a game is commercially distributed;
- a product is legally authorized;
- third-party intellectual property is licensed;
- a publisher is the rights holder.

The field may later help decide which links are displayed or emphasized, but it
does not determine catalogue eligibility.

#### Storefront evidence

Recognized storefronts are strong positive evidence for modern distribution,
but they cannot be required for all catalogue records.

Many historically commercial games were distributed through cartridges,
arcades, disks, CD-ROMs, or physical retail before digital storefronts existed.
Some have never received a digital re-release.

Therefore:

```text
recognized digital storefront present
→ useful positive evidence

recognized digital storefront absent
→ not evidence that the game was non-commercial
```

#### Provisional MVP catalogue rule

To keep the first implementation functional and simple, the provisional rule
is:

```text
game_type = Mod
→ do not expose the record in the general application search

game_type != Mod
→ allow the record to continue through the normal pipeline
```

The IGDB identifier for `Mod` is `5`.

This rule intentionally does not use:

- website `trusted`;
- storefront presence;
- the existence of a developer;
- the existence of a publisher.

Those fields remain useful metadata but are not sufficiently reliable as
standalone authorization or commercial-eligibility tests.

The rule is deliberately conservative and imperfect. Known limitations include:

- a commercially released product still classified as `Mod` may be excluded;
- a fan game incorrectly classified as `Remake`, `Port`, `Fork`, or another type
  may pass the initial filter;
- the rule is not a legal assessment of any product.

These limitations are accepted for the MVP. A future increment may introduce a
separate community-signal layer, editorial review, stronger authorization
evidence, or explicit exceptions. No such layer is part of the current
increment.

### Remaining game-type observations

A controlled sample retrieved three records for each of the remaining types:

```text
6  - Episode
7  - Season
12 - Fork
13 - Pack / Addon
14 - Update
```

Across all fifteen returned records:

| Type | Records | `parent_game` populated | `version_parent` populated |
|---|---:|---:|---:|
| Episode | 3 | 3/3 | 0/3 |
| Season | 3 | 3/3 | 0/3 |
| Fork | 3 | 3/3 | 0/3 |
| Pack / Addon | 3 | 3/3 | 0/3 |
| Update | 3 | 3/3 | 0/3 |

#### Episode

Observed examples:

```text
Sam & Max: Beyond Time and Space - Episode 5: What's New Beelzebub?
- Game type: Episode
- Parent game: Sam & Max: Beyond Time and Space
- Version parent: null
```

```text
Hector: Badge of Carnage! - Episode 1
- Game type: Episode
- Parent game: Hector: Badge of Carnage!
- Version parent: null
```

In the observed sample, `parent_game` identified the broader game or episodic
series to which the individual episode belongs.

#### Season

Observed examples:

```text
Sea of Thieves: Season 1
- Game type: Season
- Parent game: Sea of Thieves
- Version parent: null
```

```text
Mortal Kombat 1: Invasions - Season of The Spectre
- Game type: Season
- Parent game: Mortal Kombat 1
- Version parent: null
```

In the observed sample, `parent_game` identified the game to which the season
belongs.

#### Fork

Observed examples:

```text
Predecessor
- Game type: Fork
- Parent game: Paragon
- Version parent: null
```

```text
The Ur-Quan Masters
- Game type: Fork
- Parent game: Star Control II
- Version parent: null
```

```text
Paragon: The Overprime
- Game type: Fork
- Parent game: Paragon
- Version parent: null
```

The Fork sample shows that `parent_game` is not limited to downloadable or
supplementary content. It can also identify the originating product from which a
separate project was derived.

#### Pack / Addon

Observed examples:

```text
Resident Evil 2: Extra DLC Pack
- Game type: Pack / Addon
- Parent game: Resident Evil 2
- Version parent: null
```

```text
Invincible Vs.: Titan - Streetwear Skin
- Game type: Pack / Addon
- Parent game: Invincible Vs.
- Version parent: null
```

In the observed sample, `parent_game` identified the game that receives the
pack, skin, or addon.

#### Update

Observed examples:

```text
Hollow Knight: Silksong - Sea of Sorrow
- Game type: Update
- Parent game: Hollow Knight: Silksong
- Version parent: null
- First release date: 2026-12-31
```

```text
Sekiro: Shadows Die Twice - Game of the Year Edition
- Game type: Update
- Parent game: Sekiro: Shadows Die Twice
- Version parent: null
```

```text
Hearthstone: Book of Mercenaries
- Game type: Update
- Parent game: Hearthstone
- Version parent: null
```

The Sekiro example reinforces that a title suffix such as `Game of the Year
Edition` cannot be used to infer a version relationship. IGDB classified the
record as `Update` and related it through `parent_game`.

The Hollow Knight: Silksong - Sea of Sorrow example also confirms that IGDB may
return records whose `first_release_date` is later than the Collector execution
date.

#### Consolidated relationship interpretation

The combined controlled observations now support the following provisional
interpretation:

```text
game_type
→ describes the nature of the source record

parent_game
→ identifies an originating, base, receiving, or contextual product

version_parent
→ identifies a more specific edition or version relationship when provided
```

The concrete meaning of `parent_game` depends on `game_type`.

Examples:

```text
DLC
→ content related to a base game

Remake
→ recreation of an earlier product

Fork
→ project derived from another product

Season
→ season associated with a game

Update
→ update or updated content associated with a base product
```

`version_parent` showed substantially lower coverage than `parent_game` in the
observed samples. It must be treated as complementary evidence, not as a
requirement for identifying related products.

Its absence means only that the source did not provide a relationship through
that field. It must not be interpreted as proof that no version relationship
exists.

`Bundle` remains a notable exception because observed bundle records may contain
neither `parent_game` nor `version_parent`. Bundle composition likely depends on
other source fields that have not yet been evaluated.

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

### Release dates

The controlled release-date investigation evaluated:

- `first_release_date` as the source-provided first known release of a product;
- detailed `release_dates` occurrences;
- the platform and region attached to each occurrence;
- date precision and nullable components;
- status nullability;
- later platform releases and regional releases;
- completeness differences between `platforms` and `release_dates`.

The controlled sample included:

```text
144542 - Kitaria Fables
974    - Resident Evil 4
126    - Diablo III
```

#### Kitaria Fables

Observed evidence:

```text
First release date: 2021-09-01

Nintendo Switch - Europe    - 2021-09-01
Nintendo Switch - Worldwide - 2021-09-02
PC              - Worldwide - 2021-09-02
PlayStation 4   - Worldwide - 2021-09-02
PlayStation 5   - Worldwide - 2021-09-02
Xbox One        - Worldwide - 2021-09-02
Xbox Series X|S - Worldwide - 2021-09-02
```

The first known release was a regional Nintendo Switch release one day before
the worldwide occurrences. This demonstrates that `first_release_date` is not
necessarily a worldwide release date.

It also confirms that one product can contain more than one occurrence for the
same platform when region or date differs.

#### Resident Evil 4

Observed evidence:

```text
Platforms: 13
Detailed release dates: 3
Detailed platform: Nintendo GameCube only
Regions: North America, Japan, and Europe
```

Although the game record lists thirteen platforms, its detailed
`release_dates` collection contains only the three regional Nintendo GameCube
occurrences. Dates for the numerous later ports are absent from the inspected
record.

Therefore:

```text
platform is present in game.platforms
does not imply
a detailed release date exists for that platform
```

The absence of a detailed occurrence must mean only that the source did not
provide that date on the inspected record. It must not be interpreted as proof
that the product was not released for the platform.

The console rendered the source timestamp as `01/11/2005` under the current
locale, while the source's human-readable value was `Jan 11, 2005`. This is a
presentation ambiguity, not a source-date contradiction. Logs and user-facing
dates must use an explicit, unambiguous format or localized date formatting.

#### Diablo III

Observed evidence distinguishes the original market appearance from later
platform releases:

```text
2012 - PC and Mac - Worldwide
2013 - PlayStation 3 and Xbox 360 - regional occurrences
2014 - PlayStation 4 and Xbox One - regional occurrences
```

The `first_release_date` remains the 2012 PC/Mac release. The later console
occurrences do not change that general first-release value.

This confirms that:

```text
first_release_date
→ when this product first appeared according to IGDB

release_dates
→ known occurrences for this product, contextualized by platform and region
```

A platform-specific historical query cannot rely only on
`first_release_date`.

#### Cross-sample observations

The controlled sample supports the following conclusions:

- `release_dates` can contain multiple occurrences for one product;
- platform and region contextualize an occurrence;
- the same platform can have different regional dates;
- a region can be `worldwide` or a specific IGDB region;
- `release_dates.status` was null throughout this controlled sample;
- the `day` component was null even when `date` and `date_format` indicated
  daily precision;
- source order must not be assumed to be chronological;
- the detailed collection can be incomplete;
- `platforms` and `release_dates` are related but non-equivalent collections;
- `first_release_date` appears to represent the first known occurrence, but it
  must be preserved as a source-provided field rather than blindly recalculated
  from an incomplete detailed collection.

#### Product identity and release-history ownership

Release occurrences belong exclusively to the IGDB product record from which
they were retrieved.

Relations such as `parent_game` and `version_parent` preserve product
parentage, but they do not authorize release histories to be merged or
inherited.

The domain decision is:

> Original games, ports, remakes, remasters, editions, bundles, and other
> source records represented as distinct products retain their own release
> dates, platforms, regions, provenance, and market context.

For example, the original *The Legend of Zelda: The Wind Waker* and *The
Legend of Zelda: The Wind Waker HD* are related works but separate market
products. Regional releases of the original may make it relevant to more than
one year. The HD product's 2013 launch belongs only to the HD product and must
not be added to the original's release history.

This distinction is essential for producer-oriented analysis. A remaster or
new edition competes for the audience, installed base, attention, and market
conditions of its own release period and platforms. A future separately
marketed version would likewise be another product if represented by the source
as a distinct record.

A later catalogue distribution of the original product can be a new
availability or release occurrence for that original product when the source
explicitly represents it as such. It still must not inherit occurrences from a
related remake or remaster.

Names must not be used to merge histories. Same-name originals and remakes can
have different IGDB identifiers, and differently named editions can still be
related. `Source + ExternalId` remains the safe identity of a record within the
source.

#### MVP release-year filter semantics

For the MVP, `Release year` is a complementary dimension of the general
comparable-games search. It is not a dedicated release-strategy section and
must not be presented as a complete recommendation of the best launch window.

The filter answers:

> Which products have at least one known release occurrence in the selected
> year?

Conceptually:

```text
ReleaseYear = selected year
→ a known release occurrence owned by this product matches the year
→ the product is included
```

The filter may match regional, worldwide, original, or later platform
occurrences, provided that the occurrence belongs to the product being
returned. Multiple occurrences in the same year do not create duplicate game
results.

`first_release_date` remains useful as source-provided evidence of the product's
first known release. It must not be treated as the only temporal field, because
that would omit later regional and platform occurrences. It also must not be
used to invent platform or region context that the aggregate field does not
contain.

The safe provisional matching rule is:

```text
year-only query
→ match any detailed release occurrence in that year
   OR the source-provided first_release_date in that year

platform + year query
→ match one detailed occurrence that contains both the selected platform
   and the selected year
→ do not fall back to first_release_date for the platform association

platform + year + region query
→ match one detailed occurrence that satisfies all three dimensions
→ do not combine dimensions from separate occurrences
```

The year-only use of `first_release_date` is evidence of a known first release,
even if the detailed collection is empty or incomplete. It does not prove a
release for any particular platform or region.

When a platform is known through `game.platforms` but no detailed date exists
for it, the application must distinguish:

```text
not associated with this platform

from

associated with this platform, but its platform release date is unavailable
in the source
```

When the user applies a temporal filter, products without sufficient temporal
evidence are omitted from that filtered result. The interface must explain that
games with missing release-date data may be absent.

#### User-interface implications

Release evidence should be presented without implying completeness.

For a result matched by a selected year, the interface may summarize the
occurrences that caused the match, for example:

```text
2003 - GameCube - EU, NA
```

Region can be represented by a compact badge or abbreviation and an accessible
full name. A globe is appropriate for `Worldwide`. Regional groups such as
Europe, Asia, and North America should not rely only on national flags because
they do not map cleanly to one country.

Possible representations include:

```text
Worldwide    → globe + accessible label
Europe       → EU + accessible label
North America→ NA + accessible label
Japan        → JP + accessible label
Australia    → AU + accessible label
Asia         → AS + accessible label
```

The exact visual design remains a frontend decision. Regardless of design,
region labels must remain understandable without relying only on color, icon,
or abbreviation.

User-facing copy should communicate the source limitation, for example:

> Results include products with a known release in the selected year. Products
> without release-date data may be omitted.

For an unfiltered product whose platform is known but whose contextual date is
not, the interface may state:

> Platform release date unavailable in the source.

#### Provisional internal concepts

The evidence supports keeping these concepts separate in later domain design:

```text
Game.FirstReleaseDate
→ optional, source-provided first known release of the product

Game.Platforms
→ platforms associated with the product, including platforms for which no
  detailed date is available

GameRelease
→ a known occurrence owned by one product and contextualized by date,
  platform, region, precision, status, and provenance where available
```

These are mapping and domain directions, not authorization to create migrations
or a final persistence model during the proof of concept.

The first MVP can expose release year as an optional general-search filter. A
future strategic release-window feature would require broader and more complete
data, contextual platform and region analysis, product similarity, and
eventually market-response evidence such as sales, interest, or engagement.

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

The later frozen-sample coverage investigation found keywords on 43 of 100
records. This is insufficient for a manual catalogue-wide filter that could be
understood as exhaustive. Keywords remain approved for nullable many-to-many
ingestion and detail display. In the first MVP, clicking one keyword opens
Comparable Games with that source-provided keyword as a contextual, removable
criterion. Manual keyword selection, multiple-keyword `AND`, and autocomplete
are deferred. The related-game result must not be presented as exhaustive.

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

## Bundle composition observations

A controlled sample of five records classified as `Bundle` was inspected with
the fields:

- `parent_game`
- `version_parent`
- `bundles`

Observed coverage:

| Field | Coverage |
|---|---:|
| `game_type = Bundle` | 5/5 |
| `parent_game` | 0/5 |
| `version_parent` | 2/5 |
| `bundles` | 1/5 |

Observed examples:

```text
Diablo III: Battle Chest
- Game type: Bundle
- Parent game: null
- Version parent: null
- Bundles: none
```

```text
Diablo III: Reaper of Souls - Ultimate Evil Edition
- Game type: Bundle
- Parent game: null
- Version parent: Diablo III
- Bundles: Diablo III: Eternal Collection
```

```text
Midway's Greatest Arcade Hits
- Game type: Bundle
- Parent game: null
- Version parent: null
- Bundles: none
```

```text
Burnout Paradise: The Ultimate Box
- Game type: Bundle
- Parent game: null
- Version parent: Burnout Paradise
- Bundles: none
```

```text
Fatal Fury: City of the Wolves - Season Pass 2
- Game type: Bundle
- Parent game: null
- Version parent: null
- Bundles: none
```

These observations confirm that Bundle records do not use `parent_game`
consistently and may or may not use `version_parent`.

The `bundles` field also does not list the contents of the current Bundle record.
Instead, the observed result suggested that it identifies bundles that contain
the current record.

### Controlled inverse bundle query

To verify the direction of the relationship, the Collector queried games with:

```text
where bundles = 90628;
```

where:

```text
90628 - Diablo III: Eternal Collection
```

The query returned two records:

```text
Diablo III: Rise of the Necromancer
- Game type: DLC
- Parent game: Diablo III
- Bundles: Diablo III: Eternal Collection
```

```text
Diablo III: Reaper of Souls - Ultimate Evil Edition
- Game type: Bundle
- Version parent: Diablo III
- Bundles: Diablo III: Eternal Collection
```

This confirms the observed direction:

```text
component record
→ bundles contains the bundles that include that record
```

It does not mean:

```text
bundle record
→ bundles contains the products included in that bundle
```

Therefore, to reconstruct the composition of a bundle, the Collector must query
games whose `bundles` relationship contains the target bundle identifier.

The test also confirms that a bundle may contain components with different
`game_type` values. In the inspected example, the containing bundle referenced:

- one `DLC`;
- one nested `Bundle`.

The import model must not assume that bundle components are always Main Games.

### Consolidated bundle interpretation

The current evidence supports the following provisional interpretation:

```text
parent_game
→ broad relationship to an originating, base, receiving, or contextual product

version_parent
→ specific edition or version relationship when provided

bundles
→ bundles that contain the current record
```

Bundle composition is therefore represented through an inverse lookup rather
than through a forward collection on the Bundle record itself.

This closes the basic `game_type` and relationship investigation for the current
proof-of-concept scope.

## Future-release admission rule

The proof of concept confirmed that IGDB can return records whose
`first_release_date` is later than the Collector execution date.

For the MVP, the provisional admission rule is:

```text
first_release_date > Collector execution date
→ do not persist the full game in the active catalogue during that execution

first_release_date <= Collector execution date
→ allow the record to continue through the normal pipeline

first_release_date = null
→ preserve as unknown; do not treat as a future release
```

This rule exists to avoid storing and exposing unreleased products before they
become relevant to the current market-analysis catalogue.

The filter must be applied in the import pipeline before persistence. The
frontend remains a presentation boundary and must not be the only place where
future records are excluded.

### Synchronization implication

A future-dated record can be imported later only if a later Worker execution
retrieves it again.

Therefore, a production synchronization strategy must not rely exclusively on:

```text
updated_at > last successful synchronization
```

A release date becoming current does not necessarily modify the IGDB
`updated_at` value.

If the Collector discards a future record and later queries only records updated
after the previous cursor, that game may never be seen again.

The MVP synchronization strategy must include a recurring release-window query,
for example:

```text
periodically query games whose first_release_date falls within
a recent overlapping window ending at the execution date
```

The exact window remains to be defined after pagination, rate-limit, and
synchronization tests. It should overlap previous executions so that a missed or
failed run does not permanently lose eligible releases.

A practical future flow is:

```text
future release encountered
→ skip active-catalogue persistence in the current execution

later recurring release-window query retrieves the record again
→ release date is now current or past
→ import normally
```

For the current proof of concept, no pending-release marker or full future-game
record needs to be persisted. This remains safe only if the later synchronization
strategy explicitly re-queries release windows and does not depend solely on
`updated_at`.

## Fixed-sample and alternative-name observations

### Reproducible sample construction

A less recency-biased sample was constructed from games whose
`first_release_date` was earlier than the fixed cutoff:

```text
Cutoff: 2026-08-04T00:00:00Z
Seed: 20260804
Population recorded for the definitive selection: 278772
Selected offsets: 100
Returned games: 100
Unique game identifiers: 100
```

The selection operation executed the 100 offset lookups in ten sequential
batches of ten requests. All ten batches completed and produced one game for
each selected offset. The offsets are retained as evidence of how the sample
was produced, but the resulting 100 identifiers are the stable input for later
field investigations.

A subsequent verification run observed a different population count despite
using the same release-date cutoff. This confirms that IGDB can add or correct
records retroactively and that offsets alone do not preserve a sample over
time. Later investigations therefore query the frozen identifiers directly.

The successful execution demonstrates that the current client and Worker can:

- coordinate sequential batches;
- complete 100 controlled offset requests;
- validate one returned record per offset;
- verify count and identifier uniqueness;
- freeze the selected identifiers for reproducible follow-up analysis;
- retrieve the frozen collection directly by identifiers in a later request.

This was not a performance, load, throughput, concurrency, or capacity test.
No latency targets, timing distribution, rate-limit headroom, retry behavior,
or production batch size were evaluated. The result is functional evidence
about controlled batching and sample reproducibility, not evidence that the
same strategy is suitable for production ingestion.

No records were persisted in the Game Market Intelligence database during this
experiment. The 100 IGDB responses were held in memory and inspected by the
proof-of-concept Worker.

### `alternative_names` sample results

The frozen identifiers were queried to compare the main `name` with
`alternative_names`, `version_title`, and `game_localizations`. The client
validated that all 100 expected games were returned with no missing,
unexpected, or duplicate identifiers.

Observed coverage:

| Observation | Result |
|---|---:|
| Games with `alternative_names` | 49/100 |
| Alternative-name records | 67 |
| Alternative names with `comment` | 65/67 |
| Games with `version_title` | 2/100 |
| Games with `game_localizations` | 15/100 |
| Localization records | 20 |
| Duplicate alternative name inside one game | 1 |
| Alternative names equal to the same game's main name | 4 |
| Exact name collision across different games | 1 |

The cross-game collision was `Game.exe`, associated with IGDB identifiers
`347230` and `403794`.

The most frequent `alternative_names.comment` categories were:

| Comment category | Count |
|---|---:|
| Windows Executable | 32 |
| Alternative title | 6 |
| Stylized title | 3 |
| Japanese title — original | 3 |
| Acronym | 3 |
| Working title | 2 |
| Russian title | 2 |
| Japanese title — translated | 2 |
| Japanese title — romanization | 2 |
| Chinese title — traditional | 2 |
| Chinese title — simplified | 2 |
| No comment | 2 |
| Other observed categories | 6 |

`Windows Executable` accounted for 32 of 67 values, or 47.76% of all
alternative names in the sample. The field is therefore not a homogeneous
collection of titles by which a market product is officially known.

The `comment` improves interpretation but does not consistently establish
language, region, official usage, commercial provenance, or the relationship
between an alias and a distributable product. Categories such as `Alternative
title`, working titles, other aliases, and uncommented values remain ambiguous.
The field also cannot determine whether the underlying game record represents
an authorized market product, fan game, ROM hack, mod, or other community-origin
content. Product eligibility must be evaluated at game-record level using
combined evidence rather than inferred from an alias.

### MVP mapping decision

`alternative_names` is excluded from the MVP mapping and must not be used for:

- public display;
- title search;
- identity;
- automatic reconciliation.

Creating a comment-category allowlist at this stage would imply a level of
officiality and provenance that the observed data does not support.

`version_title` remains semantically separate and must not be collapsed into a
generic alias collection. `game_localizations` is a more structured candidate
for regional titles because it carries an explicit region relationship, but
its low observed coverage and provenance still require a separate evaluation.
Regional structure alone must not be treated as proof of official commercial
use.

## Fixed-sample game-mode and player-perspective observations

The frozen 100 game identifiers were reused for structurally equivalent
investigations of `game_modes` and `player_perspectives`. This preserves the
same broad sample across fields and avoids treating completeness among famous
games as representative of the wider IGDB catalogue.

The 100-record sample includes the broad source population represented by the
frozen identifiers, including main games, editions, DLCs, expansions, and
community-origin content. The measurements therefore describe that sample and
must not be presented as universal catalogue rates. The targeted 26-game sample
from nine well-known series remains useful for semantic inspection only.

### `game_modes` results and decision

| Observation | Result |
|---|---:|
| Expected and returned records | 100/100 |
| Records with one or more modes | 84/100 |
| Records without modes | 16/100 |
| Records with exactly one mode | 69/100 |
| Records with multiple modes | 15/100 |
| Duplicate mode IDs inside a record | 0 |
| Invalid IDs or blank names | 0 |
| Conflicting names for the same ID | 0 |
| Distinct mode IDs observed | 5 |

The earlier targeted sample had modes in all 26 records. The 84% fixed-sample
coverage shows that this complete targeted result was not representative, while
still providing sufficient coverage for qualified MVP use.

`game_modes` is approved with caveats for ingestion, optional detail display,
comparisons, and a source-qualified public filter. It is a nullable many-to-many
relationship. A filter means that IGDB associates the selected mode with the
game record; it does not prove that excluded games lack the mode or that the
mode applies to every platform and edition.

The field does not identify a primary mode, measure prominence or quality, or
fully distinguish overlapping concepts such as multiplayer, co-operative,
split-screen, and MMO. Missing modes mean unknown source data. Associations
must not be propagated among originals, ports, remakes, remasters, editions,
updates, or other related records, and they are not strong reconciliation
evidence.

The targeted semantic inspection also returned a purported Android version of
`Super Mario Galaxy`. Because no official Android release exists, that record
cannot support claims about the official product. It reinforces that name,
platform, and relationship data do not independently prove officiality. IGDB
is accepted for the first MVP but is not authoritative; stronger cross-source
validation is deferred until the second source is integrated.

### `player_perspectives` results and decision

| Observation | Result |
|---|---:|
| Expected and returned records | 100/100 |
| Records with one or more perspectives | 45/100 |
| Records without perspectives | 55/100 |
| Records with exactly one perspective | 42/100 |
| Records with multiple perspectives | 3/100 |
| Duplicate perspective IDs inside a record | 0 |
| Invalid IDs or blank names | 0 |
| Conflicting names for the same ID | 0 |
| Distinct perspective IDs observed | 5 |

All 26 records in the targeted known-game sample contained at least one
perspective, compared with only 45% in the fixed sample. This confirms a strong
completeness bias toward prominent and well-maintained records. Coverage
decisions must therefore use the frozen 100-record sample rather than the
targeted sample.

The targeted cases remain semantically useful. Multiple perspectives may
describe different systems or contexts inside one game rather than a single
primary camera: examples included `Pokémon Red Version`, `Pokémon Sword`,
`Final Fantasy X`, and `Grand Theft Auto V`. Differences within visually
similar series records also show that classifications are not necessarily
applied consistently.

`player_perspectives` is approved for nullable many-to-many ingestion and
optional detail display. It is not approved as a public MVP filter because 55%
missing coverage would create excessive false negatives. The field does not
identify a primary or predominant perspective, has no platform- or
edition-level granularity, and must not be propagated between related records.
Absence means unknown, not that no perspective exists, and the field is not
strong reconciliation evidence.

## Fixed-sample cover and screenshot observations

The same frozen 100 game identifiers were reused to evaluate `cover` and
`screenshots`. All 100 game records were returned. The sample retains the same
broad-population limitation as the other fixed-sample investigations: it
includes main games, related products, editions, DLCs, expansions, and
community-origin content and must not be presented as a universal IGDB rate.

### `cover` results and decision

| Observation | Result |
|---|---:|
| Expected and returned records | 100/100 |
| Records with a cover | 93/100 |
| Records without a cover | 7/100 |
| Invalid image-record IDs | 0 |
| Blank `image_id` values | 0 |
| Blank URLs | 0 |
| Non-positive dimensions | 0 |
| Duplicate image IDs across games | 0 |
| Conflicting metadata for the same image ID | 0 |

`cover` is approved with legal and operational caveats as nullable visual data
for search results and game details. It is not a filter, identity attribute,
officiality proof, or strong reconciliation signal. A missing cover means only
that IGDB did not report one for that record.

The observed source dimensions varied substantially and did not always follow
a portrait-cover ratio. Examples included square and landscape values such as
`500x500`, `1024x1024`, `320x176`, and `175x150`. The UI must therefore
standardize the image container rather than distort the image: preserve aspect
ratio, use a contain-style fit, avoid mandatory cropping, and avoid excessive
upscaling that would expose compression or pixelation.

The result component will use a single mobile-first structure. A valid cover
may appear as a compact, non-dominant thumbnail on the right. If the cover is
absent or invalid, the permanent image container is omitted and text uses the
available width. Loading placeholders remain allowed, and a detail-page
composition may use a fallback when it materially improves balance. Essential
meaning must never depend on the image.

### `screenshots` results and decision

| Observation | Result |
|---|---:|
| Expected and returned records | 100/100 |
| Records with one or more screenshots | 84/100 |
| Records without screenshots | 16/100 |
| Records with exactly one screenshot | 4/100 |
| Records with multiple screenshots | 80/100 |
| Total screenshots | 507 |
| Minimum per populated record | 1 |
| Maximum per populated record | 21 |
| Average per populated record | 6.04 |
| Invalid image-record IDs | 0 |
| Blank `image_id` values | 0 |
| Blank URLs | 0 |
| Non-positive dimensions | 0 |
| Records with duplicate image IDs | 0 |
| Duplicate image IDs across games | 0 |
| Conflicting metadata for the same image ID | 0 |

`screenshots` is approved with legal and operational caveats for game details
only. It is not approved for filters or initial result cards. A detail page may
show one principal visual slot and a small number of previews; additional
images remain available on demand and should be lazy-loaded. The source order
may be retained, but it does not establish that the first image is primary,
best, or representative. Screenshots must not be propagated between related
records or used as strong reconciliation evidence.

The 507-image total demonstrates that displaying every returned screenshot by
default would produce visual density and unnecessary transfer cost. Images are
intended to provide recognition, context, and breathing room between structured
information sections, not to turn Comparable Games into an image gallery.

### Artworks decision

`artworks` was not added to the current ingestion investigation. It is deferred
rather than rejected because it may support a future visual-research,
mood-board, or art-direction capability. That use case is outside the current
Comparable Games MVP questions.

### Operational, attribution, and rights constraints

IGDB documentation states that API data may be stored and cached and describes
visible, static user-facing attribution for commercial integrations. It also
documents CDN URL construction from `image_id`, multiple size variants, and an
approximately 30-day availability period after an image is removed or replaced.
These statements support an operational integration but do not constitute an
explicit per-image copyright licence for every cover or screenshot.

The first MVP will therefore:

- store image-record IDs, `image_id`, dimensions, provenance, and synchronization
  metadata, while deferring local binary-file storage;
- build HTTPS URLs with an IGDB CDN size appropriate to the component;
- avoid image downloads, independent redistribution, and substantive image
  transformations beyond source-supported sizing;
- support refresh, disappearance, and removal without breaking textual results;
- provide visible, static attribution to IGDB;
- avoid implying that IGDB owns the underlying images and state that image
  rights remain with their respective rights holders;
- re-evaluate the terms and contact IGDB before monetization or a material
  expansion of image use.

Reference reviewed: <https://api-docs.igdb.com/>. This is a cautious operational
PoC decision, not legal advice or a conclusion that GMI owns the images.

## Consolidated coverage and nullability observations

The frozen 100 identifiers were queried once more to consolidate top-level
presence for the remaining MVP candidate fields. Every expected record was
returned, with no unexpected or duplicate identifiers.

| Field | Present | Absent |
|---|---:|---:|
| `name` | 100/100 | 0/100 |
| `summary` | 88/100 | 12/100 |
| `first_release_date` | 100/100 | 0/100 |
| `updated_at` | 100/100 | 0/100 |
| `game_type` | 100/100 | 0/100 |
| `game_status` | 7/100 | 93/100 |
| `parent_game` | 21/100 | 79/100 |
| `version_parent` | 2/100 | 98/100 |
| `platforms` | 100/100 | 0/100 |
| `genres` | 92/100 | 8/100 |
| `themes` | 61/100 | 39/100 |
| `keywords` | 43/100 | 57/100 |
| `involved_companies` | 52/100 | 48/100 |
| `collections` | 17/100 | 83/100 |
| `franchises` | 3/100 | 97/100 |
| `release_dates` | 100/100 | 0/100 |
| `external_games` | 89/100 | 11/100 |
| `websites` | 96/100 | 4/100 |

The `first_release_date` result is conditioned by sample construction: the
eligible population required a non-null value before the fixed cutoff. It is
not evidence of 100% IGDB catalogue coverage. The same caution applies to all
sample rates: they describe the frozen identifiers, not a universal source
guarantee.

Absence is not equivalent to one semantic state. `summary`, genres, themes,
keywords, involved companies, external identifiers, and websites may be
unknown or unreported. `game_status`, parent/version relationships,
collections, and franchises are also conditional and may legitimately be not
applicable. All remain nullable at the ingestion boundary unless the source
contract and admitted catalogue rules establish otherwise.

Relationship combinations were:

- 21 records with `parent_game` only;
- two records with `version_parent` only;
- zero with both;
- 77 with neither.

All 100 sampled records had platforms and detailed release dates. There were no
platforms-without-dates or dates-without-platforms cases in this execution, but
the earlier controlled `Resident Evil 4` case still demonstrates that detailed
dates may omit occurrences for some listed platforms. The consolidated result
does not override that semantic limitation.

Eighty-nine records had `external_games`, 96 had websites, and 98 had at least
one of the two. Two had neither. These fields are strong provenance and
reconciliation candidates when present but cannot be mandatory.

The 43% keyword presence led to a scope decision rather than another sampling
exercise. Keywords are ingested and displayed, and a clicked keyword can open
non-exhaustive related games. They are excluded from the first MVP's manual
filter panel. The future search boundary should accept a collection-oriented
keyword representation with source IDs and provenance so multiple-keyword
`AND` and autocomplete can be added without remodeling the data relationship.

No additional `game_type` segmentation is required for this decision. Types
and relationships were already studied in controlled samples, and the keyword
feature no longer depends on qualifying a catalogue-wide manual filter. GMI-8
is complete for the current proof-of-concept scope.

## Pagination, rate-limit, and operational-execution observations

GMI-9 used two complementary validation layers: controlled requests against
the live IGDB API for pagination and pacing, and deterministic automated tests
for failure paths that must not be intentionally provoked against the service.

The live execution counted 279,206 records eligible under the frozen
`2026-08-04T00:00:00Z` release cutoff and inspected offsets `0`, `1`, `2`,
`499`, `500`, `501`, `139603`, and `279205`, ordered by IGDB identifier.
Every valid offset returned exactly one record. No distinct offsets produced
duplicate identifiers, no valid offset was empty, and repeating offset `500`
returned identifier `506` both times. This validates controlled offset access
across the first records, the 499/500 boundary, a middle record, and the last
eligible record for that execution.

The Worker enforced a minimum interval of 275 ms between request starts. The
smallest observed interval was approximately 594.87 ms because live response
times exceeded the configured delay. All live requests returned HTTP 200, and
the execution did not intentionally exceed the documented rate limit or
provoke HTTP 429.

A dedicated `IgdbResilienceHandler` and isolated Collector test project then
validated the non-live operational paths. Eleven test cases passed, covering:

- HTTP 429 with respect for `Retry-After`;
- exponential backoff of 250, 500, and 1,000 ms when no server delay is
  supplied;
- transient HTTP 500, 502, 503, and 504 responses;
- a maximum of three retries after the original attempt;
- retry after a transient timeout;
- immediate propagation of cancellation during a retry delay;
- one token renewal after HTTP 401;
- termination rather than an authentication loop when the renewed token also
  receives HTTP 401;
- replay of cloned requests so the same `HttpRequestMessage` and content are
  not sent twice.

After the focused project passed, the complete solution suite also passed with
119 tests, providing regression evidence that the operational handler and its
registration did not break the existing application tests.

Server-provided retry delays are capped at 30 seconds for one attempt. A
single Collector instance does not currently require randomized jitter; this
must be reconsidered if multiple synchronized instances are ever deployed.

The operational policy approved by this PoC is bounded retry rather than
unlimited recovery. A failed execution must remain failed after the retry
budget is exhausted. The current PoC has no persistent synchronization
checkpoint, so checkpoint advancement and resume semantics cannot yet be
tested. The future import job must only advance a checkpoint after its complete
unit of work is committed, remain idempotent when a page is replayed, and never
mark a partial page or failed batch as completed.

GMI-9 is complete for the current proof-of-concept scope. Incremental
checkpoints, checksums, persistence idempotency, and overlapping synchronization
windows remain implementation criteria rather than validated capabilities.

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
- counting released games and retrieving records through controlled offsets;
- retrieving the frozen alternative-name sample by IGDB identifiers;
- retrieving records where `parent_game` is populated.
- searching games by name for controlled identifier discovery;
- retrieving expanded release-date data for controlled game identifiers.
- retrieving expanded cover and screenshot metadata for controlled game
  identifiers.
- delegating bounded retries, `Retry-After`, transient timeout handling, and
  one-time token renewal to `IgdbResilienceHandler`.

The client currently exposes separate operations for:

```text
GetGamesSampleAsync
→ retrieves recently updated records

GetGamesByIdsAsync
→ retrieves a controlled collection of identifiers

GetGamesWithParentAsync
→ retrieves records where parent_game is populated

GetGamesByTypeAsync
→ retrieves a controlled sample for one game_type

GetGameTypesAsync
→ retrieves the IGDB game-type reference list

SearchGamesByNameAsync
→ supports temporary controlled discovery of identifiers by name

GetReleasedGameAtOffsetAsync
→ retrieves one record at a controlled offset for sample construction

GetReleasedGamesAtOffsetsAsync
→ coordinates the selected offset lookups

GetGamesAlternativeNamesSampleAsync
→ retrieves the frozen games with alternative names, version titles, and
  localization fields
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
  expansions, standalone expansions, DLCs, and remakes.
- In the current `parent_game`, DLC, and remake samples, `version_parent` was
  null for every returned record.
- In the controlled DLC sample, all ten records used `parent_game` to identify
  the base product.
- In the controlled remake sample, all ten records used `parent_game` to identify
  the original product.
- DLC and remake records may share the same relationship field while requiring
  different interpretation through `game_type`.
- Same-name remake and original records can have different IGDB identifiers, so
  names must not be treated as identity.
- DLC and remake metadata coverage differs by field and sample; platforms and
  release dates were complete in both, while keyword coverage varied strongly.
- `game_type`, `version_parent`, and `parent_game` must be evaluated together.
- Catalogue and reconciliation rules must not be based only on names.
- Missing `game_status` must remain unknown.
- Explicit Alpha, Beta, and Early Access statuses can appear with missing or
  independent release-date information.
- Game types and parent relationships require further evaluation before domain
  or persistence decisions.
- Company, external-game, website, and storefront evidence is useful context,
  but no inspected field independently proves authorization or commercial
  eligibility.
- The `trusted` website flag must not be used as proof that a game is officially
  licensed or commercially distributed.
- Digital-store presence must not be required because it would exclude
  historically commercial games released before digital storefronts.
- For the MVP, records classified as `game_type = Mod` will not be exposed in
  the general application search.
- The Mod exclusion is a provisional catalogue rule with known false-positive
  and false-negative risks; it is not a legal determination.
- Episode, Season, Fork, Pack / Addon, and Update samples all used
  `parent_game` in the observed records and did not use `version_parent`.
- `version_parent` has shown low coverage and must remain complementary rather
  than mandatory relationship evidence.
- `parent_game` is a broad source relationship whose meaning depends on
  `game_type`.
- Observed Bundle records did not use `parent_game` and only some used
  `version_parent`.
- The `bundles` field is an inverse relationship: it identifies bundles that
  contain the current record.
- Bundle composition must be reconstructed by querying records whose `bundles`
  relationship contains the target bundle identifier.
- A bundle may contain components with different `game_type` values, including
  DLCs and nested Bundles.
- The basic `game_type` and relationship investigation is complete for the
  current proof-of-concept scope.
- `first_release_date` is the source-provided first known release of the
  product; it is not necessarily worldwide or specific to a selected platform.
- Detailed `release_dates` represent known occurrences contextualized by
  platform and region, but the collection can be incomplete.
- The presence of a platform in `game.platforms` does not guarantee that a
  detailed release date exists for that platform.
- `platforms` and `release_dates` must remain separate, non-equivalent
  collections.
- The source order of release occurrences must not be treated as chronological.
- Date precision and component fields remain nullable; a missing component must
  not be replaced with invented precision.
- The MVP `Release year` filter should match known release occurrences owned by
  the returned product, with `first_release_date` also serving as general
  source-provided evidence for year-only matching.
- A combined platform-and-year filter requires one detailed occurrence that
  satisfies both dimensions; `first_release_date` must not be used as a
  platform-specific fallback.
- When region is also selected, platform, year, and region must be satisfied by
  the same occurrence rather than assembled from separate releases.
- Missing contextual dates must be disclosed as unavailable in the source, not
  interpreted as evidence that the platform release did not occur.
- Release histories must never be inherited or merged across distinct product
  records, including originals, remakes, remasters, ports, editions, bundles,
  and other related products.
- Parent relationships provide context between products but do not change
  ownership of release occurrences.
- The release-year capability belongs initially in the general comparable-games
  filters; current source completeness does not support a dedicated launch-window
  recommendation feature.
- Region may be exposed as an accessible compact badge or abbreviation, with a
  globe for worldwide occurrences, without relying exclusively on national
  flags.
- Future-dated records must not be persisted in the active catalogue during the
  current execution.
- A future synchronization strategy must re-query overlapping release-date
  windows; relying only on `updated_at` could permanently miss previously
  skipped future releases.
- Real-source inspection is necessary before defining the final mapping.
- The Worker should coordinate execution, while future jobs, mappers, import
  services, and repositories should contain specialized responsibilities.
- The 100-record selection completed 100 sequential offset requests in ten
  batches of ten and produced 100 unique identifiers.
- A later controlled operational execution validated stable offset access from
  the first through the last of 279,206 eligible records, including the
  499/500 boundary, with no empty or duplicate results.
- Live request pacing remained within the configured 275 ms minimum start
  interval and did not intentionally provoke HTTP 429.
- Eleven focused automated cases validated bounded retry, `Retry-After`,
  transient server failures, timeout, cancellation, and one-time token renewal
  without calling the live service; the complete solution suite passed all 119
  tests afterward.
- Persistent resume, idempotent page replay, checksums, and checkpoint
  advancement remain future import-job responsibilities because the PoC does
  not yet persist synchronization state.
- Frozen identifiers are required for reproducible follow-up analysis because
  the eligible IGDB population can change retroactively even with a fixed
  release-date cutoff.
- `alternative_names` mixes potentially useful title variants with executable
  names, working titles, and ambiguous aliases.
- `alternative_names` is excluded from MVP display, search, identity, and
  reconciliation.
- `version_title` must remain separate from alternative names.
- `game_localizations` requires a separate provenance and coverage evaluation
  before any regional-title mapping decision.
- In the frozen 100-record sample, `game_modes` had 84% coverage, with 15
  records containing multiple modes and no observed structural defects.
- `game_modes` is approved as nullable many-to-many data for details,
  comparisons, and a source-qualified MVP filter; absence means unknown.
- In the same sample, `player_perspectives` had 45% coverage, with three
  records containing multiple perspectives and no observed structural defects.
- The 100% coverage of both fields in the targeted 26-game sample was not
  representative of the broader fixed sample and demonstrates a famous-game
  completeness bias.
- `player_perspectives` is approved as nullable many-to-many detail data, but
  its public filter is deferred because the missing coverage would create too
  many false negatives.
- Neither modes nor perspectives identify a primary value, guarantee
  applicability across platforms or editions, or provide strong reconciliation
  evidence.
- In the frozen 100-record sample, `cover` had 93% coverage with no observed
  structural defects; it is approved with legal and operational caveats as an
  optional, non-dominant visual in results and details.
- In the same sample, `screenshots` had 84% coverage and returned 507 images
  across 84 populated records, with no observed structural defects; screenshots
  are approved with caveats for details and on-demand galleries only.
- Missing images do not make a textual result incomplete, images do not prove
  officiality or identity, and no image association may be propagated between
  related records.
- `artworks` is deferred for a possible future visual-research or art-direction
  capability.
- The first MVP stores image metadata and uses size-appropriate IGDB CDN URLs;
  local binary storage is deferred, visible IGDB attribution is required, and
  image rights remain with their respective rights holders.
- The consolidated frozen sample measured remaining field presence and
  confirmed that optionality must be preserved for summaries, status,
  relationships, taxonomies, companies, collections, external IDs, and
  websites.
- `first_release_date` had 100% presence by sample construction and must not be
  reported as universal source coverage.
- Keywords had 43% presence; they are approved for ingestion, details, and
  single-keyword contextual navigation, while the manual keyword filter,
  multi-keyword `AND`, and autocomplete are deferred.
- GMI-8 coverage and nullability evaluation is complete for the current PoC.
- GMI-9 pagination, rate-limit, and bounded operational-failure evaluation is
  complete for the current PoC.

## Next investigations

The following points still require investigation:

1. Measure release-date completeness and nullability in a larger,
   less-biased sample, including coverage by product type, platform, region,
   precision, and historical period.
2. Validate the provisional release-year matching semantics against additional
   originals, regional launches, ports, remakes, remasters, and editions without
   merging distinct product histories.
3. Deepen the practical distinctions among product types and relationships,
   especially editions, remasters, ports, expansions, standalone expansions,
   expanded games, bundles, DLCs, remakes, and the remaining types.
4. Compare `version_parent` and `parent_game` behavior using additional controlled
   records and determine whether other relationship fields are needed.
5. Clarify the practical distinction among `Expansion`, `Standalone Expansion`,
   and `Expanded Game`.
6. The complementary-field investigation is complete for the current PoC
   scope: aliases, involved companies, collections, franchises, game modes,
   player perspectives, covers, screenshots, and the deferral of artworks have
   been classified and documented.
7. Evaluate `game_localizations` independently, including coverage, region
   semantics, duplicates, relationship to the main name, and evidence of
   official use.
8. Coverage and nullability consolidation is complete for the current PoC; a
   larger or stratified sample is future refinement, not an MVP blocker.
9. Compare metadata completeness between parent records and related products.
10. Pagination, live request pacing, token renewal, bounded retries, timeout,
   and cancellation are validated for the current PoC. Define persistent,
   idempotent checkpoint and resume behavior when the import job is designed,
   including an overlapping release-date window for skipped future releases.
11. The evaluated external-field candidates have been classified for the MVP;
   revisit only when another source or product requirement introduces new
   evidence.
12. Consolidate the proof-of-concept approval criteria (GMI-10).
13. Define which findings affect product decisions and which require an ADR.
14. Define the mapping boundary between IGDB contracts and the internal model.
15. Define the future boundary between the Worker, jobs, import services,
    mappers, and repositories.
16. Validate the documented IGDB attribution and image-rights presentation in
    the definitive user interface before release and re-evaluate it before
    monetization.
17. Revisit commercial and community-origin classification in a future
    increment only after the simple Mod exclusion has been validated in the
    working MVP.
