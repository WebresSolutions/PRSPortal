-- Run after applying database_schema.sql (or equivalent EF migration).

INSERT INTO states(id, name, abbreviation) VALUES (1, 'New South Wales', 'NSW');
INSERT INTO states(id, name, abbreviation) VALUES (2, 'Queensland', 'QLD');
INSERT INTO states(id, name, abbreviation) VALUES (3, 'Victoria', 'VIC');
INSERT INTO states(id, name, abbreviation) VALUES (4, 'South Australia', 'SA');
INSERT INTO states(id, name, abbreviation) VALUES (5, 'Tasmania', 'TAS');
INSERT INTO states(id, name, abbreviation) VALUES (6, 'Western Australia', 'WA');
INSERT INTO states(id, name, abbreviation) VALUES (7, 'Northern Territory', 'NT');

INSERT INTO contact_type (id, name, description) VALUES (1, 'Company', 'Company Contact');
INSERT INTO contact_type (id, name, description) VALUES (2, 'Individual', 'Individual Contact');

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Construction', 'CONSTRUCTION');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Survey', 'Survey');

INSERT INTO job_assignment_type(id, name, description) VALUES(1, 'Current Owner', 'The current owner of the job.');
INSERT INTO job_assignment_type(id, name, description) VALUES(2, 'Responsible Team Member', 'Whom the job is currently assigned to.');

INSERT INTO quote_status(id, name, colour) VALUES (1, 'Draft', '#DBD3D8');
INSERT INTO quote_status(id, name, colour) VALUES (2, 'New', '#81C14B');
INSERT INTO quote_status(id, name, colour) VALUES (3, 'Sent', '#9FBBCC');
INSERT INTO quote_status(id, name, colour) VALUES (4, 'Client Review', '#7D8CC4');
INSERT INTO quote_status(id, name, colour) VALUES (5, 'Lost', '#FFE900');
INSERT INTO quote_status(id, name, colour) VALUES (6, 'Rejected', '#D56062');
INSERT INTO quote_status(id, name, colour) VALUES (7, 'Accepted', '#2E933C');

INSERT INTO file_type(job_type_id, name, description) VALUES
    -- Construction job SharePoint folders (job_type.id = 1)
    (1, '01. Comps Files', 'Construction job folder: comps files.'),
    (1, '02. Field Work', 'Construction job folder: field work.'),
    (1, '03. Received', 'Construction job folder: received.'),
    (1, '04. Sent', 'Construction job folder: sent.'),
    (1, '05. OHS', 'Construction job folder: OHS.'),
    (1, '06. Invoices & POs', 'Construction job folder: invoices and purchase orders.'),
    (1, '07. QA', 'Construction job folder: QA.'),
    (1, '08. Survey Info', 'Construction job folder: survey info.'),
    (1, '09. Title', 'Construction job folder: title.'),
    -- Survey job SharePoint folders (job_type.id = 2)
    (2, '01. Survey Files', 'Survey job folder: survey files.'),
    (2, '02. Field Work', 'Survey job folder: field work.'),
    (2, '03. Received', 'Survey job folder: received.'),
    (2, '04. Sent', 'Survey job folder: sent.'),
    (2, '05. OHS', 'Survey job folder: OHS.'),
    (2, '06. Invoices & PO''s', 'Survey job folder: invoices and purchase orders.'),
    (2, '07. QA', 'Survey job folder: QA.'),
    (2, '08. Survey Info', 'Survey job folder: survey info.'),
    (2, '09. Title', 'Survey job folder: title.'),
    (2, '10. Subdivision', 'Survey job folder: subdivision.');

INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (1, 1, 'Quote', '#1e3a5f');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (2, 1, 'Booked', '#0ea5e9');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (3, 1, 'Scheduled', '#0d9488');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (4, 1, 'Field Complete', '#ea580c');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (5, 1, 'In Review', '#7c3aed');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (6, 1, 'Completed', '#b45309');

INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (1, 2, 'Quote', '#1e3a5f');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (2, 2, 'Booked', '#0ea5e9');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (3, 2, 'Scheduled', '#0d9488');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (4, 2, 'Field Complete', '#ea580c');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (5, 2, 'In Review', '#7c3aed');
INSERT INTO job_status(sequence, job_type_id, name, colour) VALUES (6, 2, 'Completed', '#b45309');

INSERT INTO technical_contact_type (id, name, description) VALUES (1, 'Architect/Draftsperson', 'Architect Or Draftsperson');
INSERT INTO technical_contact_type (id, name, description) VALUES (2, 'Builder', 'Builder');
INSERT INTO technical_contact_type (id, name, description) VALUES (3, 'Previous Client', 'Previous Client');
INSERT INTO technical_contact_type (id, name, description) VALUES (4, 'Technical Contact', 'Technical Contact');

INSERT INTO timesheet_entry_type(name, description) VALUES ('Job', 'Job');
INSERT INTO timesheet_entry_type(name, description) VALUES ('Regular', 'Regular work hours');

INSERT INTO job_task_type (job_type_id, name, description) VALUES
    -- Survey Dept. (job_type.id = 2)
    (2, 'Subdivision', 'Survey department — subdivision workflow.'),
    (2, 'Title Re-establishment Survey', 'Survey department — title re-establishment.'),
    (2, 'Feature & Level Survey', 'Survey department — feature and level survey.'),
    (2, 'Re-estab + Feature Level Survey', 'Survey department — combined re-establishment and F&L.'),
    (2, 'Internal Building Survey', 'Survey department — internal building survey.'),
    (2, 'Application Survey', 'Survey department — application / planning survey.'),
    -- Construction Dept. (job_type.id = 1)
    (1, 'House Set Out', 'Construction department — house set out.'),
    (1, 'Extension Set Out', 'Construction department — extension set out.'),
    (1, 'Medium Density Set Out', 'Construction department — medium density set out.'),
    (1, 'Apartment Set Out', 'Construction department — apartment set out.');

