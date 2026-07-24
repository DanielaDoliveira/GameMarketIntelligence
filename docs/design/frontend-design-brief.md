# Comparable Games Frontend Design Brief

## Status

**Approved for Milestone 2 implementation**

Backend search foundation: **Implemented**

First frontend delivery: **Implemented and validated with the current empty dataset**

Milestone 2: **In Progress**

## Purpose

Define the visual, responsive, interaction, accessibility, and feedback-state direction for Milestone 2 — Comparable Games Read Experience.

This document describes the first frontend increment only.

Future Research Hub capabilities, commercial evidence, market metrics, analytical dashboards, and advanced decision-support experiences remain outside its scope.

The broader product direction is documented in:

```text
docs/product/product-vision.md
```

## Maintenance Note

This document is temporarily maintained as a single file.

Its content is grouped by responsibility so that each major section can later be extracted into a smaller document without redesigning the information architecture.

Commercial metrics are intentionally deferred to the future Market Metrics Foundation milestone.

Their future inclusion must not alter the contracts, scope, or Definition of Done of the first Comparable Games frontend delivery.

## Backend Compatibility Note

The implemented Comparable Games API currently supports:

* partial game-name search;
* one optional genre identifier;
* one optional platform identifier;
* one optional release year;
* pagination.

Different supplied categories use AND semantics.

The current API does not yet accept multiple genre or platform identifiers in the same request.

Therefore, the first frontend implementation must use:

* one optional selected genre;
* one optional selected platform.

Multiple genre and platform selection remains a planned product evolution.

It requires an API contract extension before implementation.

## First Frontend Delivery Status

The first responsive frontend delivery has been implemented without changing the broader Milestone 2 goal.

Implemented:

* responsive Blazor WebAssembly application shell;
* expanded and compact desktop navigation;
* temporary mobile navigation drawer with overlay and Escape-key support;
* contextual Comparable Games search in the header;
* one optional genre filter;
* one optional platform filter;
* optional release-year filter;
* removable active-filter chips and clear-all behavior;
* pagination controls;
* search, filter, and page state stored in the URL;
* synchronization with browser back and forward navigation;
* branded initial loading and query-loading states;
* no-data, no-results, request-error, and route-not-found states;
* reusable button, feedback, loading, card, fallback-image, filter, and pagination components;
* Blazor CSS isolation;
* separation of Razor markup, C# code-behind, and isolated CSS where it improves maintainability.

Validated:

* local build;
* automated solution tests;
* desktop and mobile browser behavior supported by the current dataset;
* deployed frontend-to-API communication.

Data-dependent validation remains pending for populated genre and platform options, populated result cards, and multi-page pagination because the production database is currently empty.

The basic game-details frontend destination remains pending.

Multiple genre and platform selection remains part of the intended future product experience. The current single-selection controls are an incremental implementation aligned with the existing API contract, not a replacement for that future goal.

## Document Map

1. **Application Structure and Navigation** — shell, header, sidebar, drawer, search placement, footer, and navigation decisions.
2. **Visual System and Styling** — palette, tokens, CSS strategy, responsive behavior, and motion.
3. **Comparable Games Query and Results** — filters, combination rules, result layouts, and game-card content.
4. **Feedback States and Accessibility** — loading, empty, validation, error, not-found, accessibility, and reduced-motion behavior.

## Table of Contents

