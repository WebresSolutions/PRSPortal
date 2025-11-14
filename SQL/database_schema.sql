-- Tier 1: Tables with FKs pointing to other tables (Junction/Child tables)
DROP TABLE IF EXISTS schedule;
DROP TABLE IF EXISTS schedule_users;
DROP TABLE IF EXISTS schedule_track;
DROP TABLE IF EXISTS timesheet_entry;
DROP TABLE IF EXISTS quote_note;
DROP TABLE IF EXISTS council_contact;
DROP TABLE IF EXISTS job_quote;
DROP TABLE IF EXISTS job_note;
DROP TABLE IF EXISTS job_file;
DROP TABLE IF EXISTS user_job;

-- Tier 2: Tables with FKs pointing to core entities (Parents/Reference tables)
DROP TABLE IF EXISTS job;
DROP TABLE IF EXISTS quote;
DROP TABLE IF EXISTS contact;
DROP TABLE IF EXISTS app_file;
DROP TABLE IF EXISTS council;
DROP TABLE IF EXISTS address;

-- Tier 3: Lookup/Type tables
DROP TABLE IF EXISTS job_type;
DROP TABLE IF EXISTS job_colour;
DROP TABLE IF EXISTS note_type;
DROP TABLE IF EXISTS file_type;
DROP TABLE IF EXISTS states;
DROP TABLE IF EXISTS app_user;

-- ============================================================================
-- APP USER TABLE
-- ============================================================================

CREATE TABLE app_user (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    identity_id VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMPTZ DEFAULT NULL,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deactivated_at TIMESTAMPTZ DEFAULT NULL,
    legacy_user_id INT NOT NULL
);

COMMENT ON TABLE app_user IS 'Application users with authentication and profile information';
COMMENT ON COLUMN app_user.deactivated_at IS 'NULL = active user, TIMESTAMPTZ = deactivated user';

CREATE INDEX idx_app_user_email ON app_user(email);
CREATE INDEX idx_app_user_identity_id ON app_user(identity_id);
CREATE INDEX idx_app_user_deactivated_at ON app_user(deactivated_at);

-- ============================================================================
-- STATE TABLE
-- ============================================================================

CREATE TABLE states (
    id INT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    abbreviation VARCHAR(3) NOT NULL UNIQUE
);

INSERT INTO states(id, name, abbreviation) VALUES (1, 'New South Wales', 'NSW');
INSERT INTO states(id, name, abbreviation) VALUES (2, 'Queensland', 'QLD');
INSERT INTO states(id, name, abbreviation) VALUES (3, 'Victoria', 'VIC');
INSERT INTO states(id, name, abbreviation) VALUES (4, 'South Australia', 'SA');
INSERT INTO states(id, name, abbreviation) VALUES (5, 'Tasmania', 'TAS');
INSERT INTO states(id, name, abbreviation) VALUES (6, 'Western Australia', 'WA');
INSERT INTO states(id, name, abbreviation) VALUES (7, 'Northern Territory', 'NT');

COMMENT ON TABLE states IS 'States or territories for address management';

-- ============================================================================
-- ADDRESS TABLE
-- ============================================================================

