# PRS Portal / Database Planning – Summary & Todo

**Source:** Meeting notes 12 March & 18 March 2026 (Nadine, Brodie, Josh, Jordan)  
**Status:** Planning / requirements gathering. Brodie, Josh, Nadine to develop task lists; then meeting to finalise before handover to Jordan.

---

## Executive summary

- **Goal:** One database/portal for jobs, scheduling, contacts, councils, timesheets, quotes, invoicing; automate as much as possible; link to SPEAR, Xero, SharePoint, mapping.
- **Job numbers:** Yearly, sequential; single numbering (no separate construction/survey); visual distinction (e.g. colour/type) for construction vs survey; subdivision treatment TBD (survey vs own type).
- **Home:** Customisable per user; PRS logo; checklist removed and replaced/linked to SPEAR or new build.
- **Progress bar:** Mandatory – per-job status, responsible person, type-specific (subdivision, survey, construction); ties to task lists and background colour on schedule.
- **Files:** SharePoint access from database; subfolders per department (survey & construction structures provided); file size/storage TBD (Luke/Rob Hore).
- **Contacts:** Two types – Company (with sub “Personnel”) and Individual; company allows assigning personnel to jobs; consider Outlook drag-and-drop for email.
- **Schedule:** 6am–6pm; day view with all tracks side by side; drag-and-drop in week view with confirm/delete safety; placeholder bookings when no job exists yet; clock UI to change; map view (day + track); vehicle/equipment register on tracks; leave sync from Xero.
- **Job status colours:** Border/side = construction vs survey; background = progress-bar status (comped, access confirmed, checked, ready for invoicing, etc.).
- **Notes:** Chronological; action notes highlighted (e.g. red); follow-up/reminders (e.g. 7 days); urgent/due-date alerts to requestor; optional subdivision notes tab.
- **Timesheets:** Currently user-specific; consider job-specific; link to Excel; standardise survey vs construction; tie to pricing/cost analysis (not always direct to invoice).
- **Quotes / Invoicing:** Quotes via DB → client accepts via DB → job created; invoices from job (quote/timesheet where relevant), human “click” to issue; future: client payment (CC/Apple Pay); Xero integration.
- **Councils / SPEAR:** Integrate or pull data from SPEAR; standard letters per council; automate planning-permit → letter; one-click send signed RE sketch to client; Brodie to get Jordan SPEAR access.
- **Other:** Client portal (“here’s your login”); dormant/inactivity alerts (e.g. 2–5 days general, 2 weeks subdivision); Lassi API for mapping (new estates); invoicing, quoting, mobile access flagged for later.

---

## Todo list (actionable items)

### Access & integration
- [ ] **SPEAR:** Brodie to get Jordan access to SPEAR; investigate pulling data into DB or linking SPEAR to DB.
- [ ] **SPEAR – checklist:** Replace front-page checklist with SPEAR link or new build.
- [ ] **SharePoint:** Enable access to SharePoint through the database.
- [ ] **Xero:** Leave sync so leave shows on schedule and blocks booking; decide: leave approved in Xero → DB or DB → Xero (manager per user if DB).
- [ ] **Lassi API:** Brodie to provide info to Jordan for mapping (street/lot in new estates).
- [ ] **Quotes/Invoicing storage:** Confirm storage for quotes and invoices that will talk to Xero.

### Home & dashboard
- [ ] **Home screen:** Make customisable per user.
- [ ] **PRS logo:** Add PRS logo/visual on home screen.
- [ ] **Progress bar (must-have):** Implement progress bar showing where a job is and who is responsible; type-specific (subdivision, survey, construction). Brodie/Josh/Nadine to define tasks and touchpoints.
- [ ] **Progress bar ↔ schedule:** When a job is created/scheduled and allocated to a team, background colour of schedule box = progress bar stage.
- [ ] **Progress bar ↔ emails:** Automated client emails when certain touchpoints are hit (e.g. construction: “set out done”); Brodie/Josh/Nadine to define.
- [ ] **Job status colours:** Side/border = construction vs survey; background = progress stage (comped, access confirmed, checked, ready for invoicing, etc.); define stages with Josh/Brodie/Nadine.

### Jobs
- [ ] **Job numbers:** Yearly, sequential; single series; visual distinction construction vs survey; decide if subdivision = “survey” or own type/colour.
- [ ] **Job address:** Required when creating a job.
- [ ] **Subdivision:** Decide how to indicate subdivision (survey vs own colour/type) given survey + construction overlap.
- [ ] **Task list:** Brodie/Josh/Nadine to create list of tasks and how they tie into Progress Bar.
- [ ] **Placeholder bookings:** Allow booking date/time when no job exists yet (e.g. call received, no job file).

