-- Tier 1: Tables with FKs pointing to other tables (Junction/Child tables)
drop table if exists dashboard_item;
drop table if exists dashboard_content;
drop table if exists dashboard;
DROP TABLE IF EXISTS schedule;
DROP TABLE IF EXISTS schedule_user;
DROP TABLE IF EXISTS timesheet_entry;
DROP TABLE IF EXISTS schedule_track;
DROP TABLE IF EXISTS quote_note;
DROP TABLE IF EXISTS council_contact;
DROP TABLE IF EXISTS job_quote;
DROP TABLE IF EXISTS job_note;
DROP TABLE IF EXISTS job_task;
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
DROP TABLE IF EXISTS schedule_colour;
DROP TABLE IF EXISTS file_type;
DROP TABLE IF EXISTS job_task_type;
DROP TABLE IF EXISTS states;
DROP TABLE IF EXISTS app_user;
DROP TABLE IF EXISTS application_setting;

-- ============================================================================
-- EXTENSIONS
-- ============================================================================
CREATE EXTENSION IF NOT EXISTS pg_trgm;
-- CREATE EXTENSION IF NOT EXISTS postgis;

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
    state_id INT REFERENCES states(id),
    post_code VARCHAR(10) NOT NULL,
    country VARCHAR(100) NOT NULL,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL,
    geohash VARCHAR(12),
    search_vector tsvector GENERATED ALWAYS AS (
            setweight(to_tsvector('english', coalesce(street, '')), 'A') ||
            setweight(to_tsvector('english', coalesce(suburb, '')), 'B')
        ) STORED NOT NULL
);

COMMENT ON TABLE address IS 'Physical addresses for contacts and jobs';
COMMENT ON COLUMN address.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_address_state_id ON address(state_id);
CREATE INDEX idx_address_deleted_at ON address(deleted_at);
CREATE INDEX idx_address_created_by ON address(created_by_user_id);
-- Add trigram indexes for text search on address fields
CREATE INDEX idx_address_street_trgm ON address USING GIN(street gin_trgm_ops);
CREATE INDEX idx_address_suburb_trgm ON address USING GIN(suburb gin_trgm_ops);
CREATE INDEX idx_address_geo_hash ON address(geohash);
CREATE INDEX idx_address_search_vector ON address USING GIN(search_vector);
-- Optional: Combined trigram index if users often search across both fields
CREATE INDEX idx_address_street_suburb_trgm ON address USING GIN((street || ' ' || suburb) gin_trgm_ops);

-- ============================================================================
-- CONTACT TABLE
-- ============================================================================

