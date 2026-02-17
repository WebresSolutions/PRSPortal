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

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Construction', 'CONSTRUCTION');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Survey', 'Survey');

INSERT INTO states(id, name, abbreviation) VALUES (1, 'New South Wales', 'NSW');
INSERT INTO states(id, name, abbreviation) VALUES (2, 'Queensland', 'QLD');
INSERT INTO states(id, name, abbreviation) VALUES (3, 'Victoria', 'VIC');
INSERT INTO states(id, name, abbreviation) VALUES (4, 'South Australia', 'SA');
INSERT INTO states(id, name, abbreviation) VALUES (5, 'Tasmania', 'TAS');
INSERT INTO states(id, name, abbreviation) VALUES (6, 'Western Australia', 'WA');
INSERT INTO states(id, name, abbreviation) VALUES (7, 'Northern Territory', 'NT');


INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash) 
    VALUES ('123 Maple Avenue', 'Richmond', 1, '3121', 'Australia', 1, ST_SetSRID(ST_Point(144.9913, -37.8230), 4326), 'r1p9g5');
INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash) 
    VALUES ('456 Ocean Parade', 'Bondi', 2, '2026', 'Australia', 1, ST_SetSRID(ST_Point(151.2743, -33.8915), 4326), 'r3gx2s');
INSERT INTO address (street, suburb, state_id, post_code, country, created_by_user_id, geom, geohash) 
    VALUES ('78 Mountain View Road', 'Sandy Bay', 3, '7005', 'Australia', 1, ST_SetSRID(ST_Point(147.3340, -42.9030), 4326), 'r22m6v');

INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id) 
    VALUES ('Alice', 'Smith', 'alice.smith@example.com', '+61 411 222 333', 1, 1);
INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id, parent_contact_id) 
    VALUES ('Bob', 'Jones', 'bob.jones@provider.net', '+61 422 333 444', 2, 1, 1);
INSERT INTO contact (first_name, last_name, email, phone, address_id, created_by_user_id) 
    VALUES ('Charlie', 'Brown', 'charlie.b@work.com', '+61 433 444 555', 3, 1);

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

INSERT INTO job (contact_id, address_id, council_id, job_colour_id, job_type_id, invoice_number, job_number, details, created_by_user_id) 
    VALUES (1, 1, 1, 1, 1, 'INV-2026-001', 1001, 'Emergency pipe repair at main square', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, job_type_id, invoice_number, job_number, details, created_by_user_id) 
    VALUES (2, 2, 2, 2, 1, 'INV-2026-002', 1002, 'Scheduled vegetation clearing near coastal path', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, job_type_id, invoice_number, job_number, details, created_by_user_id) 
    VALUES (3, 3, 3, 3, 1, 'INV-2026-003', 1003, 'Graffiti removal from heritage building facade', 1);
INSERT INTO job (contact_id, address_id, council_id, job_colour_id, job_type_id, invoice_number, job_number, details, created_by_user_id) 
    VALUES (1, 1, 1, 1, 1, 'INV-2026-004', 1004, 'Follow-up inspection for site drainage', 1);

INSERT INTO user_job (user_id, job_id, created_by_user_id) 
    VALUES (1, 1, 1);
INSERT INTO user_job (user_id, job_id, created_by_user_id) 
    VALUES (1, 2, 1);
INSERT INTO user_job (user_id, job_id, created_by_user_id) 
    VALUES (1, 3, 1);
INSERT INTO user_job (user_id, job_id, created_by_user_id) 
    VALUES (1, 4, 1);

INSERT INTO job_note (job_id, note, action_required, created_by_user_id) 
    VALUES (1, 'Initial site inspection complete. Waiting on parts.', FALSE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id) 
    VALUES (2, 'Client requested a callback regarding the vegetation clearing boundary.', TRUE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id) 
    VALUES (3, 'Graffiti removal started. High-pressure cleaner in use.', FALSE, 1);
INSERT INTO job_note (job_id, note, action_required, created_by_user_id) 
    VALUES (4, 'Graffiti removal started. High-pressure cleaner in use.', TRUE, 1);