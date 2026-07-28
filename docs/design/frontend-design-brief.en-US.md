# Comparable Games Frontend Design Brief

> Status: first responsive delivery implemented; representative-data validation pending.

## Purpose

Define the current responsive, interaction, accessibility, and feedback-state direction for Comparable Games without repeating the broader product vision.

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

- image or fallback;
- name;
- release date/year;
- genres;
- platforms;
- details action.

Long descriptions and analytical metrics do not belong in cards.

## Feedback and accessibility

- loading keeps context visible;
- no-results preserves the applied criteria and offers clear recovery;
- no-data explains that the dataset is not available yet;
- validation preserves user input and explains the invalid field;
- errors avoid exposing internal details;
- keyboard navigation, focus indicators, semantic labels, `aria-live`, and reduced motion are required.

## Deferred

- multiple genre/platform values;
- advanced filters;
- complete details page;
- reliability filter;
- source presentation in results;
- market metrics, charts, recommendations, authentication, and dark theme.
