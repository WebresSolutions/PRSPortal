INSERT INTO app_user (
    identity_id,
    email,
    display_name,
    legacy_user_id
)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    'testuser@example.com',
    'Test User',
    0
);

INSERT INTO app_user (
    identity_id,
    email,
    display_name,
    legacy_user_id
)
VALUES (
    '550e8400-e29b-41d4-a716-446655440001',
    'testuser2@example.com',
    'Test User 2',
    1
);

INSERT INTO timesheet_entry_type(name, description) VALUES ('Job', 'Job');
INSERT INTO timesheet_entry_type(name, description) VALUES ('Regular', 'Regular work hours');

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Construction', 'CONSTRUCTION');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Survey', 'Survey');

INSERT INTO job_assignment_type(id, name, description) VALUES (1, 'Current Owner', 'The current owner of the job.');
INSERT INTO job_assignment_type(id, name, description) VALUES (2, 'Responsible Team Member', 'Whom the job is currently assigned to.');

INSERT INTO file_type(job_type_id, name, description) VALUES
    -- Construction job SharePoint folders (job_type.id = 1)
    (1, '01 COMPS FILES', 'Construction job folder: comps files.'),
    (1, '02 FIELDWORK', 'Construction job folder: field work.'),
    (1, '03 RECEIVED', 'Construction job folder: received.'),
    (1, '04 SENT', 'Construction job folder: sent.'),
    (1, '05 OHS', 'Construction job folder: OHS.'),
    (1, '06 INVOICES & POs', 'Construction job folder: invoices and purchase orders.'),
    (1, '07 QA', 'Construction job folder: QA.'),
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

INSERT INTO service_type (code, service_name, description) VALUES
    ('TITLE-REEST', 'Title Re-establishment Survey', ''),
    ('FEATURE-AHD', 'Feature & AHD Level Survey', ''),
    ('NEIGH-SITE', 'Neighbourhood Site Description', ''),
    ('INTERNAL-SURVEY', 'Internal Building Survey', ''),
    ('EVLEVATION-PLAN', 'Elevations Plan', '');

INSERT INTO quote_status(id, name, colour) VALUES (1, 'Draft', '#cccccc');
INSERT INTO quote_status(id, name, colour) VALUES (2, 'New', '#fffff');
INSERT INTO quote_status(id, name, colour) VALUES (3, 'Sent', '#0d9488');
INSERT INTO quote_status(id, name, colour) VALUES (4, 'Lost', '#dc2626');
INSERT INTO quote_status(id, name, colour) VALUES (5, 'Rejected', '#ea580c');
INSERT INTO quote_status(id, name, colour) VALUES (6, 'Accepted', '#16a34a');

INSERT INTO states(id, name, abbreviation) VALUES (1, 'New South Wales', 'NSW');
INSERT INTO states(id, name, abbreviation) VALUES (2, 'Queensland', 'QLD');
INSERT INTO states(id, name, abbreviation) VALUES (3, 'Victoria', 'VIC');
INSERT INTO states(id, name, abbreviation) VALUES (4, 'South Australia', 'SA');
INSERT INTO states(id, name, abbreviation) VALUES (5, 'Tasmania', 'TAS');
INSERT INTO states(id, name, abbreviation) VALUES (6, 'Western Australia', 'WA');
INSERT INTO states(id, name, abbreviation) VALUES (7, 'Northern Territory', 'NT');

INSERT INTO contact_type (id, name, description) VALUES (1, 'Company', 'Company Contact');
INSERT INTO contact_type (id, name, description) VALUES (2, 'Individual', 'Individual Contact');

INSERT INTO technical_contact_type (id, name, description) VALUES (1, 'Architect/Draftsperson', 'Architect Or Draftsperson');
INSERT INTO technical_contact_type (id, name, description) VALUES (2, 'Builder', 'Builder');
INSERT INTO technical_contact_type (id, name, description) VALUES (3, 'Previous Client', 'Previous Client');
INSERT INTO technical_contact_type (id, name, description) VALUES (4, 'Technical Contact', 'Technical Contact');

