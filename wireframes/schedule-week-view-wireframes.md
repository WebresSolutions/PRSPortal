# Schedule – Week View Wireframes

Week view for the Schedule page, aligned with the existing **single-day view** in the new system (`Portal.Client/Pages/Schedule.razor`).

## Single-day view (reference)

- **Route:** `/schedule` or `/schedule/{date}/{jobType}`
- **Header:** Title “Schedule – [JobType]”, date as `dddd, MMMM d, yyyy`, `MudDatePicker`, “Add Staff Track” button, status badge, view/edit/delete icons
- **Content:** Grid of staff tracks (one card per track). Each card has “Assigned Users” and a **MudCalendar** in **Day** view with events (job number, address, notes)

## Week view options

### 1. Table layout — `schedule-week-view-table.html`

- **Use when:** You want to mirror the legacy Schedule page and keep a single scrollable table.
- **Structure:**
  - Same header as single-day: “Schedule – Cadastrals”, week range, **Previous / Date / Next** for week navigation, badge, “Add Staff Track”, view/edit/delete.
  - Main: one table with columns **Day | Parties | Time | Job Number | Address | Notes**. Rows grouped by day (day group header row, then detail rows).
  - Optional right sidebar: **Job Search**, **Contact Search**, **Task Search** (same fields as legacy).
- **Pros:** Dense, familiar, good for print/export.  
- **Cons:** Less “card per track” than the new single-day grid.

### 2. Cards layout — `schedule-week-view-cards.html`

Two variants in one file:

- **Option A – 7 day columns**
  - Week as 7 columns (Mon–Sun). Each column is a card for that day, containing a list of schedule items (party, time, job, address, notes).
  - Closest to “one column = one day” like the single-day view, but compressed into a week strip.
- **Option B – Stacked day sections**
  - One section per day (e.g. “Tue, Mar 10, 2026”). Inside each section, a **card grid** of schedule items (same card style as single-day staff tracks).
  - Reuses the single-day idea of “cards in a grid” but repeated per day.

**Pros:** Visually consistent with the new single-day design; good for responsive (columns collapse).  
**Cons:** Less dense than the table; Option A can feel tight on small screens.

## Recommendation

- Use **Table** if the main goal is parity with the old week view and maximum information density.
- Use **Cards (Option B – stacked day sections)** if you want the week view to feel like “multiple single-day views” and reuse the same card/grid components.

## Implementation notes

- Reuse **PageHeader** (or same header structure) and **StatusBadge** from the single-day Schedule page.
- Week navigation: either `MudDatePicker` with a “week” mode or Previous/Next that move by 7 days and show the week range in the header.
- Single-day and week view can share the same route base (`/schedule`) with a toggle or tab (e.g. “Day” vs “Week”), or separate routes (e.g. `/schedule/week/{date}/{jobType}`).
