# Presentation Style Guide

**Project:** Research Infrastructure Costing & Pricing Tool (CITS5206 Capstone)
**Status:** Binding for all presentations produced in this repository
**Version:** 2.3 — 14 August 2026 (change log: §11)

Every HTML presentation in `presentations/` **must** conform to this document. The goal is that every deck in this repository — whoever builds it, whenever over the semester — looks like it came from one studio.

Requirement language follows RFC 2119: **MUST**, **MUST NOT**, **SHOULD**, **MAY**.

---

## 1. Design Intent

The visual language is **editorial, not corporate**. Its reference points are the Swiss/International Typographic Style and modern newspaper design: a strict grid, generous whitespace, one high-contrast serif for voice, one neutral sans for information, and a single saturated red used as punctuation rather than decoration.

Three principles govern every decision below:

1. **Red is a verb, not a wallpaper.** Red marks the one thing on the page that matters. If everything is red, nothing is.
2. **Black is a surface, not an outline.** Deep near-black is used as a full-bleed field for cover and section pages, never as borders and boxes around light content.
3. **Whitespace is the design.** A slide with four words and a lot of air reads as confident. A slide with sixteen bullets reads as unprepared.

---

## 2. Colour

### 2.1 Tokens

All colours **MUST** be referenced through these CSS custom properties. Hard-coded hex values in a deck are a defect.

```css
:root {
  /* Ink — the black family. Never pure #000. */
  --ink-900: #0B0B0C;   /* full-bleed dark surface: cover, section dividers */
  --ink-800: #141416;   /* dark surface, elevated */
  --ink-700: #1C1C1F;   /* dark cards, table header on dark */
  --ink-500: #3A3A40;   /* hairlines and dividers on dark */
  --ink-300: #6E6E75;   /* muted text on dark (captions only) */

  /* Paper — the light family. Never pure #FFF. */
  --paper:     #FAF8F5; /* default slide background — warm off-white */
  --paper-alt: #F1EDE7; /* panels, table zebra stripe, code blocks */
  --paper-rule:#DDD6CC; /* hairlines and dividers on light */

  /* Red — the single accent. */
  --red-700: #9B0F22;   /* pressed / deep emphasis / red on red-tint */
  --red-600: #C8102E;   /* PRIMARY ACCENT — the project red */
  --red-400: #E8384F;   /* red for text on dark surfaces only */
  --red-100: #F6E2E4;   /* red tint fill — callout backgrounds */

  /* Stone — warm neutrals for secondary information. */
  --stone-600:#5C554D;  /* secondary body text on paper */
  --stone-400:#8A8078;  /* captions, axis labels, disabled */
  --stone-200:#C9C1B8;  /* chart series, subtle fills */

  /* Semantic — the costing domain needs surplus/deficit. */
  --surplus: #1F6F5C;   /* deep jade — positive balance */
  --deficit: #C8102E;   /* = --red-600 — negative balance */
}
```

### 2.2 Contrast — verified, not assumed

Measured with the WCAG 2.1 relative-luminance formula:

| Foreground | Background | Ratio | Verdict |
|---|---|---|---|
| `--ink-900` | `--paper` | **18.6 : 1** | AAA — default body pairing |
| `--ink-900` | `--paper-alt` | **16.9 : 1** | AAA — text on panels |
| `--stone-600` | `--paper` | **6.9 : 1** | AA ✓ — secondary text, captions, furniture |
| `--red-600` | `--paper` | **5.6 : 1** | AA ✓ — red text on light |
| `--paper` | `--red-600` | **5.6 : 1** | AA ✓ — knockout text on red |
| `--red-700` | `--red-100` | **6.8 : 1** | AA ✓ — text inside a red callout |
| `--surplus` | `--paper` | **5.7 : 1** | AA ✓ |
| `--paper` | `--ink-900` | **18.6 : 1** | AAA — dark-slide body |
| `--red-400` | `--ink-900` | **4.8 : 1** | AA ✓ — red text on dark |
| `--stone-400` | `--ink-900` | **5.1 : 1** | AA ✓ — furniture on dark |
| `--red-600` | `--ink-900` | **3.3 : 1** | ✗ body — large bold ≥ 28 px only |
| `--stone-400` | `--paper` | **3.6 : 1** | ✗ text — chart axes and decoration only |

