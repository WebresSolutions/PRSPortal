# Job Detail Page - Modern UI Wireframe

## Overview
This wireframe describes the modern UI design for the Job Detail page, matching the clean, card-based design style of the Dashboard.

## Layout Structure

### Header Section
- **Job Title**: Large, bold heading showing "Job #[JobId] | [JobNumber]"
- **Job Type Badge**: Status badge showing job type (Construction/Surveying) with color coding
- **Back Button**: Navigation button to return to Jobs list
- **Action Buttons**: Edit, Lock, Print icons (top right)

### Main Content Area

#### Top Row - Key Information Cards (4 cards in a row)
1. **Job Number Card**
   - Title: "JOB NUMBER"
   - Value: Large number display
   - Subtitle: Job type indicator

2. **Address Card**
   - Title: "LOCATION"
   - Value: Full address (Street, Suburb, State, Postcode)
   - Subtitle: Map links (Street Directory, Google Maps)

3. **Contact Card**
   - Title: "CONTACT"
   - Value: Contact name
   - Subtitle: Phone, Mobile, Email (if available)

4. **Status Card**
   - Title: "STATUS"
   - Value: Current status indicator
   - Subtitle: Last modified date

#### Middle Section - Two Column Layout

**Left Column (66% width):**

1. **Job Details Card**
   - Header: "Job Details" with edit icon
   - Content:
     - Council: [Council Name]
     - Map Reference: [Reference]
     - Last Plan Reference: [Reference]
     - Schedule Colour: [Colour indicator]
     - Other Links: [Links]
   - Footer: Created/Modified timestamps

2. **Notes Card**
   - Header: "Notes" with tabs (All, Requiring Action, Deleted) and "+" button
   - Content: List of notes
     - Each note shows:
       - Note content
       - Action Required badge (if applicable)
       - Created date and user
       - Action icons (Edit, View, Delete)

**Right Column (33% width):**

1. **Quick Actions Card**
   - Header: "Quick Actions"
   - Content:
     - Add Technical Contact button
     - Add Task button
     - Add Invoice button
     - Add Site Visit button
     - Add Note button

2. **Recent Activity Card**
   - Header: "Recent Activity"
   - Content: Timeline of recent changes/updates

#### Bottom Section - Full Width Cards

1. **Technical Contacts Card**
   - Header: "Technical Contacts" with tabs (All, Deleted) and "+" button
   - Content: Table/List showing:
     - Role
     - Contact Name
     - Phone
     - Mobile
     - Email
     - Actions (Edit, Delete)

2. **Tasks Card**
   - Header: "Tasks" with tabs (All, Active, To Invoice, Deleted) and "+" button
   - Content: Table/List showing:
     - Task Description
     - Quoted Price
     - Active Date
     - Completed Date
     - Invoiced Date
     - Actions (Edit, Delete)

3. **Invoices Card**
   - Header: "Invoices" with tabs (All, Unsent, Unpaid, Overdue, Deleted) and "+" button
   - Content: Table/List showing:
     - Invoice Number
     - Contact
     - Total Price
     - Created Date
     - Sent Date
     - Paid Date
     - Actions (Edit, Delete)

4. **Checklist Card**
   - Header: "Checklist"
   - Content: Table with columns:
     - Item Name
     - Site Work Completed (dropdown: Not Completed, Completed, etc.)
     - Checked (checkbox)
     - Sent (checkbox)
   - Items include:
     - RE Survey
     - Feature Plan
     - MGA
     - Unit Location
     - Boundary Marking
     - Abstract of Field Records
     - Excavation Set Out
     - Set out of Dwelling(s)
     - Pin Footings

5. **Site Visits Card**
   - Header: "Site Visits" with "+" button
   - Content: Table/List showing:
     - Date
     - Time
     - Assignees
     - Category
     - Actions (Edit, Delete)

## Design Specifications

### Color Scheme
- Primary: Blue (#var(--color-primary))
- Success: Green (#var(--color-success))
- Warning: Orange/Yellow (#var(--color-yellow))
- Background: Light grey (#var(--color-background))
- Cards: White with shadow

### Typography
- Headers: Bold, large (1.5rem - 2rem)
- Card Titles: Bold, medium (1.25rem)
- Body Text: Regular (0.95rem)
- Labels: Small, uppercase, letter-spaced (0.9rem)

### Spacing
- Card padding: 1.5rem
- Card margin: 1rem
- Section gap: 2rem
- Element gap: 0.5rem - 1rem

### Card Styling
- White background
- Border radius: var(--border-rad)
- Box shadow: shadow class
- Hover effects: Subtle elevation on hover

### Responsive Behavior
- Mobile: Single column, stacked cards
- Tablet: Two column layout for middle section
- Desktop: Full layout as described

## Component Patterns

### Status Badges
- Rounded pills with colored backgrounds
- Text in white or dark depending on background
- Used for: Job Type, Action Required, Status indicators

### Action Buttons
- Primary: Blue background, white text
- Secondary: Outlined style
- Icon buttons: Circular with icons
- "+" buttons: Green, for adding new items

### Tables/Lists
- Clean, minimal design
- Alternating row colors (striped)
- Hover effects on rows
- Action icons in last column

### Tabs
- Underline style tabs
- Active tab highlighted
- Smooth transitions

## Interactive Elements

### Modals/Dialogs
- Used for: Adding/Editing items
- Overlay with backdrop
- Centered content
- Close button in top right

### Forms
- Input fields with labels
- Validation feedback
- Submit/Cancel buttons
- Loading states

## Notes
- All cards should have consistent styling matching the Dashboard
- Use the same grid system as Dashboard (grid-row, grid-col)
- Maintain consistent spacing and typography
- Ensure accessibility (keyboard navigation, screen readers)
- Loading states should show skeleton loaders
- Empty states should show helpful messages