CREATE TABLE contact (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    parent_contact_id INT REFERENCES contact(id),
    legacy_id INT,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    full_name VARCHAR(201) NOT NULL GENERATED ALWAYS AS (first_name || ' ' || last_name) STORED,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    fax VARCHAR(50),
    address_id INT REFERENCES address(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL,
    search_vector tsvector GENERATED ALWAYS AS (
            setweight(to_tsvector('english', coalesce(first_name, '')), 'A') ||
            setweight(to_tsvector('english', coalesce(last_name, '')), 'B') ||
            setweight(to_tsvector('english', coalesce(email, '')), 'C')
        ) STORED NOT NULL
);

COMMENT ON TABLE contact IS 'Client or vendor contact information';
COMMENT ON COLUMN contact.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';

CREATE INDEX idx_contact_email ON contact(email);
CREATE INDEX idx_contact_parent_contact_id ON contact(parent_contact_id);
CREATE INDEX idx_contact_address_id ON contact(address_id);
CREATE INDEX idx_contact_deleted_at ON contact(deleted_at);
CREATE INDEX idx_contact_created_by ON contact(created_by_user_id);
CREATE INDEX idx_contact_name_trgm ON contact USING GIN(full_name gin_trgm_ops);
CREATE INDEX idx_contact_email_trgm ON contact USING GIN(email gin_trgm_ops);
CREATE INDEX idx_contact_phone_trgm ON contact USING GIN(phone gin_trgm_ops);
CREATE INDEX idx_contact_search_vector ON contact USING GIN(search_vector);

-- ============================================================================
-- COUNCIL TABLE
-- ============================================================================

CREATE TABLE council(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    address_id int references address(id),
    legacy_id INT,
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(50) NULL,
    fax VARCHAR(50)  NULL,
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
    legacy_id INT,
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
-- JOB COLOUR TABLE
-- ============================================================================

CREATE TABLE job_colour (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    color VARCHAR(20) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT job_color_format CHECK (color IS NULL OR color LIKE '#%'),
    CONSTRAINT job_color_unique UNIQUE (color)
);

COMMENT ON TABLE job_colour IS 'Colour of job';

-- ============================================================================
-- JOB TYPE TABLE
-- ============================================================================

CREATE TABLE job_type (
    id INT PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    abbreviation VARCHAR(15) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE job_type IS 'Type of job';
COMMENT ON COLUMN job_type.name IS 'Construction = Set out. Survey = CAD.';

INSERT INTO job_type(id, name, abbreviation) VALUES (1, 'Construction', 'CONSTRUCTION');
INSERT INTO job_type(id, name, abbreviation) VALUES (2, 'Survey', 'Survey');

-- ============================================================================
-- JOB TABLE
-- ============================================================================

CREATE TABLE job (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    contact_id INT NOT NULL REFERENCES contact(id),
    address_id INT REFERENCES address(id),
    council_id INT REFERENCES council(id),
    job_colour_id INT REFERENCES job_colour(id),
    job_type_id INT NOT NULL REFERENCES job_type(id),
    legacy_id INT,
    invoice_number VARCHAR(255) UNIQUE,
    job_number INT,
    details TEXT,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE job IS 'Main job/project tracking with invoicing';
COMMENT ON COLUMN job.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means active';
COMMENT ON COLUMN job.job_number IS 'Used in junction with the job type to identify the job. With either be type Construction or Surveying';

CREATE INDEX idx_job_number ON job(job_number);
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
    legacy_id INT,
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
-- USER JOB NOTES (One-to-Many)
-- ============================================================================
CREATE TABLE job_note (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    assigned_user_id INT  REFERENCES app_user(id),
    job_id INT NOT NULL REFERENCES job(id),
    legacy_id INT,
    note TEXT NOT NULL,
    action_required BOOLEAN NOT NULL DEFAULT FALSE,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

COMMENT ON TABLE job_note IS 'Many-to-many relationship between users and jobs';
COMMENT ON COLUMN job_note.deleted_at IS 'Soft delete TIMESTAMPTZ - NULL means to show';

CREATE INDEX idx_job_note_user_id ON job_note(assigned_user_id);
CREATE INDEX idx_job_note_job_id ON job_note(job_id);
CREATE INDEX idx_job_note_deleted_at ON job_note(deleted_at);
CREATE INDEX idx_job_note_created_by ON job_note(created_by_user_id);

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
-- JOB TASK TABLE (Many-to-One)
-- ============================================================================
CREATE TABLE job_task_type(
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

CREATE INDEX idx_job_task_type_name ON job_task_type(name);
CREATE INDEX idx_job_task_type_created_by ON job_task_type(created_by_user_id);

-- ============================================================================
-- JOB TASK TABLE (Many-to-One)
-- ============================================================================

CREATE TABLE job_task(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    job_id INT NOT NULL REFERENCES job(id),
    legacy_id INT,
    description TEXT,
    quoted_price DECIMAL(10, 2),
    invoice_required bool not null default false,
    active_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    invoiced_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_by_user_id INT REFERENCES app_user(id),
    modified_on TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ DEFAULT NULL
);

CREATE INDEX idx_task_job_id ON job_task(job_id);
CREATE INDEX idx_task_deleted_at ON job_task(deleted_at);
CREATE INDEX idx_task_created_by ON job_task(created_by_user_id);

-- ============================================================================
-- QUOTE TABLE
-- ============================================================================

CREATE TABLE quote (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    legacy_id INT,
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
    legacy_id INT,
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
    legacy_id INT,
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

CREATE TABLE schedule_user(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    schedule_track_id INT NOT NULL REFERENCES schedule_track(id),
    user_id INT NOT NULL REFERENCES app_user(id),
    created_by_user_id INT NOT NULL REFERENCES app_user(id),
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_schedule_users_schedule_track_id ON schedule_user(schedule_track_id);
CREATE INDEX idx_schedule_users_user_id ON schedule_user(user_id);
CREATE INDEX idx_schedule_users_created_by_id ON schedule_user(created_by_user_id);

-- ============================================================================
-- SCHEDULE COLOUR TABLE
-- ============================================================================

CREATE TABLE schedule_colour (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    color VARCHAR(20) NOT NULL,
    description VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT schedule_colour_format CHECK (color IS NULL OR color LIKE '#%')
);

COMMENT ON TABLE schedule_colour IS 'Colour of the schedule';

-- ============================================================================
-- SCHEDULE TABLE
-- ============================================================================

CREATE TABLE schedule(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    schedule_track_id INT NOT NULL REFERENCES schedule_track(id),
    schedule_colour_id INT NOT NULL REFERENCES schedule_colour(id),
    legacy_id INT,
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
CREATE INDEX idx_schedule_track_id ON schedule(schedule_track_id);
CREATE INDEX idx_schedule_colour_id ON schedule(schedule_colour_id);
CREATE INDEX idx_schedule_start_time ON schedule(start_time);
CREATE INDEX idx_schedule_end_time ON schedule(end_time);
CREATE INDEX idx_schedule_created_by ON schedule(created_by_user_id);

-- ============================================================================
-- APPLICATION SETTINGS TABLE
-- ============================================================================
CREATE TABLE application_setting(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    key VARCHAR(255) NOT NULL,
    value JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT application_settings_key_unique UNIQUE (key)
);

-- ============================================================================
-- USER DASHBOARD TABLE
-- ============================================================================
CREATE TABLE dashboard(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    user_id INT NOT NULL REFERENCES app_user(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL DEFAULT 'Dashboard',
    is_default BOOLEAN DEFAULT FALSE,
    dashboard_y INT NOT NULL DEFAULT 12,
    dashboard_x INT NOT NULL DEFAULT 12,
    created_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_on TIMESTAMPTZ
);

-- Index for fast loading of a specific user's dashboards
CREATE INDEX idx_user_dashboard_user ON dashboard(user_id);

-- ============================================================================
-- DASHBOARD ITEM CONTENT
-- ============================================================================
CREATE TABLE dashboard_content(
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(50) NOT NULL -- Removed trailing comma
);

COMMENT ON TABLE dashboard_content IS 'Holds widgets defined in the front end.';

-- ============================================================================
-- DASHBOARD ITEM TABLE
-- ============================================================================
CREATE TABLE dashboard_item (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    dashboard_id INT NOT NULL REFERENCES dashboard(id) ON DELETE CASCADE,
    content_id INT NOT NULL REFERENCES dashboard_content(id),
    -- Layout
    position_x INT NOT NULL,
    position_y INT NOT NULL,
    colspan INT NOT NULL,
    rowspan INT NOT NULL,
    -- Customization
    custom_title VARCHAR(100),
    settings JSONB DEFAULT '{}'::jsonb,
    is_hidden BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_dashboard_item_dashboard_id ON dashboard_item(dashboard_id);
CREATE INDEX idx_dashboard_item_content ON dashboard_item(content_id);