insert into schedule_colour ( color, description) values('#FF0000', 'Test Colour');
insert into schedule_colour ( color, description) values('#FFFFFF', 'Test Colour 2');

INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash)
    VALUES ('123 Maple Avenue', 'Richmond', 1, '3121', 'Australia', 1, ST_SetSRID(ST_Point(144.9913, -37.8230), 4326), 'r1p9g5');
INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash)
    VALUES ('456 Ocean Parade', 'Bondi', 2, '2026', 'Australia', 1, ST_SetSRID(ST_Point(151.2743, -33.8915), 4326), 'r3gx2s');
INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash)
    VALUES ('78 Mountain View Road', 'Sandy Bay', 3, '7005', 'Australia', 1, ST_SetSRID(ST_Point(147.3340, -42.9030), 4326), 'r22m6v');

INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id, type_id)
    VALUES ('Alice', 'Smith', 'jordan@ws1.com.au', '+61 411 222 333', 1, 1, 2);
INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id, parent_contact_id, type_id)
    VALUES ('Bob', 'Jones', 'jordan@ws1.com.au', '+61 422 333 444', 2, 1, 1, 2);
INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id, type_id)
    VALUES ('Charlie', 'Brown', 'jordan@ws1.com.au', '+61 433 444 555', 3, 1, 2);

INSERT INTO council (name, address_id, phone, email, website, created_by_user_id)
    VALUES ('City of Melbourne', 1, '03 9658 9658', 'enquiries@melbourne.vic.gov.au', 'https://www.melbourne.vic.gov.au', 1);
INSERT INTO council (name, address_id, phone, email, website, created_by_user_id)
    VALUES ('Waverley Council', 2, '02 9083 8000', 'info@waverley.nsw.gov.au', 'https://www.waverley.nsw.gov.au', 1);
INSERT INTO council (name, address_id, phone, email, website, created_by_user_id)
    VALUES ('Hobart City Council', 3, '03 6238 2711', 'coh@hobartcity.com.au', 'https://www.hobartcity.com.au', 1);

INSERT INTO council_contact (council_id, contact_id, address_id, created_by_user_id)
    VALUES (1, 1, 1, 1);
INSERT INTO council_contact (council_id, contact_id, address_id, created_by_user_id)
    VALUES (2, 2, 2, 1);
INSERT INTO council_contact (council_id, contact_id, address_id, created_by_user_id)
    VALUES (3, 3, 3, 1);

INSERT INTO job_colour (color)
    VALUES ('#3498db');
INSERT INTO job_colour (color)
    VALUES ('#e74c3c');
INSERT INTO job_colour (color)
    VALUES ('#2ecc71');

INSERT INTO job (contact_id, address_id, council_id, job_colour_id, invoice_number, job_number, details, created_by_user_id)
    VALUES (1, 1, 1, 1, 'INV-2026-001', 1001, 'Emergency pipe repair at main square', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, invoice_number, job_number, details, created_by_user_id)
    VALUES (2, 2, 2, 1, 'INV-2026-002', 1002, 'Scheduled vegetation clearing near coastal path', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, invoice_number, job_number, details, created_by_user_id)
    VALUES (3, 3, 3, 1, 'INV-2026-003', 1003, 'Graffiti removal from heritage building facade', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, invoice_number, job_number, details, created_by_user_id)
    VALUES (1, 1, 1, 1, 'INV-2026-004', 1004, 'Follow-up inspection for site drainage', 1);

INSERT INTO job_note (job_id, note, action_required, created_by_user_id)
    VALUES (1, 'Initial site inspection complete. Waiting on parts.', FALSE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id)
    VALUES (2, 'Client requested a callback regarding the vegetation clearing boundary.', TRUE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id)
    VALUES (3, 'Graffiti removal started. High-pressure cleaner in use.', FALSE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id)
    VALUES (4, 'Graffiti removal started. High-pressure cleaner in use.', TRUE, 1);
