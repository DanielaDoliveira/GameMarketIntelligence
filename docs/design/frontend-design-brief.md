## Navigation Pattern

GameMarketIntel will use an adaptive lateral-navigation pattern inspired by modern productivity applications.

The navigation structure remains consistent across devices, while its presentation changes according to the available viewport width.

### Desktop Behavior

On desktop layouts:

- the lateral navigation is visible by default;
- the application header contains a menu button that can collapse or expand the sidebar;
- the sidebar occupies its own layout space and does not cover the page content;
- the content area resizes when the sidebar changes state;
- navigation labels and icons are visible in the expanded state;
- a future compact state may display icons only.

Initial desktop structure:

```text
Application Header
├── Menu button
├── Product identity
├── Search or page context
└── Secondary actions

Page Layout
├── Persistent sidebar
│   ├── Overview
│   ├── Comparable Games
│   ├── Genres
│   ├── Platforms
│   ├── Data Sources
│   └── Future modules
│
└── Main content
```

The sidebar should be expanded by default on sufficiently wide screens.

The user should be able to collapse it when more horizontal space is required for research or comparison content.

### Mobile Behavior

On mobile layouts:

- the sidebar is hidden by default;
- the application header contains a visible menu button;
- the menu button opens a temporary lateral drawer;
- the drawer appears above the page content;
- the remaining page receives a visual overlay while the drawer is open;
- the content remains the primary focus when navigation is not required.

Initial mobile structure:

```text
Application Header
├── Menu button
├── Search or page context
└── Optional secondary action

Main Content
└── Current page

Temporary Drawer
└── Full application navigation
```

The drawer must close when:

- the user selects a destination;
- the user activates the close button;
- the user clicks or taps outside the drawer;
- the user presses the Escape key when a keyboard is available.

### Gesture Support

Swipe gestures may be added later as progressive enhancement.

They must not be the only way to open or close the navigation because gesture-only navigation may:

- be difficult to discover;
- conflict with browser or operating-system gestures;
- reduce keyboard and mouse accessibility;
- behave inconsistently across devices.

The visible menu button remains the primary navigation control.

### Responsive Navigation Principle

The navigation should follow this progression:

```text
Mobile
→ hidden drawer opened by the header menu button

Tablet or narrow desktop
→ hidden or compact sidebar

Wide desktop
→ expanded sidebar visible by default
```

The application must preserve the same navigation hierarchy and routes across all viewport sizes.

Only the presentation and interaction model should adapt.

## Application Shell

The GameMarketIntel application shell will use four primary regions:

```text
Application shell
├── Header
├── Adaptive sidebar or mobile drawer
├── Main content
└── Footer
```

Each region must have a distinct responsibility.

The header provides product identity, contextual search, and limited secondary actions.

The sidebar provides application navigation.

The main content contains the active research experience.

The footer contains secondary product, source, and project information.

The application shell must remain visually consistent across:

- result pages;
- game-detail pages;
- loading states;
- no-results states;
- error states;
- route-not-found pages.

## Header Pattern

The application header should remain visible, lightweight, and visually consistent across pages.

Its responsibilities are:

- expose the navigation menu control;
- preserve product identity;
- provide access to the current search experience;
- reserve limited space for future secondary actions.

The header must not duplicate the navigation options already available in the sidebar.

### Desktop Header

On desktop, the header should use a three-region composition:

```text
Left
→ menu button and product identity

Center
→ contextual search field

Right
→ optional secondary action
```

Recommended structure:

```text
[ Menu ] [ GameMarketIntel ]   [ Search comparable games... ]   [ Action ]
```

The search field should occupy the central region of the header and receive most of the available horizontal space.

The product identity should remain visible on the left without competing with the search experience.

The right region should remain minimal during the first increment.

It may later support:

- user preferences;
- language selection;
- help;
- profile;
- notifications.

No placeholder avatar or unnecessary action should be introduced before a real product need exists.

### Mobile Header

