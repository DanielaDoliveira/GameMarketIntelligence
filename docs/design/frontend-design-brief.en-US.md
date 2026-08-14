# Comparable Games Frontend Design Brief

> Status: first responsive delivery implemented; reviewed after IGDB PoC
> approval on August 14, 2026; representative-data validation pending.

## Purpose

Define the current responsive, interaction, accessibility, and feedback-state direction for Comparable Games without repeating the broader product vision.

In the first IGDB MVP, the filtering experience is the primary product-value
surface. Its controls and explanations must reflect the PoC-approved field
semantics, qualify source-dependent results, and communicate missing data
without overwhelming the producer.

## Implemented experience

- responsive Blazor WebAssembly shell;
- desktop sidebar that starts expanded and can be collapsed;
- mobile navigation that starts closed and opens as a drawer;
- visible product identity on desktop and mobile;
- explicit search and filter form with a visible `Search` button;
- unified submission of name, genre, platform, and release year;
- controls disabled while a query is loading;
- URL-preserved applied state and browser history synchronization;
- active-filter removal, clear-all, pagination, and result count;
- loading, no-data, no-results, validation, request-error, and not-found states;
- reusable components, code-behind separation, CSS isolation, and responsive validation.

The production dataset is still empty, so populated filters, cards, and multi-page pagination require final validation after ingestion.

## Search behavior

The current API supports:

- one optional partial name;
- one optional genre;
- one optional platform;
- one optional release year;
- pagination.

Different supplied categories use AND semantics. Multiple values per category remain future scope.

The search control belongs to the Comparable Games form, not the global header. Enter and the visible button submit the same complete form state.

## Visual system

- Open Sans for body text;
- Space Grotesk for headings;
- semantic CSS custom properties;
- standard CSS, Grid, Flexbox, and Blazor CSS isolation;
- mobile-first breakpoints based on content;
- subtle motion with `prefers-reduced-motion` support;
- no dark theme in the current milestone.

## Result cards

Cards show only concise discovery information:

- optional cover when available and suitable for the selected display size;
- name;
- release date/year;
- genres;
- platforms;
- details action.

Long descriptions and analytical metrics do not belong in cards.

The card layout is mobile-first and must remain visually complete without an
image. Missing or unusable image data does not require a permanent placeholder:
the textual content may use the available space naturally. If a neutral
missing-image indicator is later adopted for accessibility or consistency, it
must not dominate the card or imply that the record itself is incomplete.

Images are reduced to fit predefined containers without being enlarged beyond
an appropriate source rendition. Aspect ratio is preserved through fitting or
cropping rules defined by the UI; source records are never stretched. Covers
support recognition and visual breathing room but are not a filter or identity
signal. Screenshots remain in details and on-demand galleries, with visible
source attribution and the documented rights caveats.

## Feedback and accessibility

- loading keeps context visible;
- no-results preserves the applied criteria and offers clear recovery;
- no-data explains that the dataset is not available yet;
- validation preserves user input and explains the invalid field;
- errors avoid exposing internal details;
- keyboard navigation, focus indicators, semantic labels, `aria-live`, and reduced motion are required.

## Deferred

The following items are deferred from the implemented frontend. Some belong to
the remainder of the first IGDB MVP, while others remain later-iteration
product scope and must be prioritized against the approved data decisions:

- multiple genre/platform values;
- approved source-qualified filters not yet implemented, such as modes and
  themes;
- complete details page;
- reliability filter;
- source presentation in results;
- contextual keyword navigation;
- market metrics, charts, recommendations, authentication, and dark theme;
- perspective filtering and manual multi-keyword filtering, which remain
  future iterations unless later source evidence changes their coverage.