**Two rules follow directly from the table:**

1. On a dark surface, red text **MUST** use `--red-400`. `--red-600` on `--ink-900` is permitted only for display type ≥ 28 px bold, and for rules, bars and fills.
2. `--stone-400` **MUST NOT** be used for text on Paper at any size — use `--stone-600`. It is reserved for chart axes, gridlines and furniture on Ink.

These slides hold themselves to **WCAG 2.1 AA**. A deck projected badly in a bright room is the normal case, not the edge case.

### 2.3 Usage rules

- Red **MUST NOT** exceed roughly **10% of a slide's visual area**.
- Each slide **SHOULD** have exactly **one red focal point** — a key figure, one highlighted row, one underlined phrase. Not three.
- A slide **MUST** be either a Paper slide or an Ink slide. Gradients between the two families are prohibited.
- Ink slides are **reserved** for: the cover, the section dividers — one per member (see [`docs/project/team.md`](../docs/project/team.md)) — and the closing slide. This gives the deck a rhythm — the audience learns that a black page means "new speaker".
- `--surplus` / `--deficit` **MUST** be used for balance figures, never a generic red/green.
- Colour **MUST NOT** be the sole carrier of meaning. A deficit is red *and* parenthesised *and* labelled.

### 2.4 Chart palette

Charts **MUST** draw series in this order, no substitutions:

| Order | Token | Hex |
|---|---|---|
| 1 | `--red-600` | `#C8102E` |
| 2 | `--ink-700` | `#1C1C1F` |
| 3 | `--stone-400` | `#8A8078` |
| 4 | `--red-100` (with `--red-700` 1 px stroke) | `#F6E2E4` |
| 5 | `--stone-200` | `#C9C1B8` |

Axes and gridlines: `--paper-rule` at 1 px. No 3D, no drop shadows, no chart-junk gradients. Label series directly on the plot in preference to a legend.

---

## 3. Typography

### 3.1 Families

Three faces. No fourth face may be introduced.

| Role | Family | Weights | Used for |
|---|---|---|---|
| **Display serif** | **Playfair Display** | 400, 500, 700 + 400 italic | Cover title, section titles, slide titles, big metrics, pull-quotes |
| **Text sans** | **Inter** | 400, 500, 600 | All running text, bullets, labels, tables, captions, navigation |
| **Mono** | **IBM Plex Mono** | 400, 500 | Formulas, code, story IDs (`US-17`), file paths, config keys |

**Why this pairing.** Playfair Display is a high-contrast transitional serif in the Baskerville/Didone lineage — the same tradition that makes a masthead or a book title page read as considered. Its thick/thin modulation only reads as elegant at size, so it is confined to display use. Inter is a neutral, large-x-height grotesque drawn for screens; it disappears, which is exactly what running text should do. Setting a warm, voiced serif against a cool, silent sans is the classic editorial split, and it gives every slide a clear two-level hierarchy before a single colour is applied. IBM Plex Mono joins them because this project is fundamentally about formulas and identifiers, and those must never be set in a proportional face.

### 3.2 Loading