On mobile, the header should use a compact search-centered composition inspired by modern productivity applications.

Recommended structure:

```text
[ Menu ]   [ Search comparable games... ]   [ Compact action ]
```

The search field should remain the dominant element.

The full product name does not need to remain visible when horizontal space is limited.

A compact product mark may appear when it does not reduce the usability of the search field.

The mobile header should preserve:

- a visible menu button;
- a comfortable search target;
- sufficient spacing between controls;
- clear focus and active states;
- support for touch and keyboard interaction.

## Search Placement

For the first frontend increment, the Comparable Games search should appear in the center of the application header.

The Comparable Games page must not repeat the same primary search field inside the main content area.

The main content should instead contain:

- the page title;
- genre and platform filters;
- active-filter chips;
- result count;
- results or feedback states.

Recommended structure:

```text
Header
└── Search comparable games

Comparable Games page
├── Page title
├── Genre and platform filters
├── Active-filter chips
├── Result count
└── Results
```

This prevents duplicate search controls and gives the application a clear visual focus.

## Search Scope

During the first increment, the header search is contextual.

On the Comparable Games page, it searches comparable games by partial name.

It is not yet a global application search.

Future versions may expand the same header search position to include:

- games;
- genres;
- platforms;
- data sources;
- market indicators.

Any future expansion to global search must clearly communicate the type and scope of the returned results.

## Footer Pattern

The application should include a lightweight footer below the main content.

The footer may contain:

- product name;
- About link;
- Data Sources link;
- repository or project link;
- data-update information when available.

Recommended desktop structure:

```text
GameMarketIntel · About · Data Sources · Last updated · GitHub
```

Recommended mobile structure:

```text
GameMarketIntel

About · Data Sources
GitHub
```

The footer must remain visually secondary and must not compete with the research workflow.

## Application Shell Decision

The initial application-shell decision is:

> Use a persistent header with a centered contextual search field, adaptive lateral navigation, a responsive main-content area, and a lightweight footer.

This pattern was selected because it:

- gives search a prominent and stable location;
- avoids duplicating search controls inside the page;
- separates navigation from search;
- supports both desktop and mobile layouts;
- preserves space for future analytical content;
- maintains a consistent visual structure across pages.

## Initial Color Direction

The first visual direction will use a light interface.

The color system should communicate:

- trust;
- calm;
- clarity;
- lightness;
- practicality;
- fluidity.

### Base Surfaces

Recommended direction:

```text
Application background
→ very light cool gray or blue-gray

Primary content surfaces
→ white

Secondary surfaces
→ subtle blue-gray

Borders
→ low-contrast neutral tone
```

The interface should avoid using pure white for every layer.

A subtle distinction between the page background, cards, navigation, and supporting surfaces should help organize content without creating visual weight.

### Primary Color

The primary color should come from a stable blue family.

It may be used for:

- active navigation;
- primary buttons;
- links;
- focus indicators;
- selected controls;
- progress feedback.

The blue should feel trustworthy and clear without becoming highly saturated or resembling a financial trading interface.

### Secondary Color

A restrained blue-green or cyan family may support the sense of:

- movement;
- progress;
- freshness;
- fluidity.

It should be used for secondary emphasis rather than competing with the primary blue.

### Accent Color

A soft violet or lilac may be used selectively for product identity.

Appropriate uses include:

- subtle gradients;
- decorative details;
- selected badges;
- loading illustrations;
- secondary highlights.

The accent must remain restrained so that the interface does not become visually dominated by purple.

### Text Colors

Recommended hierarchy:

```text
Primary text
→ dark navy or blue-black

Secondary text
→ medium cool gray

Supporting metadata
→ lighter neutral gray

Disabled text
→ low-contrast neutral tone
```

Pure black should not be required for the main interface.

A dark navy tone may preserve readability while remaining aligned with the product identity.

### Semantic Colors

The design system should define distinct colors for:

- success;
- information;
- warning;
- error.

Semantic meaning must never depend on color alone.

