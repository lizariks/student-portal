-- Connect to EnrollmentDb
\c EnrollmentDb;

-- ============================================================
-- Table: Students
-- Stores student information
-- ============================================================
CREATE TABLE IF NOT EXISTS students (
                                        StudentId SERIAL PRIMARY KEY,
                                        FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL
    );

CREATE INDEX idx_student_email ON Students(Email);

-- ============================================================
-- Table: Courses
-- Stores available courses
-- ============================================================
CREATE TABLE IF NOT EXISTS courses (
                                       CourseId SERIAL PRIMARY KEY,
                                       Title VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL
    );

CREATE INDEX idx_course_title ON Courses(Title);

-- ============================================================
-- Table: Enrollments
-- Stores student course enrollments
-- ============================================================
CREATE TABLE IF NOT EXISTS enrollments (
                                           EnrollmentId SERIAL PRIMARY KEY,
                                           StudentId INT NOT NULL REFERENCES Students(StudentId) ON DELETE CASCADE,
    CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE,
    EnrolledAt TIMESTAMP NOT NULL DEFAULT NOW(),
    Status VARCHAR(50) NOT NULL,

    CONSTRAINT unique_student_course UNIQUE(StudentId, CourseId)
    );

CREATE INDEX idx_enrollment_student ON Enrollments(StudentId);
CREATE INDEX idx_enrollment_course ON Enrollments(CourseId);
CREATE INDEX idx_enrollment_status ON Enrollments(Status);

-- ============================================================
-- Table: EnrollmentStatusHistories
-- Tracks status changes for enrollments
-- ============================================================
CREATE TABLE IF NOT EXISTS enrollmentStatusHistories (
                                                         HistoryId SERIAL PRIMARY KEY,
                                                         EnrollmentId INT NOT NULL REFERENCES Enrollments(EnrollmentId) ON DELETE CASCADE,
    OldStatus VARCHAR(50) NOT NULL,
    NewStatus VARCHAR(50) NOT NULL,
    ChangedAt TIMESTAMP NOT NULL DEFAULT NOW()
    );

CREATE INDEX idx_history_enrollment ON EnrollmentStatusHistories(EnrollmentId);
CREATE INDEX idx_history_changed_at ON EnrollmentStatusHistories(ChangedAt);

-- ============================================================
-- Seed Data: Students
-- ============================================================
INSERT INTO Students (FirstName, LastName, Email) VALUES
                                                      ('Alice', 'Johnson', 'alice.johnson@student.edu'),
                                                      ('Bob', 'Smith', 'bob.smith@student.edu'),
                                                      ('Charlie', 'Brown', 'charlie.brown@student.edu')
    ON CONFLICT (Email) DO NOTHING;

-- ============================================================
-- Seed Data: Courses
-- ============================================================
INSERT INTO courses (Title, Description) VALUES
                                             ('Mathematics 101', 'Introduction to calculus and algebra'),
                                             ('Introduction to Programming', 'Learn programming fundamentals with Python'),
                                             ('Database Systems', 'Relational databases and SQL'),
                                             ('Computer Networks', 'Understanding network protocols and architecture')
    ON CONFLICT DO NOTHING;

-- ============================================================
-- Seed Data: Enrollments
-- ============================================================
INSERT INTO enrollments (StudentId, CourseId, EnrolledAt, Status) VALUES
                                                                      (1, 1, '2024-09-01', 'Active'),
                                                                      (1, 2, '2024-09-01', 'Active'),
                                                                      (2, 2, '2024-09-01', 'Completed'),
                                                                      (3, 3, '2024-09-01', 'Active')
    ON CONFLICT (StudentId, CourseId) DO NOTHING;

-- ============================================================
-- Seed Data: EnrollmentStatusHistories
-- ============================================================
INSERT INTO enrollmentStatusHistories (EnrollmentId, OldStatus, NewStatus, ChangedAt) VALUES
                                                                                          (1, 'Pending', 'Active', '2024-09-01 10:00:00'),
                                                                                          (2, 'Pending', 'Active', '2024-09-01 10:05:00'),
                                                                                          (3, 'Pending', 'Active', '2024-09-01 10:10:00'),
                                                                                          (3, 'Active', 'Completed', '2024-12-15 14:30:00'),
                                                                                          (4, 'Pending', 'Active', '2024-09-01 10:15:00')
    ON CONFLICT DO NOTHING;