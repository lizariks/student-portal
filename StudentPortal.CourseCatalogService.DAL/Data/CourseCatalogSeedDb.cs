using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;

namespace StudentPortal.CourseCatalogService.DAL.Data;
    public static class 
        
        
        
        
        CourseCatalogSeedDb
    {
        public static async Task Seed(CourseCatalogDbContext db)
        {
            var now = DateTime.UtcNow;

            // Sync sequences so auto-generated IDs don't collide with rows inserted with explicit IDs
            await db.Database.ExecuteSqlRawAsync(@"
                SELECT setval(pg_get_serial_sequence('""Roles""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Roles""), 1), 1), true);
                SELECT setval(pg_get_serial_sequence('""Users""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Users""), 1), 1), true);
                SELECT setval(pg_get_serial_sequence('""Courses""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Courses""), 1), 1), true);
                SELECT setval(pg_get_serial_sequence('""Modules""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Modules""), 1), 1), true);
                SELECT setval(pg_get_serial_sequence('""Lessons""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Lessons""), 1), 1), true);
                SELECT setval(pg_get_serial_sequence('""Materials""', 'Id'), GREATEST(COALESCE((SELECT MAX(""Id"") FROM ""Materials""), 1), 1), true);
            ");

            // Roles — idempotent via ON CONFLICT DO NOTHING to survive concurrent restarts
            await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Roles"" (""Name"") VALUES ('Admin')    ON CONFLICT (""Name"") DO NOTHING;
                INSERT INTO ""Roles"" (""Name"") VALUES ('Student')  ON CONFLICT (""Name"") DO NOTHING;
                INSERT INTO ""Roles"" (""Name"") VALUES ('Teacher')  ON CONFLICT (""Name"") DO NOTHING;
                INSERT INTO ""Roles"" (""Name"") VALUES ('Moderator') ON CONFLICT (""Name"") DO NOTHING;
            ");

            // Users
            if (!await db.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Id = 1,
                        Email = "admin@portal.com",
                        PasswordHash = "hashed_password_admin",
                        Nickname = "AdminUser",
                        FirstName = "System",
                        LastName = "Admin"
                    },
                    new User
                    {
                        Id = 2,
                        Email = "teacher@portal.com",
                        PasswordHash = "hashed_password_teacher",
                        Nickname = "ProfSmith",
                        FirstName = "John",
                        LastName = "Smith"
                    },
                    new User
                    {
                        Id = 3,
                        Email = "student@portal.com",
                        PasswordHash = "hashed_password_student",
                        Nickname = "Learner1",
                        FirstName = "Alice",
                        LastName = "Brown"
                    }
                };
                await db.Users.AddRangeAsync(users);
                await db.SaveChangesAsync();
            }

            // UserRoles — look up role IDs by name to avoid hardcoded ID assumptions
            if (!await db.UserRoles.AnyAsync())
            {
                var roles = await db.Roles.AsNoTracking().ToListAsync();
                int? adminId = roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
                int? teacherId = roles.FirstOrDefault(r => r.Name == "Teacher")?.Id;
                int? studentId = roles.FirstOrDefault(r => r.Name == "Student")?.Id;

                var userRoles = new List<UserRole>();
                if (adminId.HasValue) userRoles.Add(new UserRole { UserId = 1, RoleId = adminId.Value });
                if (teacherId.HasValue) userRoles.Add(new UserRole { UserId = 2, RoleId = teacherId.Value });
                if (studentId.HasValue) userRoles.Add(new UserRole { UserId = 3, RoleId = studentId.Value });

                if (userRoles.Count > 0)
                {
                    await db.UserRoles.AddRangeAsync(userRoles);
                    await db.SaveChangesAsync();
                }
            }

            // Courses
            if (!await db.Courses.AnyAsync())
            {
                var course = new Course
                {
                    Id = 1,
                    Code = "CS101",
                    Title = "Introduction to Programming",
                    Description = "Learn the basics of programming.",
                    IsPublished = true,
                    PublishedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                    InstructorId = 2
                };
                await db.Courses.AddAsync(course);
                await db.SaveChangesAsync();
            }

            // Modules (idempotent by Title+CourseId)
            var existingModuleTitles = await db.Modules
                .Where(m => m.CourseId == 1)
                .Select(m => m.Title)
                .ToHashSetAsync();

            var moduleSeeds = new[]
            {
                new Module { Title = "Getting Started",             Description = "Install your tools and write your first program.", Order = 1, CourseId = 1 },
                new Module { Title = "Core Concepts",               Description = "Variables, control flow, and functions.",           Order = 2, CourseId = 1 },
                new Module { Title = "Object-Oriented Programming", Description = "Classes, inheritance, and polymorphism.",           Order = 3, CourseId = 1 },
            };

            foreach (var m in moduleSeeds.Where(m => !existingModuleTitles.Contains(m.Title)))
                await db.Modules.AddAsync(m);
            await db.SaveChangesAsync();

            // Reload to get real IDs
            var allModules = await db.Modules.Where(m => m.CourseId == 1).ToListAsync();
            int modGettingStarted = allModules.First(m => m.Title == "Getting Started").Id;
            int modCore           = allModules.First(m => m.Title == "Core Concepts").Id;
            int modOop            = allModules.First(m => m.Title == "Object-Oriented Programming").Id;

            // Lessons (idempotent by Title+ModuleId)
            var existingLessons = await db.Lessons
                .Where(l => allModules.Select(m => m.Id).Contains(l.ModuleId))
                .Select(l => new { l.ModuleId, l.Title })
                .ToListAsync();
            var existingLessonKeys = existingLessons.Select(l => (l.ModuleId, l.Title)).ToHashSet();

            var lessonSeeds = new[]
            {
                new Lesson { ModuleId = modGettingStarted, Title = "Introduction to Programming Languages", Content = "An overview of compiled vs interpreted languages and where C# fits in.", EstimatedDuration = TimeSpan.FromMinutes(20), Order = 1 },
                new Lesson { ModuleId = modGettingStarted, Title = "Setting Up Your Environment",           Content = "Install the .NET SDK and Visual Studio Code. Write and run your first Hello World.", EstimatedDuration = TimeSpan.FromMinutes(15), Order = 2 },
                new Lesson { ModuleId = modCore,           Title = "Variables & Data Types",               Content = "Understand int, string, bool, double and when to use each one.", EstimatedDuration = TimeSpan.FromMinutes(25), Order = 1 },
                new Lesson { ModuleId = modCore,           Title = "Control Flow: If/Else & Loops",        Content = "Make decisions with if/else and repeat actions with for and while loops.", EstimatedDuration = TimeSpan.FromMinutes(30), Order = 2 },
                new Lesson { ModuleId = modCore,           Title = "Functions & Methods",                  Content = "Break code into reusable functions. Learn parameters, return types, and scope.", EstimatedDuration = TimeSpan.FromMinutes(30), Order = 3 },
                new Lesson { ModuleId = modOop,            Title = "Classes & Objects",                    Content = "Define classes, create objects, and understand fields vs properties.", EstimatedDuration = TimeSpan.FromMinutes(35), Order = 1 },
                new Lesson { ModuleId = modOop,            Title = "Inheritance & Polymorphism",           Content = "Extend classes with inheritance and override behaviour with polymorphism.", EstimatedDuration = TimeSpan.FromMinutes(40), Order = 2 },
            };

            foreach (var l in lessonSeeds.Where(l => !existingLessonKeys.Contains((l.ModuleId, l.Title))))
                await db.Lessons.AddAsync(l);
            await db.SaveChangesAsync();

            // Reload lessons to get real IDs
            var allLessons = await db.Lessons
                .Where(l => allModules.Select(m => m.Id).Contains(l.ModuleId))
                .ToListAsync();

            int LessonId(int moduleId, string title) =>
                allLessons.First(l => l.ModuleId == moduleId && l.Title == title).Id;

            // Materials (idempotent by Title+LessonId)
            var existingMaterialKeys = (await db.Materials
                .Where(mat => allLessons.Select(l => l.Id).Contains(mat.LessonId))
                .Select(mat => new { mat.LessonId, mat.Title })
                .ToListAsync())
                .Select(m => (m.LessonId, m.Title))
                .ToHashSet();

            var materialSeeds = new[]
            {
                // Getting Started — Lesson 1
                new Material { LessonId = LessonId(modGettingStarted, "Introduction to Programming Languages"), Title = "Lecture Slides",        Url = "https://portal.com/materials/cs101-intro-slides.pdf", Type = MaterialType.File,  Order = 1 },
                new Material { LessonId = LessonId(modGettingStarted, "Introduction to Programming Languages"), Title = "Language Overview Video", Url = "https://portal.com/materials/cs101-intro-video",     Type = MaterialType.Video, Order = 2 },
                // Getting Started — Lesson 2
                new Material { LessonId = LessonId(modGettingStarted, "Setting Up Your Environment"), Title = "VS Code Download",     Url = "https://code.visualstudio.com/download",                   Type = MaterialType.Link,  Order = 1 },
                new Material { LessonId = LessonId(modGettingStarted, "Setting Up Your Environment"), Title = "Setup Guide PDF",      Url = "https://portal.com/materials/cs101-setup-guide.pdf",       Type = MaterialType.File,  Order = 2 },
                // Core Concepts — Lesson 1
                new Material { LessonId = LessonId(modCore, "Variables & Data Types"), Title = "Variables Explained",    Url = "https://portal.com/materials/cs101-variables-video",      Type = MaterialType.Video, Order = 1 },
                new Material { LessonId = LessonId(modCore, "Variables & Data Types"), Title = "Practice Exercises",     Url = "https://portal.com/materials/cs101-variables-exercises.pdf", Type = MaterialType.File, Order = 2 },
                // Core Concepts — Lesson 2
                new Material { LessonId = LessonId(modCore, "Control Flow: If/Else & Loops"), Title = "Control Flow Tutorial", Url = "https://portal.com/materials/cs101-controlflow-video",   Type = MaterialType.Video, Order = 1 },
                new Material { LessonId = LessonId(modCore, "Control Flow: If/Else & Loops"), Title = "Loop Challenge Quiz",   Url = null,                                                       Type = MaterialType.Quiz,  Order = 2 },
                // Core Concepts — Lesson 3
                new Material { LessonId = LessonId(modCore, "Functions & Methods"), Title = "Functions Deep Dive",    Url = "https://portal.com/materials/cs101-functions-video",      Type = MaterialType.Video, Order = 1 },
                new Material { LessonId = LessonId(modCore, "Functions & Methods"), Title = "Function Writing Quiz",  Url = null,                                                       Type = MaterialType.Quiz,  Order = 2 },
                // OOP — Lesson 1
                new Material { LessonId = LessonId(modOop, "Classes & Objects"), Title = "OOP Introduction",       Url = "https://portal.com/materials/cs101-oop-intro-video",      Type = MaterialType.Video, Order = 1 },
                new Material { LessonId = LessonId(modOop, "Classes & Objects"), Title = "Class Diagram Reference", Url = "https://portal.com/materials/cs101-class-diagrams",       Type = MaterialType.Link,  Order = 2 },
                // OOP — Lesson 2
                new Material { LessonId = LessonId(modOop, "Inheritance & Polymorphism"), Title = "Inheritance Tutorial", Url = "https://portal.com/materials/cs101-inheritance-video",   Type = MaterialType.Video, Order = 1 },
                new Material { LessonId = LessonId(modOop, "Inheritance & Polymorphism"), Title = "OOP Final Quiz",       Url = null,                                                       Type = MaterialType.Quiz,  Order = 2 },
            };

            foreach (var mat in materialSeeds.Where(mat => !existingMaterialKeys.Contains((mat.LessonId, mat.Title))))
                await db.Materials.AddAsync(mat);
            await db.SaveChangesAsync();

            // StudentCourse
            if (!await db.StudentCourses.AnyAsync())
            {
                await db.StudentCourses.AddAsync(new StudentCourse
                {
                    UserId = 3,
                    CourseId = 1,
                    EnrolledAt = now
                });
                await db.SaveChangesAsync();
            }
        }
    }