Icons, labels, and explanatory text should support each state.

### Initial Palette

The initial GameMarketIntel palette is:

| Design token | Hex | Primary use |
|---|---:|---|
| `background` | `#F5F7FB` | Application background |
| `surface` | `#FFFFFF` | Cards, header, and navigation |
| `surface-muted` | `#EEF2F8` | Secondary surfaces and skeletons |
| `border` | `#DDE4EE` | Subtle separators and borders |
| `text-primary` | `#172033` | Main text |
| `text-secondary` | `#5E6B80` | Supporting text and metadata |
| `text-muted` | `#8995A8` | Low-emphasis information |
| `primary` | `#315E9E` | Primary actions and active navigation |
| `primary-hover` | `#274C82` | Hover and pressed states |
| `primary-soft` | `#E7EFFA` | Selected surfaces and subtle highlights |
| `secondary` | `#3F8F9D` | Progress and secondary emphasis |
| `secondary-soft` | `#E2F2F3` | Soft secondary surfaces |
| `accent` | `#7A6FC2` | Restrained product identity accents |
| `accent-soft` | `#EFECFA` | Soft accent surfaces |
| `success` | `#2E7D5B` | Success states |
| `warning` | `#B7791F` | Warning states |
| `error` | `#B54747` | Error states |
| `info` | `#3975AD` | Informational and loading states |

These values are an initial design direction and must be validated for accessibility in their actual component combinations.

The implementation should reference semantic tokens rather than choosing arbitrary framework colors directly.

### Dark Theme

A dark theme is not part of the first frontend delivery.

The initial system should nevertheless avoid decisions that make future dark-theme support unnecessarily difficult.

Color values must be represented through reusable semantic design tokens rather than scattered literal values.

## Initial Design Tokens

The frontend should use semantic design tokens so that components reference the purpose of a color rather than repeating literal values.

Initial CSS custom properties:

```css
:root {
    --color-background: #F5F7FB;
    --color-surface: #FFFFFF;
    --color-surface-muted: #EEF2F8;
    --color-border: #DDE4EE;

    --color-text-primary: #172033;
    --color-text-secondary: #5E6B80;
    --color-text-muted: #8995A8;

    --color-primary: #315E9E;
    --color-primary-hover: #274C82;
    --color-primary-soft: #E7EFFA;

    --color-secondary: #3F8F9D;
    --color-secondary-soft: #E2F2F3;

    --color-accent: #7A6FC2;
    --color-accent-soft: #EFECFA;

    --color-success: #2E7D5B;
    --color-warning: #B7791F;
    --color-error: #B54747;
    --color-info: #3975AD;
}
```

Components must use semantic tokens rather than repeating hexadecimal values.

Example:

```css
.game-card {
    color: var(--color-text-primary);
    background: var(--color-surface);
    border: 1px solid var(--color-border);
}

.primary-action {
    color: var(--color-surface);
    background: var(--color-primary);
}

.primary-action:hover {
    background: var(--color-primary-hover);
}
```

This preserves the product identity if the underlying palette changes later.

## Styling Strategy

The initial GameMarketIntel frontend will use standard CSS and Blazor CSS isolation.

Tailwind CSS will not be introduced in the first frontend implementation.

This decision prioritizes:

- native integration with the Blazor build process;
- simpler local development;
- fewer external build dependencies;
- predictable Continuous Integration and deployment;
- direct control over responsive layouts and interaction states;
- clear separation between global design tokens and component-specific styles.

The styling structure should use:

```text
Global CSS
├── design tokens
├── typography
├── reset and base styles
├── reusable shared classes
└── application-wide accessibility styles

Blazor CSS isolation
├── application shell
├── header
├── sidebar
├── mobile drawer
├── game cards
├── loading states
├── empty states
└── error states
```

CSS Grid and Flexbox should provide the primary layout mechanisms.

Media queries should progressively adapt the mobile-first layout for tablet and desktop viewports.