CREATE TABLE address (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    street VARCHAR(255) NOT NULL,
    suburb VARCHAR(100) NOT NULL,
    state_id INT NOT NULL REFERENCES states(id),
    post_code VARCHAR(10) NOT NULL,
    country VARCHAR(100) NOT NULL,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE address IS 'Physical addresses for contacts and jobs';
COMMENT ON COLUMN address.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_address_state_id ON address(state_id);
CREATE INDEX idx_address_deleted_at ON address(deleted_at);
CREATE INDEX idx_address_created_by ON address(created_by_user_id);

-- ============================================================================
-- CONTACT TABLE
-- ============================================================================

CREATE TABLE contact (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    fax VARCHAR(20),
    address_id INT REFERENCES address(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE contact IS 'Client or vendor contact information';
COMMENT ON COLUMN contact.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_contact_email ON contact(email);
CREATE INDEX idx_contact_address_id ON contact(address_id);
CREATE INDEX idx_contact_deleted_at ON contact(deleted_at);
CREATE INDEX idx_contact_created_by ON contact(created_by_user_id);

-- ============================================================================
-- COUNCIL TABLE
-- ============================================================================

CREATE TABLE council(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    address_id int references address(id),
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NULL,
    fax VARCHAR(20)  NULL,
    email VARCHAR(255) NULL,
    website VARCHAR(255),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE council IS 'Council information';
Create index idx_council_address_id on council(address_id);

-- ============================================================================
-- COUNCIL CONTACT TABLE
-- ============================================================================

CREATE TABLE council_contact(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    council_id INT NOT NULL REFERENCES council(id),
    contact_id INT NOT NULL REFERENCES contact(id),
    address_id INT NOT NULL REFERENCES address(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ
);

create index idx_council_conact_council_id on council(id);
create index idx_council_contact_contact_id on contact(id);
create index idx_council_contact_address_id on address(id);

-- ============================================================================
-- FILE TYPE TABLE
-- ============================================================================

CREATE TABLE file_type(
    id INT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE file_type IS 'File type and metadata';

-- ============================================================================
-- FILE TABLE
-- ============================================================================

CREATE TABLE app_file (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    file_type_id INT NOT NULL REFERENCES file_type(id),
    filename VARCHAR(255) NOT NULL,
    title VARCHAR(255),
    description TEXT,
    external_id VARCHAR(255) UNIQUE,
    file_hash VARCHAR(64) NOT NULL,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT NOT NULL REFERENCES app_user(id),
    modified_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE app_file IS 'File metadata and storage references';
COMMENT ON COLUMN app_file.file_hash IS 'SHA-256 hash for duplicate detection';
COMMENT ON COLUMN app_file.external_id IS 'Reference to external storage system (S3, etc)';
COMMENT ON COLUMN app_file.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_file_hash ON app_file(file_hash);
CREATE INDEX idx_file_type ON app_file(file_type_id);
CREATE INDEX idx_file_created_by ON app_file(created_by_user_id);
CREATE INDEX idx_file_deleted_at ON app_file(deleted_at);
CREATE INDEX idx_file_external_id ON app_file(external_id);

-- ============================================================================
-- NOTE TYPE TABLE
-- ============================================================================

CREATE TABLE note_type (
    id INT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE note_type IS 'Type of note';

-- ============================================================================
-- JOB COLOUR TABLE
-- ============================================================================

CREATE TABLE job_colour (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    color VARCHAR(20) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE job_colour IS 'Colour of job';

-- ============================================================================
-- JOB TYPE TABLE
-- ============================================================================

CREATE TABLE job_type (
    id INT PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    abbreviation VARCHAR(10) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE job_type IS 'Type of job';

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Set Out', 'SET');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Cadastral', 'CAD');

-- ============================================================================
-- JOB TABLE
-- ============================================================================

CREATE TABLE job (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    contact_id INT REFERENCES contact(id),
    address_id INT REFERENCES address(id),
    council_id INT REFERENCES council(id),
    job_colour_id INT REFERENCES job_colour(id),
    job_type_id INT REFERENCES job_type(id),
    invoice_number VARCHAR(255) UNIQUE,
    cad_job_number VARCHAR(25),
    set_out_job_number VARCHAR(25),
    total_price NUMERIC(10, 2),
    date_sent TIMESTAMPTZ,
    payment_received TIMESTAMPTZ,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE job IS 'Main job/project tracking with invoicing';
COMMENT ON COLUMN job.total_price IS 'Total job price - consider calculating from timesheet entries';
COMMENT ON COLUMN job.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_job_invoice_number ON job(invoice_number);
CREATE INDEX idx_job_contact_id ON job(contact_id);
CREATE INDEX idx_job_address_id ON job(address_id);
CREATE INDEX idx_job_council_id ON job(council_id);
CREATE INDEX idx_job_colour_id ON job(job_colour_id);
CREATE INDEX idx_job_type_id ON job(job_type_id);
CREATE INDEX idx_job_created_on ON job(created_on);
CREATE INDEX idx_job_deleted_at ON job(deleted_at);
CREATE INDEX idx_job_created_by ON job(created_by_user_id);

-- ============================================================================
-- USER JOB TABLE (Many-to-Many)
-- ============================================================================

CREATE TABLE user_job (
    user_id INT NOT NULL REFERENCES app_user(id),
    job_id INT NOT NULL REFERENCES job(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL,
    PRIMARY KEY (user_id, job_id)
);

COMMENT ON TABLE user_job IS 'Many-to-many relationship between users and jobs';
COMMENT ON COLUMN user_job.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active assignment';

CREATE INDEX idx_user_job_user_id ON user_job(user_id);
CREATE INDEX idx_user_job_job_id ON user_job(job_id);
CREATE INDEX idx_user_job_deleted_at ON user_job(deleted_at);

-- ============================================================================
-- JOB FILE TABLE (Many-to-Many)
-- ============================================================================

CREATE TABLE job_file (
    job_id INT NOT NULL REFERENCES job(id),
    file_id INT NOT NULL REFERENCES app_file(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (job_id, file_id)
);

COMMENT ON TABLE job_file IS 'Files attached to specific jobs';

CREATE INDEX idx_job_file_job_id ON job_file(job_id);
CREATE INDEX idx_job_file_file_id ON job_file(file_id);
CREATE INDEX idx_job_file_created_by ON job_file(created_by_user_id);

-- ============================================================================
-- JOB NOTE TABLE (Many-to-One)
-- ============================================================================

CREATE TABLE job_note (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    job_id INT NOT NULL REFERENCES job(id),
    content TEXT NOT NULL, -- Note content is now here
    -- note_type_id is removed as the table title implies the type
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE job_note IS 'Notes specifically attached to a job (One-to-Many: Job to Note).';

CREATE INDEX idx_job_note_job_id ON job_note(job_id);
CREATE INDEX idx_job_note_created_by ON job_note(created_by_user_id);
CREATE INDEX idx_job_note_deleted_at ON job_note(deleted_at);

-- ============================================================================
-- QUOTE TABLE
-- ============================================================================

CREATE TABLE quote (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    address_id INT REFERENCES address(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE quote IS 'A quote for a job';
CREATE INDEX idx_quote_address_id ON quote(address_id);
CREATE INDEX idx_quote_created_by ON quote(created_by_user_id);
CREATE INDEX idx_quote_modified_by ON quote(modified_by_user_id);

-- ============================================================================
-- JOB QUOTES TABLE
-- ============================================================================

CREATE TABLE job_quote (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    job_id INT NOT NULL REFERENCES job(id),
    quote_id INT NOT NULL REFERENCES quote(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE job_quote IS 'Links quotes to jobs';
CREATE INDEX idx_job_quote_job_id ON job_quote(job_id);
CREATE INDEX idx_job_quote_quote_id ON job_quote(quote_id);
CREATE INDEX idx_job_quote_created_by ON job_quote(created_by_user_id);

-- ============================================================================
-- QUOTE NOTES TABLE
-- ============================================================================

CREATE TABLE quote_note (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    quote_id INT NOT NULL REFERENCES quote(id),
    content TEXT NOT NULL,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE quote_note IS 'Notes specifically attached to a quote (One-to-Many: Quote to Note).';

CREATE INDEX idx_quote_note_quote_id ON quote_note(quote_id);
CREATE INDEX idx_quote_note_created_by ON quote_note(created_by_user_id);
CREATE INDEX idx_quote_note_deleted_at ON quote_note(deleted_at);

-- ============================================================================
-- TIMESHEET ENTRY TABLE
-- ============================================================================

CREATE TABLE timesheet_entry (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    job_id INT REFERENCES job(id),
    user_id INT NOT NULL REFERENCES app_user(id),
    description TEXT,
    date_from TIMESTAMPTZ NOT NULL,
    date_to TIMESTAMPTZ,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    CONSTRAINT check_timesheet_dates CHECK (date_to IS NULL OR date_to >= date_from)
);

COMMENT ON TABLE timesheet_entry IS 'Time tracking entries for jobs and their associated note content.';
COMMENT ON COLUMN timesheet_entry.date_to IS 'NULL indicates ongoing/in-progress entry';

CREATE INDEX idx_timesheet_job_id ON timesheet_entry(job_id);
CREATE INDEX idx_timesheet_user_id ON timesheet_entry(user_id);
CREATE INDEX idx_timesheet_date_from ON timesheet_entry(date_from);
CREATE INDEX idx_timesheet_date_to ON timesheet_entry(date_to);
CREATE INDEX idx_timesheet_created_by ON timesheet_entry(created_by_user_id);

-- ============================================================================
-- SCHEDULE TRACK TABLE
-- ============================================================================

CREATE TABLE schedule_track(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    job_type_id INT NOT NULL REFERENCES job_type(id),
    date DATE,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

CREATE INDEX idx_schedule_track_job_type_id ON schedule_track(job_type_id);
CREATE INDEX idx_schedule_track_created_by_user_id ON schedule_track(created_by_user_id);

-- ============================================================================
-- SCHEDULE USERS TABLE
-- ============================================================================

CREATE TABLE schedule_users(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    schedule_track_id INT NOT NULL REFERENCES schedule_track(id),
    user_id INT NOT NULL REFERENCES app_user(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_schedule_users_schedule_track_id ON schedule_users(schedule_track_id);
CREATE INDEX idx_schedule_users_user_id ON schedule_users(user_id);
CREATE INDEX idx_schedule_users_created_by_id ON schedule_users(created_by_user_id);

-- ============================================================================
-- SCHEDULE TABLE
-- ============================================================================

CREATE TABLE schedule(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    job_id INT REFERENCES job(id),
    notes TEXT,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    CONSTRAINT check_schedule_times CHECK (end_time >= start_time)
);

COMMENT ON TABLE schedule IS 'User schedules for work hours.';
COMMENT ON COLUMN schedule.start_time IS 'Start time of the schedule';
COMMENT ON COLUMN schedule.end_time IS 'End time of the schedule';

CREATE INDEX idx_schedule_job_id ON schedule(job_id);
CREATE INDEX idx_schedule_start_time ON schedule(start_time);
CREATE INDEX idx_schedule_end_time ON schedule(end_time);
CREATE INDEX idx_schedule_created_by ON schedule(created_by_user_id);
