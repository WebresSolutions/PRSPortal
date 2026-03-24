-- Run after applying database_schema.sql (or equivalent EF migration).

INSERT INTO states(id, name, abbreviation) VALUES (1, 'New South Wales', 'NSW');
INSERT INTO states(id, name, abbreviation) VALUES (2, 'Queensland', 'QLD');
INSERT INTO states(id, name, abbreviation) VALUES (3, 'Victoria', 'VIC');
INSERT INTO states(id, name, abbreviation) VALUES (4, 'South Australia', 'SA');
INSERT INTO states(id, name, abbreviation) VALUES (5, 'Tasmania', 'TAS');
INSERT INTO states(id, name, abbreviation) VALUES (6, 'Western Australia', 'WA');
INSERT INTO states(id, name, abbreviation) VALUES (7, 'Northern Territory', 'NT');

INSERT INTO contact_type (name, description) VALUES ('Company', 'Company Contact');
INSERT INTO contact_type (name, description) VALUES ('Personal', 'Personal Contact');

INSERT INTO file_type(id, name, description) VALUES (1, 'General', 'General Files.');
INSERT INTO file_type(id, name, description) VALUES (2, 'Job', 'General Job Files.');
INSERT INTO file_type(id, name, description) VALUES (3, 'Surveying', 'Surveying Files.');
INSERT INTO file_type(id, name, description) VALUES (4, 'Construction', 'Construction Files.');

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Construction', 'CONSTRUCTION');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Survey', 'Survey');

INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (1, 1, 'Quote', '#1e3a5f');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (2, 1, 'Booked', '#0ea5e9');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (3, 1, 'Scheduled', '#0d9488');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (4, 1, 'Field Complete', '#ea580c');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (5, 1, 'In Review', '#7c3aed');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (6, 1, 'Completed', '#b45309');

INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (1, 2, 'Quote', '#1e3a5f');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (2, 2, 'Booked', '#0ea5e9');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (3, 2, 'Scheduled', '#0d9488');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (4, 2, 'Field Complete', '#ea580c');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (5, 2, 'In Review', '#7c3aed');
INSERT INTO job_status(status_position, job_type_id, name, colour) VALUES (6, 2, 'Completed', '#b45309');

INSERT INTO technical_contact_type (id, name, description) VALUES (1, 'Architect/Draftsperson', 'Architect Or Draftsperson');
INSERT INTO technical_contact_type (id, name, description) VALUES (2, 'Builder', 'Builder');
INSERT INTO technical_contact_type (id, name, description) VALUES (3, 'Previous Client', 'Previous Client');
INSERT INTO technical_contact_type (id, name, description) VALUES (4, 'Technical Contact', 'Technical Contact');

INSERT INTO quote_status(name) VALUES ('Draft');
INSERT INTO quote_status(name) VALUES ('Lost');
INSERT INTO quote_status(name) VALUES ('Rejected');
INSERT INTO quote_status(name) VALUES ('Sent');

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

INSERT INTO notification_type (name, description) VALUES ('General', 'A general notification for a user.')