### Global CSS Responsibilities

Global CSS should contain styles that intentionally apply across the application, including:

- semantic design tokens;
- font and text defaults;
- box-sizing and base resets;
- page background;
- default link behavior;
- shared focus indicators;
- reduced-motion behavior;
- reusable layout helpers when they provide clear value.

Global CSS should avoid component-specific selectors unless a style is intentionally shared.

### CSS Isolation Responsibilities

Blazor CSS isolation should be used when styles belong to a specific component or layout.

Examples:

```text
MainLayout.razor.css
→ application shell, header, sidebar, and drawer

GameCard.razor.css
→ game-card structure and responsive behavior

LoadingSkeleton.razor.css
→ skeleton appearance and animation

EmptyState.razor.css
→ empty-state presentation

ErrorState.razor.css
→ error-state presentation and retry action
```

This keeps component behavior close to its Razor markup and reduces accidental style collisions.

### Responsive Strategy

The frontend must remain mobile-first.

Base styles should represent the mobile layout.

Larger layouts should be introduced through progressive media queries.

Example direction:

```css
.comparable-games-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 1rem;
}

@media (min-width: 48rem) {
    .comparable-games-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }
}

@media (min-width: 75rem) {
    .comparable-games-grid {
        grid-template-columns: repeat(3, minmax(0, 1fr));
    }
}
```

Breakpoints should be chosen according to content behavior rather than specific device models.

### Interaction and Motion

CSS transitions and animations should remain subtle and purposeful.

They may support:

- sidebar expansion and collapse;
- mobile drawer entrance and exit;
- overlay appearance;
- loading skeletons;
- focus and active states.

The interface should respect the user's reduced-motion preference.

Example:

```css
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        scroll-behavior: auto;
        transition-duration: 0.01ms !important;
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
    }
}
```

### Styling Decision

The initial styling decision is:

> Use standard CSS, semantic CSS custom properties, CSS Grid, Flexbox, media queries, and Blazor CSS isolation.

This approach may be reviewed later if the frontend grows enough to justify an additional styling framework.


## Comparable Games Query Experience

The first functional frontend case will allow users to search and filter comparable games.

The query must support:

- partial game-name search;
- selection of one or more genres;
- selection of one or more platforms;
- removal of individual filters;
- clearing all active filters;
- result-count feedback;
- loading, no-results, error, and unavailable-data states.

### Query Combination Rules

Different filter categories must use AND semantics.

Example:

```text
Name contains "Hades"
AND
Genre matches Action OR Roguelike
AND
Platform matches PC OR Nintendo Switch
```

Multiple values inside the same category must initially use OR semantics.

For genres:

```text
Selected genres
→ Action
→ Roguelike

Result
→ games associated with Action OR Roguelike
```

For platforms:

```text
Selected platforms
→ PC
→ Nintendo Switch

Result
→ games available on PC OR Nintendo Switch
```

This behavior supports exploratory research, reduces the likelihood of empty result sets, and remains familiar to users of digital catalogs.

Support for matching all selected genres may be evaluated later for more specialized research.

A possible future control would be:

```text
Genre matching
(•) Any selected genre
( ) All selected genres
```

This control is not part of the first frontend delivery.

## Search and Filter Layout

The primary Comparable Games search field must appear in the center of the application header.

The page body must not repeat the same game-name search field.

Initial page structure:

```text
Header
└── [ Search comparable games... ]

Comparable Games

[ Genres ] [ Platforms ] [ More filters ]

Active filters
[Action ×] [Roguelike ×] [PC ×] [Clear all]

12 games found

Results
```

On mobile:

- the search field should occupy the dominant central area of the header;
- genre and platform filters may open in a temporary panel, drawer, or bottom sheet;
- active filters should remain visible as removable chips;
- the result count should appear before the result list;
- controls must remain comfortable for touch interaction.

On desktop:

