-- Courses table
CREATE TABLE IF NOT EXISTS Courses (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Credits INT NOT NULL
);

-- Students table
CREATE TABLE IF NOT EXISTS Students (
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL
);

-- Enrollments table
CREATE TABLE IF NOT EXISTS Enrollments (
    Id SERIAL PRIMARY KEY,
    StudentId INT NOT NULL REFERENCES Students(Id) ON DELETE CASCADE,
    CourseId INT NOT NULL REFERENCES Courses(Id) ON DELETE CASCADE,
    EnrollmentDate DATE NOT NULL
);

-- Enrollment Status History table
CREATE TABLE IF NOT EXISTS EnrollmentStatusHistories (
    Id SERIAL PRIMARY KEY,
    EnrollmentId INT NOT NULL REFERENCES Enrollments(Id) ON DELETE CASCADE,
    Status VARCHAR(50) NOT NULL,
    ChangedAt TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Seed Courses
INSERT INTO Courses (Name, Credits) VALUES
('Mathematics 101', 4),
('Introduction to Programming', 3),
('Database Systems', 3),
('Computer Networks', 3);

-- Seed Students
INSERT INTO Students (FirstName, LastName, DateOfBirth) VALUES
('Alice', 'Johnson', '2000-05-12'),
('Bob', 'Smith', '1999-11-23'),
('Charlie', 'Brown', '2001-03-15');

-- Seed Enrollments
INSERT INTO Enrollments (StudentId, CourseId, EnrollmentDate) VALUES
(1, 1, '2024-09-01'),
(1, 2, '2024-09-01'),
(2, 2, '2024-09-01'),
(3, 3, '2024-09-01');

-- Seed Enrollment Status Histories
INSERT INTO EnrollmentStatusHistories (EnrollmentId, Status) VALUES
(1, 'Active'),
(2, 'Active'),
(3, 'Completed'),
(4, 'Active');