INSERT INTO notification_type (name, description) VALUES ('General', 'A general notification for a user.');

INSERT INTO service_type (code, service_name, description) VALUES
    ('TITLE-REEST', 'Title Re-establishment Survey',
    '<p>Undertake a Title Re-establishment Survey to determine and mark the title boundaries of the subject land.</p>
    <ul>
        <li>Research current survey information.</li>
        <li>Undertake field Survey (including marking of title boundaries).</li>
        <li>Preparation of Plan of Title Re-establishment Survey detailing:
            <ul>
                <li>Lot boundaries and dimensions (including boundary marks placed)</li>
                <li>Location of easements</li>
                <li>Relationship of occupation/fencing to title boundaries.</li>
            </ul>
        </li>
        <li>Preparation of a written report detailing title particulars (including covenants and easements), methodology, and key findings of the Title Re-establishment Survey.</li>
        <li>Preparation of &ldquo;Record of Having Re-established a Parcel&rdquo;, as per Regulation 16 of the Surveying (Cadastral Surveys) Regulations 2025</li>
    </ul>'),

    ('FEATURE-AHD', 'Feature & AHD Level Survey', '<p>Undertake a Feature &amp; AHD Level Survey to document existing site features and levels for design, planning and construction purposes.</p>
    <ul>
        <li>Connect survey to Australian Height Datum (A.H.D) and place Temporary Benchmark (T.B.M) for future construction purposes.</li>
        <li>Connect survey into MGA2020 survey control marks and orient survey to MGA2020 bearings.</li>
        <li>Measure levels and contours (0.2m intervals) across site.</li>
        <li>Measure spot levels on adjoining properties within 1m of fence/boundary (note: only where accessible).</li>
        <li>Locate existing buildings, sheds and garages (inc. floor level and building heights).</li>
        <li>Locate windows on the existing buildings (inc. head and sill heights).</li>
        <li>Location of significant trees (3m or taller) and details of approximate height, spread and trunk dimensions.</li>
        <li>Locate fences (including materials and height), significant features (including driveways and hard surfaces) and visible services on the subject site.</li>
        <li>Search Before You Dig (BYD) data and overlay underground assets within 9m of property (quality level D only)</li>
        <li>Locate Road details including back &amp; invert of kerb, visible services &amp; crossovers directly in front of the subject site.</li>
        <li>Locate adjoining dwellings within 9m of site, including:
            <ul>
                <li>Building setbacks</li>
                <li>Windows facing the subject site (inc. head &amp; sill heights)</li>
                <li>Top of gutter</li>
                <li>Ridges, parapets, chimneys etc.</li>
                <li>Floor level (measured at front door thresholds only if easily accessible).</li>
            </ul>
        </li>
        <li>Measure location and setbacks of further surrounding dwellings (three properties either side).</li>
        <li>Show location of private open space and pools (where unable to survey, aerial imagery will be used).</li>
        <li>Supply photos of site.</li>
        <li>Provide 3D triangle file for surface/contours to allow integration into compatible software.</li>
        <li>Preparation of Plan of Feature Survey (digital copy provided in PDF and DWG format).</li>
    </ul>'),
    ('NEIGH-SITE', 'Neighbourhood Site Description', '<p>Provide a Neighbourhood Site Description of the surrounding area (within 50m of the site) to assist with design response and planning documentation for the proposed development.</p>
    <ul>
        <li>Measure and record surrounding buildings within the immediate neighbourhood context, including dwelling type, building siting and visible setbacks to street frontages.</li>
        <li>Identify roof forms of surrounding dwellings and buildings, where visible from accessible vantage points and aerial imagery.</li>
        <li>Show relevant site characteristics, adjoining context and local amenity features on the plan.</li>
        <li>Capture photographs of surrounding properties to document the existing neighbourhood and streetscape character.</li>
    </ul>'),
    ('INTERNAL-SURVEY', 'Internal Building Survey', '<p>Undertake an internal building survey of the existing dwelling to capture the principal internal layout and visible built elements for design and documentation purposes.</p>
    <ul>
        <li>Undertake an internal survey of the existing dwelling, including principal room dimensions, wall layouts, window locations and ceiling heights.</li>
        <li>Locate and record visible fixed internal elements relevant to the building layout, including cabinetry, bulkheads and other permanent built features where reasonably accessible.</li>
        <li>Prepare a plan showing the internal layout and measurements based on the surveyed information above.</li>
        <li>Supply digital copies of the completed plan via e-mail.</li>
    </ul>'),
    ('EVLEVATION-PLAN', 'Elevations Plan', '<p>Prepare elevation drawings of the existing building to document the external built form and visible f&ccedil;ade elements for design and documentation purposes.</p>
    <ul>
        <li>Record visible external elements including windows, doors, frames, chimneys, vents, roof details and other relevant building features.</li>
        <li>Prepare four elevation plans of the existing building, being the external faces of the building on all four sides.</li>
        <li>Supply digital copies of the completed elevation drawings via e-mail.</li>
    </ul>');