- the search field should occupy the center of the header;
- filters may remain visible in a horizontal toolbar or supporting side panel;
- active filters and result count should remain visually close to the results;
- the layout may use additional horizontal space without becoming dense.

The exact filter-panel presentation will be validated in the wireframes.

## Results Layout

The result presentation must adapt to the available width.

### Mobile Results

Mobile results must use one card per row.

Cards should be stacked vertically and use the available content width.

```text
Result list
├── Game card
├── Game card
├── Game card
└── Game card
```

This approach prioritizes:

- readability;
- touch interaction;
- predictable vertical scanning;
- sufficient space for game names and metadata;
- a calm layout without compressed columns.

A multi-column result grid should not be used on narrow mobile screens.

### Desktop Results

Desktop layouts may use a responsive multi-column composition.

The result area may evolve from one column to two or three columns when the content has enough space.

```css
.comparable-games-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 1rem;
}

@media (min-width: 48rem) {
    .comparable-games-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }
}

@media (min-width: 75rem) {
    .comparable-games-grid {
        grid-template-columns: repeat(3, minmax(0, 1fr));
    }
}
```

CSS Grid may organize the result collection, while Flexbox may organize content inside each card.

The final desktop arrangement should be validated according to card width, metadata density, and image proportions rather than maximizing the number of columns.

## Initial Game Card Content

The first game card should prioritize concise research information.

Required content:

```text
Game image
Game name
Release year
Genres
Platforms
```

The card should not contain a long description.

Long text would reduce scanability, particularly on mobile, and compete with the information needed for comparison.

A future details page or expanded view may contain:

- full description;
- publisher;
- developer;
- release history;
- source information;
- market metrics;
- comparison indicators.

The initial card may use badges or compact text groups for genres and platforms.

## Experience States

The Comparable Games experience must distinguish between different application and query states.

Required states:

```text
Initial application loading
Query loading
Results available
No matching results
No data available
Request error
Route not found
```

These states must share the same visual language while differing in prominence and purpose.

## Initial Application Loading

The initial Blazor WebAssembly loading experience is a product-level state.

It must reassure the user that the application is starting rather than appearing frozen or broken.

Primary message:

> Preparing your market research workspace...

The visual direction should use:

- a centered product mark or loading symbol;
- the soft violet accent from the product palette;
- subtle expanding or rotating circular elements;
- calm, continuous motion;
- clear readable text;
- generous empty space.

The motion may take inspiration from animated circular hover effects in the visual references, but it should be adapted into a calm loading identity rather than reproduced as a button interaction.

The animation must not:

- flash aggressively;
- rotate at a distracting speed;
- imply an exact progress percentage when none is available;
- continue when reduced motion is requested.

Suggested structure:

```text
Soft animated product symbol

Preparing your market research workspace...
```

## In-Application Query Loading

Query loading must use a smaller and more discreet version of the initial loading identity.

Primary message:

> Searching comparable games...

The visual treatment may include:

- a small circular loader or product symbol;
- subtle violet or primary-blue motion;
- animated ellipsis;
- optional skeleton placeholders when the result structure is already known.

Suggested wireframe representation:

```text
[ loading icon ] Searching comparable games...
```

The query loading state should communicate activity without making the user feel that the entire application has restarted.

The relationship between both loading states is:

```text
Initial application loading
→ prominent branded experience

Query loading
→ compact functional version of the same visual identity
```

## No Results State

The no-results state appears when data exists, but no game matches the current search and filters.

It must not be presented as a technical failure.

The page should preserve the current search and filter context so the user can understand why no results were returned.

Recommended structure:

```text
Simple search-related illustration or icon

No matching games found

Try changing your search or removing one or more filters.

[ Clear all filters ]
```

Alternative concise heading:

> No results found

The supporting message should explain the next useful action.

The primary recovery action should be one of:

- clear all filters;
- reset filters;
- clear search;
- return to all games.

For the first Comparable Games case, the preferred action is:

> Clear all filters

The action should use the primary color and remain visually clear without dominating the page.

### No Results Visual Direction

