# Presentations

All project presentations live here. They are plain HTML — one self-contained file per deck — so they diff in GitHub and review like every other artefact.

## Contents

| File | Purpose |
|---|---|
| `STYLE-GUIDE.md` | **Binding** style policy: colour, typography, layout, structure. Read before touching anything. |
| `template.html` | Empty skeleton implementing the guide. Copy it; never present from it; never put content in it. |
| `assets/` | Images, diagrams and exported charts, one subfolder per deck. |
| `YYYY-MM-DD-<topic>.html` | Delivered decks, one per file. |

## Starting a new deck

```bash
cp presentations/template.html presentations/2026-08-18-assignment-1-mvp.html
```

Then edit the copy. The skeleton already contains the mandatory spine — cover → one section per team member → closing — with every content slot marked `TODO`. Work through them in order; the deck is finished when no `TODO` remains.

Three things to set first, all marked in the file:

1. The `<title>` — the tab name and the PDF filename come from it.
2. The cover title, descriptor and date.
3. Each section's title and owner. **Owners and their order come from [`docs/project/team.md`](../docs/project/team.md)** — that file is the roster's only home, so read it rather than remembering.

The deck has exactly as many sections as there are members in `docs/project/team.md`, one each. **If someone cannot present on the day, that section's owner is reassigned to another member** — agree it on the spot or in the Teams chat, change the divider to name only the person actually presenting, and note the swap in the commit message. Never delete or merge a section to cover an absence; one person can carry two. Full rule: [`STYLE-GUIDE.md`](STYLE-GUIDE.md) §4.2.

Page numbers, section indicators and the progress bar are generated at runtime. Add or delete a page and they stay correct on their own; never type a page number by hand.

## Presenting

Open the file in any browser. `F` for fullscreen.

| Key | Action |
|---|---|
| `→` `↓` `Space` `PgDn` | Next page |
| `←` `↑` `PgUp` | Previous page |
| `Home` / `End` | First / last page |
| `1`–`9` | Jump to that section |
| `F` | Fullscreen |

Clicking the right or left half of the screen also pages forward and back, as does swiping on a tablet. The current page is written to the URL hash, so a slide is linkable and survives a reload.

**Fonts load from the Google Fonts CDN with a system fallback.** Open the deck once with the network off before any client meeting.

## Exporting a PDF handout

`Ctrl/Cmd + P` → Landscape → enable **Background graphics** → Save as PDF. One slide per page.

## Before you present

Run the compliance checklist in [`STYLE-GUIDE.md`](STYLE-GUIDE.md) §9. Every box must be ticked.