* [1. Application Structure and Navigation](#1-application-structure-and-navigation)
* [2. Visual System and Styling](#2-visual-system-and-styling)
* [3. Comparable Games Query and Results](#3-comparable-games-query-and-results)
* [4. Feedback States and Accessibility](#4-feedback-states-and-accessibility)
* [Future Evolution](#future-evolution)
* [Future Document Split](#future-document-split)

# 1. Application Structure and Navigation

## Navigation Pattern

GameMarketIntel will use an adaptive lateral-navigation pattern inspired by modern productivity applications.

The navigation structure remains consistent across devices, while its presentation changes according to available viewport width.

### Desktop Behavior

On desktop layouts:

* lateral navigation is visible by default;
* the application header contains a menu button that can collapse or expand the sidebar;
* the sidebar occupies its own layout space and does not cover page content;
* the content area resizes when the sidebar changes state;
* navigation labels and icons are visible in the expanded state;
* the compact state displays icons while preserving product identity.

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

The user should be able to collapse it when additional horizontal space is required for research or comparison content.

### Mobile Behavior

On mobile layouts:

* the sidebar is hidden by default;
* the application header contains a visible menu button;
* the menu button opens a temporary lateral drawer;
* the drawer appears above the page content;
* the remaining page receives a visual overlay while the drawer is open;
* content remains the primary focus when navigation is not required.

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

* the user selects a destination;
* the user activates the close button;
* the user clicks or taps outside the drawer;
* the user presses Escape when a keyboard is available.

### Gesture Support

Swipe gestures may be added later as progressive enhancement.

They must not be the only way to open or close navigation because gesture-only navigation may:

* be difficult to discover;
* conflict with browser or operating-system gestures;
* reduce keyboard and mouse accessibility;
* behave inconsistently across devices.

The visible menu button remains the primary navigation control.

### Responsive Navigation Principle

Navigation should follow this progression:

```text
Mobile
→ hidden drawer opened by the header menu button

Tablet or narrow desktop
→ hidden or compact sidebar

Wide desktop
→ expanded sidebar visible by default
```

The application must preserve the same navigation hierarchy and routes across viewport sizes.

Only presentation and interaction should adapt.

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

The shell must remain visually consistent across:

* result pages;
* game-detail pages;
* loading states;
* no-results states;
* no-data states;
* validation states;
* request-error states;
* route-not-found pages.

## Header Pattern

The header should remain visible, lightweight, and visually consistent across pages.

Its responsibilities are:

* expose the navigation menu control;
* preserve product identity;
* provide access to the current search experience;
* reserve limited space for future secondary actions.

The header must not duplicate navigation options already available in the sidebar.

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

The search field should occupy the central region and receive most available horizontal space.

The product identity should remain visible on the left without competing with search.

The right region should remain minimal during the first increment.

It may later support:

* user preferences;
* language selection;
* help;
* profile;
* notifications.

No placeholder avatar or unnecessary action should be introduced before a real need exists.

### Mobile Header

On mobile, the header should use a compact search-centered composition.

Recommended structure:

```text
[ Menu ]   [ Search comparable games... ]   [ Compact action ]
```

The search field should remain the dominant element.

The full product name does not need to remain visible when horizontal space is limited.

A compact product mark may appear when it does not reduce search usability.

The mobile header should preserve:

* a visible menu button;
* a comfortable search target;
* sufficient spacing between controls;
* clear focus and active states;
* support for touch and keyboard interaction.

## Search Placement

For the first frontend increment, Comparable Games search should appear in the center of the application header.

The Comparable Games page must not repeat the same primary search field inside the main content.

The main content should contain:

* page title;
* genre filter;
* platform filter;
* optional release-year filter;
* active-filter presentation;
* result count;
* results or feedback states.

Recommended structure:

```text
Header
└── Search comparable games

Comparable Games page
├── Page title
├── Genre, platform, and optional year filters
├── Active-filter context
├── Result count
└── Results
```

This prevents duplicate search controls and gives the application a clear visual focus.

## Search Scope

During the first increment, header search is contextual.

On the Comparable Games page, it searches games by partial name.

It is not yet a global application search.

Future versions may expand the same search position to include:

* games;
* genres;
* platforms;
* data sources;
* market indicators.

Any future expansion to global search must clearly communicate the type and scope of returned results.

## Footer Pattern

The application should include a lightweight footer below the main content.

The footer may contain:

* product name;
* About link;
* Data Sources link;
* repository or project link;
* data-update information when available.

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

* gives search a prominent and stable location;
* avoids duplicating search controls;
* separates navigation from search;
* supports desktop and mobile layouts;
* preserves space for future analytical content;
* maintains consistent visual structure across pages.

## Navigation Design Decision

The initial navigation decision is:

> Use a persistent and collapsible lateral sidebar on desktop, and a temporary lateral drawer opened from the header on mobile.

This pattern was selected because it:

* preserves screen space on smaller devices;
* supports a growing number of product modules;
* maintains consistent navigation across viewport sizes;
* keeps main research content visible when navigation is not required;
* supports mouse, keyboard, touch, and assistive technology;
* can evolve without redesigning the application shell.

# 2. Visual System and Styling

## Initial Color Direction

The first visual direction will use a light interface.

The color system should communicate:

* trust;
* calm;
* clarity;
* lightness;
* practicality;
* fluidity.

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

A subtle distinction between page background, cards, navigation, and supporting surfaces should organize content without creating visual weight.

### Primary Color

The primary color should come from a stable blue family.

It may be used for:

* active navigation;
* primary buttons;
* links;
* focus indicators;
* selected controls;
* progress feedback.

The blue should feel trustworthy and clear without resembling a financial trading interface.

### Secondary Color

A restrained blue-green or cyan family may support:

* movement;
* progress;
* freshness;
* fluidity.

It should support secondary emphasis rather than compete with the primary blue.

### Accent Color

A soft violet or lilac may be used selectively for product identity.

Appropriate uses include:

* subtle gradients;
* decorative details;
* selected badges;
* loading illustrations;
* secondary highlights.

The accent must remain restrained.

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

Pure black should not be required.

A dark navy tone may preserve readability while aligning with product identity.

### Semantic Colors

The design system should define distinct colors for:

* success;
* information;
* warning;
* error.

Semantic meaning must never depend on color alone.

Icons, labels, and explanatory text should support each state.

## Initial Palette

| Design token     |       Hex | Primary use                           |
| ---------------- | --------: | ------------------------------------- |
| `background`     | `#F5F7FB` | Application background                |
| `surface`        | `#FFFFFF` | Cards, header, and navigation         |
| `surface-muted`  | `#EEF2F8` | Secondary surfaces and skeletons      |
| `border`         | `#DDE4EE` | Subtle separators and borders         |
| `text-primary`   | `#172033` | Main text                             |
| `text-secondary` | `#5E6B80` | Supporting text and metadata          |
| `text-muted`     | `#8995A8` | Low-emphasis information              |
| `primary`        | `#315E9E` | Primary actions and active navigation |
| `primary-hover`  | `#274C82` | Hover and pressed states              |
| `primary-soft`   | `#E7EFFA` | Selected surfaces and highlights      |
| `secondary`      | `#3F8F9D` | Progress and secondary emphasis       |
| `secondary-soft` | `#E2F2F3` | Soft secondary surfaces               |
| `accent`         | `#7A6FC2` | Product identity accents              |
| `accent-soft`    | `#EFECFA` | Soft accent surfaces                  |
| `success`        | `#2E7D5B` | Success states                        |
| `warning`        | `#B7791F` | Warning states                        |
| `error`          | `#B54747` | Error states                          |
| `info`           | `#3975AD` | Information and loading states        |

These values are an initial design direction and must be validated for accessibility in actual component combinations.

The implementation should reference semantic tokens instead of arbitrary colors.

## Dark Theme

A dark theme is not part of the first frontend delivery.

The initial system should avoid decisions that make future dark-theme support unnecessarily difficult.

Colors must be represented through reusable semantic tokens rather than scattered literal values.

## Initial Design Tokens

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

Components must use semantic tokens instead of repeating hexadecimal values.

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

## Styling Strategy

The initial frontend will use standard CSS and Blazor CSS isolation.

Tailwind CSS will not be introduced in the first frontend implementation.

This decision prioritizes:

* native integration with the Blazor build process;
* simpler local development;
* fewer external build dependencies;
* predictable CI and deployment;
* direct control over responsive layouts;
* clear separation between global tokens and component styles.

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

CSS Grid and Flexbox should provide primary layout mechanisms.

Media queries should progressively adapt the mobile-first layout.

### Global CSS Responsibilities

Global CSS should contain styles intentionally shared across the application:

* semantic design tokens;
* font and text defaults;
* box-sizing and base resets;
* page background;
* default link behavior;
* shared focus indicators;
* reduced-motion behavior;
* reusable layout helpers where valuable.

Global CSS should avoid component-specific selectors unless a style is intentionally shared.

### CSS Isolation Responsibilities

Blazor CSS isolation should be used when styles belong to a specific component or layout.

Examples:

```text
MainLayout.razor.css
→ shell, header, sidebar, and drawer

GameCard.razor.css
→ game-card structure and responsive behavior

LoadingSkeleton.razor.css
→ skeleton appearance and animation

EmptyState.razor.css
→ empty-state presentation

ErrorState.razor.css
→ error-state presentation and retry action
```

### Responsive Strategy

The frontend must remain mobile-first.

Base styles should represent the mobile layout.

Larger layouts should be introduced through progressive media queries.

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

* sidebar expansion and collapse;
* mobile drawer entrance and exit;
* overlay appearance;
* loading skeletons;
* focus and active states.

The interface should respect reduced-motion preferences.

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

## Styling Decision

> Use standard CSS, semantic CSS custom properties, CSS Grid, Flexbox, media queries, and Blazor CSS isolation.

# 3. Comparable Games Query and Results

## Comparable Games Query Experience

The first functional frontend case will allow users to search and filter comparable games.

The first implementation must support:

* partial game-name search;
* one optional genre;
* one optional platform;
* optional release year when exposed by the first UI;
* removal of individual active filters;
* clearing all active filters;
* result-count feedback;
* pagination;
* loading, no-results, validation, error, and unavailable-data states.

## Current Query Combination Rules

Different filter categories use AND semantics.

Example:

```text
Name contains "Hades"
AND
Genre matches Action
AND
Platform matches PC
AND
Release year matches 2020
```

The first API contract accepts only one selected genre and one selected platform.

The frontend must not present multi-select behavior before the API supports it.

## Future Multi-Select Behavior

A future API and frontend version may support multiple values inside the same category using OR semantics.

Example:

```text
Name contains "Hades"
AND
Genre matches Action OR Roguelike
AND
Platform matches PC OR Nintendo Switch
```

Possible future genre control:

```text
Genre matching
(•) Any selected genre
( ) All selected genres
```

This control is not part of the first frontend delivery.

## Search and Filter Layout

The primary Comparable Games search field must appear in the center of the application header.

The page body must not repeat the same name-search field.

Initial page structure:

```text
Header
└── [ Search comparable games... ]

Comparable Games

[ Genre ] [ Platform ] [ Release year ]

Active filters
[Action ×] [PC ×] [2020 ×] [Clear all]

12 games found

Results
Pagination
```

### Mobile

On mobile:

* the search field should occupy the dominant header area;
* filters may open in a temporary panel, drawer, or bottom sheet;
* active filters should remain visible;
* result count should appear before results;
* controls must remain comfortable for touch interaction;
* pagination must remain understandable on narrow screens.

### Desktop

On desktop:

* filters may appear in a horizontal toolbar or supporting side panel;
* active filters and result count should remain visually close to results;
* the layout may use additional horizontal space without becoming dense;
* pagination should appear after the result region.

The exact filter-panel presentation will be validated in wireframes.

## Search Behavior

The first implementation uses explicit form submission from the header search.

This avoids sending an API request after every individual keystroke and gives the user control over when a query is applied.

Current behavior:

```text
User types
    ↓
User submits the search
    ↓
Page returns to page 1
    ↓
Latest search and existing filters are written to the URL
    ↓
The Comparable Games request is executed
```

A debounced search interaction may be evaluated later if product usage demonstrates that it improves the research workflow.

The current search and filters must remain preserved during loading and recoverable error states.

## Pagination Behavior

The API returns:

* current page;
* page size;
* total items;
* total pages.

The frontend should:

* return to page `1` when a filter changes;
* disable previous-page navigation on page `1`;
* disable next-page navigation on the final page;
* preserve active search and filters when changing pages;
* avoid showing impossible page numbers;
* present the current page and total pages clearly.

Default behavior:

```text
Page:
1

Page size:
20
```

The first UI does not need to expose a page-size selector unless it provides clear value.

## Results Layout

The result presentation must adapt to available width.

### Mobile Result Pattern

Mobile should use a vertically stacked result list.

Each result should contain:

```text
Cover image
Game name
Release year or date
Genres
Platforms
Details affordance
```

Recommended pattern:

```text
┌────────────────────────────┐
│ [Cover]  Game name         │
│          Release year      │
│          Genres            │
│          Platforms      ›  │
└────────────────────────────┘
```

One result should appear per row.

### Desktop Result Pattern

Desktop may use a responsive card grid.

Recommended progression:

```text
Narrow
→ one card per row

Medium
→ two cards per row

Wide
→ three cards per row
```

Cards should remain readable and should not become excessively narrow.

## Game Card Content

Each card should initially expose:

* cover image or fallback;
* game name;
* release year or date;
* genres;
* platforms;
* details affordance.

Long descriptions should not appear in result cards.

Descriptions belong to the details destination.

## Images

Image URLs come from external data.

The frontend must support:

* missing image URLs;
* broken external images;
* delayed image loading;
* accessible alternative text;
* neutral fallback visuals.

Fallback imagery should use the established palette and should not resemble an error state.

## Genre and Platform Presentation

Genres and platforms should be visually secondary to the game name.

They may appear as:

* compact tags;
* restrained text lists;
* short metadata rows.

Cards must avoid excessive chip density.

When many categories are present, the UI may show a limited number followed by a summary such as:

```text
+2 more
```

## Basic Game Details Destination

The first details destination should expose:

* cover image or fallback;
* game name;
* release date;
* genres;
* platforms;
* short description when available.

It should not introduce:

* market metrics;
* sales data;
* analytical charts;
* recommendation scores;
* unsupported source claims.

A more complete details experience may be added later.

# 4. Feedback States and Accessibility

## Initial Blazor Loading State

Blazor WebAssembly may require noticeable time during the first load.

The interface must reassure users that loading is expected and progressing.

Recommended message:

> Preparing your market research workspace...

The loading experience should:

* appear immediately;
* use the GameMarketIntel visual identity;
* avoid a blank page;
* avoid indefinite-looking motion;
* remain lightweight;
* preserve user confidence during cold or first loads.

## Query Loading State

When the application is searching:

> Searching comparable games...

The query-loading state should:

* preserve the current page structure;
* keep search and filters visible;
* use skeletons or restrained progress feedback;
* avoid replacing the entire application shell;
* avoid content jumps where practical.

## Results Available State

When results are available, the page should show:

* current search and filters;
* active-filter context;
* total matching game count;
* result list or grid;
* pagination controls.

Example:

```text
12 games found
```

Pluralization should be handled correctly.

## No Results State

The no-results state appears when the request succeeds but no game matches the current criteria.

The page should preserve search and filter context.

Recommended structure:

```text
Simple search-related illustration or icon

No matching games found

Try changing your search or removing one or more filters.

[ Clear all filters ]
```

Alternative heading:

> No results found

The supporting message should explain the next useful action.

The preferred action is:

> Clear all filters

### Mobile No Results

On mobile, the state should be vertically stacked:

```text
Illustration
Heading
Supporting message
Primary recovery action
```

The illustration should remain compact enough that the action is visible without excessive scrolling.

### Desktop No Results

On desktop, the state should occupy the main result region while search and filters remain accessible.

It should use:

* centered composition;
* generous but controlled whitespace;
* restrained line-based illustration;
* clear heading;
* short supporting message;
* one recovery action.

## No Data Available State

The no-data state is different from no results.

It appears when the application does not currently contain game records.

Recommended structure:

```text
Data-related illustration or icon

Game data is not available yet

We are preparing the first comparable-games dataset.

[ Try again ]
```

This state must not instruct the user to clear filters when filters are not the cause.

## Validation State

Invalid query values return standardized HTTP `400` responses.

Current validation rules include:

* page must be at least `1`;
* page size must be between `1` and `100`;
* release year cannot be greater than the current year.

The frontend should not normally generate invalid values.

When a validation response is received, the interface should:

* preserve current user input;
* explain which input is invalid;
* avoid presenting the problem as an unexpected system failure;
* guide the user toward a valid correction.

Example:

```text
Some search options are invalid

Page size must be between 1 and 100.
```

The API uses the implemented global exception handler and `ValidationProblemDetails` response structure.

## Request Error State

A request error occurs when a query cannot be completed because of:

* network failure;
* API unavailability;
* unexpected server failure;
* deployment cold start exceeding the expected request duration.

Recommended structure:

```text
Calm error icon

We couldn't load comparable games

Please try again. Your search and filters have been preserved.

[ Try again ]
```

Error messaging should avoid technical details in the primary interface.

Diagnostic information may be logged separately.

## Cold Start and User Confidence

The API may experience cold starts under free hosting constraints.

The frontend should communicate that the request is still progressing rather than appearing frozen.

Recommended behavior:

```text
Initial request
    ↓
Normal loading feedback
    ↓
If loading continues longer than expected
    ↓
Reassuring secondary message
```

Possible secondary message:

> The research service is starting. This may take a little longer on the first request.

The interface must not:

* display an indefinite blank region;
* imply that the user caused the delay;
* repeatedly retry without control;
* erase the current search context.

## Route Not Found Page

The application must provide a dedicated not-found page for invalid routes.

This differs from the Comparable Games no-results state.

Recommended content:

```text
404

Page not found

The page you are looking for may have moved or does not exist.

[ Return to overview ]
```

The page should:

* use the established palette;
* maintain product identity;
* clearly communicate that the route does not exist;
* provide a direct path back to a valid destination;
* remain simple and lightweight.

## Accessibility Requirements

The first frontend increment must include:

* semantic HTML;
* meaningful heading hierarchy;
* visible keyboard focus;
* keyboard-accessible navigation;
* Escape support for the mobile drawer;
* labels for search and filter controls;
* accessible names for icon-only buttons;
* sufficient color contrast;
* status announcements where appropriate;
* alternative text for meaningful images;
* decorative images hidden from assistive technology;
* reduced-motion support;
* touch-friendly target sizes.

## Live Feedback

Dynamic feedback such as loading, result counts, validation, and errors should be announced appropriately.

Possible use:

```html
<div aria-live="polite">
    12 games found
</div>
```

Urgent errors may use a stronger announcement when necessary, but routine updates should not interrupt the user excessively.

## Keyboard Interaction

Keyboard users must be able to:

* open and close navigation;
* move through navigation links;
* focus the search field;
* operate filter controls;
* remove active filters;
* activate clear-all;
* navigate pagination;
* open a game result;
* activate retry actions.

Focus must return to a logical location after dialogs, drawers, or temporary panels close.

## Reduced Motion

The application should respect:

```css
@media (prefers-reduced-motion: reduce)
```

Motion should not be required to understand state changes.

## Error Tone

Errors should sound:

* calm;
* direct;
* actionable;
* non-technical.

They should not blame the user or expose internal stack traces.

# Future Evolution

After the first frontend and API contract are validated, the query experience may evolve to include:

* multiple genre selection;
* multiple platform selection;
* OR semantics inside the same category;
* optional all-selected-genres matching;
* release-period filters;
* publisher filters;
* developer filters;
* sorting;
* URL state extended for future multi-select and advanced filter contracts;
* saved searches;
* richer details pages;
* source and reliability context;
* research links;
* market metrics;
* analytical comparisons.

These capabilities must not be introduced before their API contracts and product value are defined.

# Future Document Split

When the frontend documentation grows, this file may be divided into:

```text
docs/frontend/
├── application-shell.md
├── visual-system.md
├── comparable-games-query.md
├── comparable-games-results.md
├── feedback-states.md
└── accessibility.md
```

The current section structure is designed to support that future split.
