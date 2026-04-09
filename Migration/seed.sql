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
INSERT INTO quote_status(id, name, colour) VALUES (4, 'Lost', '#FFE900'); 
INSERT INTO quote_status(id, name, colour) VALUES (5, 'Rejected', '#D56062');
INSERT INTO quote_status(id, name, colour) VALUES (6, 'Accepted', '#2E933C');

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
    ('TITLE-REEST', 'Title Re-establishment Survey', ''),
    ('FEATURE-AHD', 'Feature & AHD Level Survey', ''),
    ('NEIGH-SITE', 'Neighbourhood Site Description', ''),
    ('INTERNAL-SURVEY', 'Internal Building Survey', ''),
    ('EVLEVATION-PLAN', 'Elevations Plan', '');