### Job files & storage
- [ ] **Subfolders:** Implement department subfolders – use provided Survey and Construction folder structures (may be refined).
- [ ] **File size/storage:** Confirm types and max size; discuss with Luke or Rob Hore (e.g. 10–100 MB typical; some jobs in GBs).
- [ ] **Quotes/Invoices:** Ensure storage for quotes and invoices that integrate with Xero.

### Scheduling
- [ ] **Hours:** Set standard scheduling hours to 6am–6pm.
- [ ] **Day view:** Show all job booking tracks side by side.
- [ ] **Week view:** Click-and-drag to move bookings; confirm/delete prompt; prevent jobs disappearing.
- [ ] **Clock UI:** Replace current clock interface (Brodie not a fan); Jordan’s current one is placeholder.
- [ ] **Map view:** Day view and track view on map; consider Google Maps vs SPEAR for new estates.
- [ ] **Vehicle & equipment register:** Add to job tracks (assign cars/equipment to jobs/tracks/days).

### Contacts
- [ ] **Contact types:** Implement Company vs Individual. Company can have Personnel (sub-contacts); Individual cannot.
- [ ] **Personnel on jobs:** Allocate specific company personnel to a job from company’s personnel list.
- [ ] **Outlook:** Explore drag-and-drop email from Outlook when creating Personnel contact (Josh).

### Job notes
- [ ] **Chronological:** All notes chronological; do not sort “red/action” notes to top.
- [ ] **Action styling:** Actioned items stand out (e.g. red) to assignee.
- [ ] **Follow-up:** After 7 days, if action note not actioned, notify original requestor.
- [ ] **Urgent/due date:** If task is urgent or has due date, email alert to requestor if not actioned by date; if not urgent but “date needed by” set, pop-up after 7 days.
- [ ] **Subdivision notes:** Consider separate subdivision notes tab; confirm if Progress Bar makes it redundant.
- [ ] **Critical dates:** Option to set timing/urgency/awareness for critical dates.

### Timesheets
- [ ] **Current:** Jordan’s timesheet is user-specific, not job-specific – decide approach.
- [ ] **Excel:** Consider linking timesheet to existing Excel spreadsheet.
- [ ] **Standardise:** Brodie and Josh to work on standardised timesheet (at least survey vs construction).
- [ ] **Template:** Send timesheet/pricing template to Jordan; Brodie/Josh to standardise construction and survey.
- [ ] **Job times:** For job-assigned timesheets, show time entries and total hours per job.
- [ ] **Pricing reports:** Ability to track pricing and produce Excel reports.
- [ ] **Invoicing:** Not always from timesheet; sometimes from quote; invoice created from job info (quote/timesheet where relevant), person must “click” to process.

### Quotes & invoicing (TBD / later)
- [ ] **Quotes:** Generate/send via system; client accept via system; accepted quote → job in DB (confirm with PRS team).
- [ ] **Invoicing:** Create from job (quote, sometimes timesheet); human click to issue; future client payment (CC/Apple Pay).
- [ ] **Batch invoicing:** Confirmed not used.
- [ ] **Quotes section:** TBD – confirm with PRS team.

### Councils & SPEAR
- [ ] **SPEAR:** DB to talk to SPEAR (subdivision/planning permits Vic).
- [ ] **Standard letters:** Per-council standard letters for planning permits; automate (e.g. permit received → match to standard letter → merged doc for review).
- [ ] **RE sketch:** One-click send signed lodged RE sketch to client (DB ↔ SPEAR).
- [ ] **Councils updates:** TBD – confirm with team.

### Client portal & dormant
- [ ] **Client portal:** “Here’s your login” – limited client access to relevant sections (e.g. subdivision clients).
- [ ] **Dormant/inactivity:** If no activity 3–5 days (general) or 2 weeks (subdivision), alert responsible person; jobs need “allocated to responsible person”; subdivisions to subs officer.

### Automation (general)
- [ ] **Maximise automation:** Quotes, acceptance, job creation, letters, SPEAR, invoicing, client comms – automate wherever possible.
- [ ] **Council example:** Planning permit received → base letter from DB + SPEAR, standards matched, human adjust.

### Flagged for later
- [ ] Invoicing workflow and Xero.
- [ ] Quoting workflow and templates.
- [ ] Mobile access.

---

## Next steps (from email)

- **Brodie & Josh:** Develop task info in the next week; schedule meeting to finalise/get detail; then through to Jordan.
- **Brodie/Josh/Nadine:** Create task lists and Progress Bar touchpoints for subdivision, survey, construction; define job stages and colours; define automated email triggers.
- **Nadine:** Review 12 March email and 18 March notes; link items (bold/blue in original = 18 March additions).

---

## Reference – file structures (from 12 March email)

- **Survey:** (structure listed in email – use as reference for subfolders.)
- **Construction:** (structure listed in email – use as reference for subfolders.)

*(Exact folder names can be copied from the original email if not already in repo.)*