The no-results state should follow the lighter visual references:

- centered composition;
- generous whitespace;
- simple line-based illustration;
- low visual noise;
- restrained use of primary blue, blue-green, and soft violet;
- heading, supporting text, and one clear recovery action.

The illustration may use a search, empty document, or empty-container metaphor.

It should not appear humorous or alarming enough to undermine the reliability of the research product.

### Mobile No Results

On mobile, the state should be vertically centered within the available result region when practical.

Structure:

```text
Illustration
Heading
Supporting message
Primary recovery action
```

The illustration should remain compact enough that the action is visible without excessive scrolling.

### Desktop No Results

On desktop, the same centered state may occupy the main result region while search and filters remain accessible.

The larger viewport may allow a slightly more detailed illustration, but the state should remain visually restrained.

## No Data Available State

The no-data state is different from no results.

It appears when the application does not currently have game records available for the query experience.

Recommended structure:

```text
Data-related illustration or icon

Game data is not available yet

We are preparing the first comparable-games dataset.

[ Try again ]
```

This state should not instruct users to clear filters when filters are not the cause.

The message may be refined later according to the actual collection and data-import process.

## Error State

A request error occurs when the query cannot be completed because of a network, API, or unexpected application failure.

Recommended structure:

```text
Calm error icon

We couldn't load comparable games

Please try again. Your search and filters have been preserved.

[ Try again ]
```

The current search and selected filters should remain intact whenever possible.

Error messaging should avoid technical details in the primary interface.

Diagnostic information may be logged separately.

## Route Not Found Page

The application must provide a dedicated not-found page for invalid or unavailable routes.

This page is different from the Comparable Games no-results state.

The route-not-found page should:

- use the established GameMarketIntel palette;
- maintain the product identity;
- clearly communicate that the requested page does not exist;
- provide a direct path back to a valid application destination;
- remain simple and lightweight.

Recommended content:

```text
404

Page not found

The page you are looking for may have moved or does not exist.

[ Return to overview ]
```

Alternative supporting text:

> We couldn't find the page you requested.

The preferred primary action is:

> Return to overview

### Route Not Found Visual Direction

The page may take structural inspiration from the provided 404 reference:

- a large centered content surface;
- an illustration occupying one side or the upper area;
- concise text;
- one clear recovery button;
- subtle decorative shapes behind the main surface;
- strong contrast between the page background and primary content container.

The GameMarketIntel version should use the existing light palette rather than adopting the dark blue palette from the reference directly.

Possible composition:

```text
Application background

Decorative accent shapes

Large surface
├── 404 and message
├── supporting text
├── Return to overview action
└── restrained research or game-related illustration
```

On mobile, the composition should stack vertically:

```text
Illustration
404
Page not found
Supporting message
Return to overview
```

On desktop, text and illustration may be positioned side by side.

The not-found page should not require the main application sidebar to remain open.

A lightweight header with product identity may be retained so the page still feels part of the application.

## Empty-State Accessibility

All empty, loading, error, and not-found states must:

- provide real text rather than embedding essential messages inside illustrations;
- preserve sufficient color contrast;
- avoid relying on color alone;
- expose meaningful accessible names for icons and actions;
- maintain keyboard-accessible recovery actions;
- announce important dynamic state changes when appropriate;
- respect reduced-motion preferences.

Decorative illustrations should be hidden from assistive technologies when they do not add semantic meaning.

Loading messages should be exposed through an appropriate live region when the implementation changes state dynamically.


## Navigation Design Decision

The initial navigation decision is:

> Use a persistent and collapsible lateral sidebar on desktop, and a temporary lateral drawer opened from the header on mobile.

This pattern was selected because it:

- preserves screen space on smaller devices;
- supports a growing number of product modules;
- maintains consistent navigation across viewport sizes;
- keeps the main research content visible when navigation is not required;
- supports mouse, keyboard, touch, and assistive-technology interaction;
- can evolve without redesigning the application shell.