Fonts **MUST** be loaded from the Google Fonts CDN with a system fallback stack, so a deck still presents if the room has no network.

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,500;0,700;1,400&family=Inter:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500&display=swap" rel="stylesheet">
```

```css
--font-display: "Playfair Display", "Iowan Old Style", "Palatino Linotype", Palatino, Georgia, "Times New Roman", serif;
--font-sans:    "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
--font-mono:    "IBM Plex Mono", ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
```

Presenting off a laptop with no internet is a foreseeable failure. **Test the deck once with the network off before any client meeting.**

### 3.3 Scale

The slide canvas is a fixed **1280 × 720** box scaled to the viewport (§4.1), so these are absolute pixel values and behave identically on every screen.

| Token | Size / line-height | Face & weight | Use |
|---|---|---|---|
| `--t-cover` | 72 / 1.05 | Display 700 | Cover title only |
| `--t-section` | 56 / 1.08 | Display 500 | Section divider title |
| `--t-metric` | 84 / 1.00 | Display 700, `tnum` | One hero figure per slide |
| `--t-title` | 44 / 1.15 | Display 500 | Slide title (`h1`) |
| `--t-sub` | 28 / 1.25 | Sans 600 | Slide subtitle, column headings (`h2`) |
| `--t-lead` | 24 / 1.40 | Sans 400 | Opening/summary paragraph |
| `--t-body` | 20 / 1.45 | Sans 400 | Default body, bullets, table cells |
| `--t-small` | 16 / 1.40 | Sans 400 | Captions, source lines, footnotes |
| `--t-label` | 13 / 1.20 | Sans 600, `0.14em` tracking, uppercase | Eyebrows, kickers, nav, page numbers |

**Minimum size for content is 20 px; nothing content-bearing falls below 16 px.** The 13 px `--t-label` is the single documented exception and is reserved for eyebrows, page furniture and table headers — navigational chrome the audience never has to read from the back row. If content will not fit at 20 px, the content is wrong: split the slide.

### 3.4 Setting rules

- Measure **MUST NOT** exceed **62 characters** (`max-width: 62ch`). Full-canvas-width paragraphs are prohibited.
- Body text is **left-aligned, ragged right**. Justified text and centred paragraphs are prohibited. Centring is permitted only for cover, section dividers and single-line pull-quotes.
- Headings **MUST NOT** be all-caps. Only `--t-label` eyebrows are uppercased.
- Numerals in tables and metrics **MUST** use tabular figures: `font-variant-numeric: tabular-nums;`
- Apply `-0.02em` letter-spacing to display type ≥ 44 px (Playfair sets loose at size); leave Inter at its default tracking.
- Emphasis is achieved with **weight 600** or `--red-600`, never with <u>underline</u>, and never with both at once.
- Currency is written `$150,000` — never `150000`, never `150k` in a client-facing deck.
- Australian English spelling throughout: *utilisation*, *organisation*, *centre*. Dates are set long-form and day-first — `3 August 2026`, never `08/03/26`, which reads as a different day on either side of the Pacific.
- Quotations from the client **MUST** be set in Playfair 400 italic at `--t-lead`, with a 3 px `--red-600` left rule, and **MUST** be attributed.

---

## 4. Layout & Structure

### 4.1 Canvas

- Aspect ratio **16:9**, canvas **1280 × 720 px**, scaled to fit the viewport by transform. Never reflow slide content responsively — a slide is a fixed composition.
- Safe margins: **64 px** left/right, **56 px** top/bottom. Nothing but full-bleed colour fields may enter the margin.
- Grid: **12 columns**, 24 px gutter, inside the safe area. Content **MUST** snap to it. Common splits: 12 (full), 6+6, 8+4, 4+4+4.
- Baseline spacing scale (px): **4, 8, 12, 16, 24, 32, 48, 64, 96**. No arbitrary values.

### 4.2 Deck structure — mandatory

Every deck **MUST** have exactly **one section per team member**, in the order listed in [`docs/project/team.md`](../docs/project/team.md).

**`docs/project/team.md` is the only place the roster lives.** Names and their order **MUST NOT** be copied into this guide, into `template.html`, or into any tooling. When the roster changes, one file changes.

Each section consists of:

- exactly **one section divider** page (Ink, full-bleed, numeral + title + owner), plus
- **2–3 content pages**.

A deck is therefore `cover → n × (divider + 2–3 pages) → closing`, where *n* is the number of members listed in `docs/project/team.md` — **3n + 2 to 4n + 2 pages**. Sections **MUST NOT** be added, removed or merged to suit one speaker; if a member has less to say, they take two pages, not zero.

Rationale: a one-section-per-member spine makes speaking time visibly equal, matches the assessment's per-member accountability, and lets any deck be rehearsed as self-contained blocks, one per speaker.

**When a member cannot attend.** The spine is fixed; who speaks to it on the day is not. If a member cannot attend, **that section's owner is reassigned to another member.** The team agrees the reassignment — on the spot before the deck is presented, or in the Teams chat ahead of time — and the divider's owner line **MUST** then name **the person actually presenting, and only that person**. A divider that announces someone who is not in the room is worse than no name at all, and a divider naming two people leaves the audience unsure who to look at.

Reassignment changes who owns a section for that delivery. It does not change the deck: a section is **never** deleted, merged into a neighbour or renumbered to cover an absence. *n* always equals the number of members listed in `docs/project/team.md`, so one person **MAY** carry two or three sections on the day.

Record the reassignment — a line in the deck's commit message, or in the minutes of the meeting where it was agreed — so the reason is on file rather than inferred from a name that changed.

### 4.3 Page types

| Type | Surface | Composition |
|---|---|---|
| **Cover** | Ink | Project title (Display 700), one-line descriptor, unit code + date, thin red rule |
| **Section divider** | Ink | Large red section numeral, section title, owner name, generated `SECTION n OF N` label |
| **Statement** | Paper | ≤ 12 words at `--t-title`, centred vertically, one red keyword. For turning points |
| **Standard** | Paper | Eyebrow → title → lead → content in 1–2 columns |
| **Metric** | Paper | One `--t-metric` figure with a label above and a one-line caption below |
| **Two-column** | Paper | 6+6 or 8+4 — text against a table, chart, diagram or screenshot |
| **Quote** | Paper | Client quotation, Playfair italic, red left rule, attribution |
| **Closing** | Ink | Thank-you / questions, repo URL, team names |

### 4.4 Page furniture

- Every content page **MUST** carry, in `--t-label`: a **section indicator** (top-left) and a **page number** (bottom-right).
- A **segmented progress bar** at the foot of the canvas — one segment per section — shows how far through the deck the audience is; filled segments are `--red-600`.
- **Page numbers and progress segments are generated by the deck engine, never typed.** A hand-typed page number is a defect: it goes stale the moment a page is inserted.
- Furniture is `--stone-600` on Paper and `--stone-400` on Ink — both clear AA at 13 px. It **MUST NOT** compete with content.
- Cover and closing pages carry no furniture.

### 4.5 Content density

Hard ceilings per content page:

- **6 bullets**, each **≤ 2 lines**.
- **1 chart**, or **1 table of ≤ 6 rows × 5 columns**.
- **1 hero metric.**

Over the ceiling, split the page. "It fits" is not the standard; "it reads from the back row in eight seconds" is.

---

## 5. Components

- **Rules.** Horizontal rules are 1 px `--paper-rule` / `--ink-500`. The only heavy rule is the **3 px red accent rule** under a slide title or beside a quote — one per page.
- **Callout.** `--red-100` fill, 3 px `--red-600` left border, 24 px padding, no rounded corners beyond 2 px, no shadow.
- **Flags.** `[ASK]` and `[DECIDE]` are set in Sans 600 at `--t-label` in `--red-600`, square brackets included, inline and immediately before the text they qualify. A flag is content, not furniture: it spends the page's one red focal point, so a page carrying a flag does not also carry a red metric.
- **Captions & source lines.** `--t-small` in `--stone-600`, 8 px below the figure, table or chart they belong to and aligned to its left edge — never floated, never in the margin. A source line names document, section and date, and sets identifiers in `--font-mono`.
- **Tables.** No vertical rules. 1 px horizontal rules only. Header row: Sans 600, `--t-label` tracking, `--paper-alt` fill. Numeric columns right-aligned with tabular figures. Zebra striping optional at `--paper-alt`.
- **Code / formula.** `--font-mono` at 18 px on `--paper-alt`, 20 px padding, 2 px radius. Long formulas may drop to 16 px.
- **Corners & shadows.** Border radius `0` or `2px`. **Box shadows are prohibited inside the slide canvas** — depth is expressed by the ink/paper surface change, not by fake elevation. (A 1 px hairline framing the canvas itself against the browser background is not slide content and is permitted.)
- **Images.** Full-bleed or grid-aligned; never floated arbitrarily. Screenshots get a 1 px `--paper-rule` border. Every image needs `alt` text.
- **Icons.** Inline SVG, 1.5 px stroke, `currentColor`. No emoji as iconography. No clip-art.

---

## 6. Motion

- Page transition: **180 ms**, `cubic-bezier(0.22, 0.61, 0.36, 1)`, opacity + ≤ 24 px translate. Nothing else.
- No flips, cubes, dissolves, parallax or auto-playing anything.
- Build-in animations within a page are prohibited; if a point needs staging, make it a second page.
- `@media (prefers-reduced-motion: reduce)` **MUST** disable all transitions.

---

## 7. Interaction

Every deck **MUST** support:

| Input | Action |
|---|---|
| `→` `↓` `Space` `PageDown` | Next page |
| `←` `↑` `PageUp` | Previous page |
| `Home` / `End` | First / last page |
| number keys | Jump to that section's divider |
| `F` | Fullscreen |
| Click right / left half, or swipe | Next / previous page |

Requirements: the current page **MUST** be reflected in the URL hash so a page is linkable and survives reload; page navigation **MUST** work with no network; the deck **MUST** be a **single self-contained `.html` file** (inline CSS and JS) apart from the font CDN link and anything in `assets/`.

`template.html` ships a conforming engine. It derives the section count, the number keys and the progress bar from the markup, so **do not fork or hand-edit it** — add pages and it keeps up on its own.

---

## 8. Output & Files

```
presentations/
├── README.md                               ← how to build, present and export a deck
├── STYLE-GUIDE.md                          ← this document
├── template.html                           ← empty skeleton; copy it to start a deck
├── YYYY-MM-DD-<topic>.html                 ← one delivered deck per file
└── assets/                                 ← images, diagrams, exported charts
```

- Naming: `YYYY-MM-DD-<short-topic>.html`, lower-case and hyphenated — e.g. `2026-08-11-assignment-1-mvp.html`. The date sorts the folder chronologically for free.
- One deck per file. **`template.html` contains no content and MUST stay that way** — copy it, never present from it, never let a finished deck's material settle back into it.
- Assets: `assets/YYYY-MM-DD-<topic>/<name>.png`. Prefer SVG for diagrams; Mermaid diagrams from `docs/` **SHOULD** be exported to SVG rather than screenshotted.
- Decks **MUST** print to a clean PDF via `Ctrl/Cmd + P` (one slide per page, landscape, background graphics on) so a handout can be attached to a submission.
- Decks are committed to the repo like any other artefact. Markdown and HTML only — no `.pptx`, so that every change to a deck diffs in GitHub and reviews like the rest of the project.

---

## 9. Compliance Checklist

Run this before every presentation. Every box **MUST** be ticked.

- [ ] Exactly one section per team member — *n* = the number of members in `docs/project/team.md` — in that file's order, each with a divider and 2–3 content pages
- [ ] Every divider names one person: the member actually presenting it. Any reassignment agreed by the team and recorded in the commit message or minutes (§4.2)
- [ ] Ink surface used only for the cover, the section dividers and the closing page
- [ ] Only the three approved font families; no hard-coded hex outside `:root`
- [ ] No content text below 20 px; nothing below 16 px except 13 px `--t-label` furniture
- [ ] No page exceeds the §4.5 density ceilings
- [ ] Red covers ≲ 10% of every page; one red focal point per page
- [ ] Red text on Ink uses `--red-400`
- [ ] `[ASK]` / `[DECIDE]` flags and source captions are set in their tokens
- [ ] Keyboard, click and hash navigation all work
- [ ] **Opens and presents correctly with the network disconnected**
- [ ] Prints to a clean one-slide-per-page PDF
- [ ] Rehearsed at 1280 × 720 and checked from the back of the room

---

## 10. Amending This Guide

This document is version-controlled. Changes are proposed by pull request, need one other team member's approval, and require the version number and date at the top to be bumped **and a line added to §11**.

**Decks already delivered are not retrofitted.** A deck states the guide version it was built against in its header comment, and that claim is a statement about the day it was presented. If a delivered deck is edited afterwards for any reason, the edit is judged against the *current* version — reopening the file reopens the obligation.

**Deviating from this guide is allowed — silently deviating is not.** If a slide genuinely needs something not covered here, raise it, get agreement, and then write it down for everyone.

---

## 11. Change Log

| Version | Date | Change |
|---|---|---|
| 2.3 | 14 Aug 2026 | §4.2 absence rule rewritten: when a member cannot attend, that section's **owner is reassigned to another member**, agreed on the spot or in the Teams chat, and the divider names **only** the person actually presenting. The `Presenter — for Absent Member` dual-credit form is withdrawn. §9 checklist items 1–2 reworded to match, making *n* = the member count explicit. §10 now states that editing a delivered deck reopens its obligation to the current version. This change log added — earlier versions bumped the number without recording what moved. |
| 2.2 | 14 Aug 2026 | Version in force when the [5 August facilitator checkpoint deck](2026-08-05-facilitator-checkpoint.html) had its section owners filled in. Contents not separately recorded. |
| ≤ 2.1 | — | Not recorded. The guide carried no change log before v2.3, so the history of v1.0 → v2.2 cannot be reconstructed from this file. |
