using Bogus;
using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.Domain.Entities;

namespace StudentPortal.CourseCatalogService.DAL.Data;
    public static class CourseCatalogSeedDb
    {
        public static async Task Seed(CourseCatalogDbContext db)
        {
            var now = DateTime.UtcNow;

            // Sync sequences so auto-generated IDs don't collide with rows inserted with explicit IDs
            await db.Database.ExecuteSqlRawAsync(@"
                SELECT setval(pg_get_serial_sequence('""Roles""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Roles""), 0), true);
                SELECT setval(pg_get_serial_sequence('""Users""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Users""), 0), true);
                SELECT setval(pg_get_serial_sequence('""Courses""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Courses""), 0), true);
                SELECT setval(pg_get_serial_sequence('""Modules""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Modules""), 0), true);
                SELECT setval(pg_get_serial_sequence('""Lessons""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Lessons""), 0), true);
                SELECT setval(pg_get_serial_sequence('""Materials""', 'Id'), COALESCE((SELECT MAX(""Id"") FROM ""Materials""), 0), true);
            ");

            // Roles — idempotent: add any missing role by name
            var requiredRoles = new[] { "Admin", "Student", "Teacher", "Moderator" };
            var existingRoleNames = await db.Roles.Select(r => r.Name).ToListAsync();
            var missingRoles = requiredRoles
                .Where(name => !existingRoleNames.Contains(name))
                .Select(name => new Role { Name = name })
                .ToList();
            if (missingRoles.Count > 0)
            {
                await db.Roles.AddRangeAsync(missingRoles);
                await db.SaveChangesAsync();
            }

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
                var roles = await db.Roles.ToListAsync();
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

            // Modules
            if (!await db.Modules.AnyAsync())
            {
                var module = new Module
                {
                    Id = 1,
                    Title = "Getting Started",
                    Order = 1,
                    CourseId = 1
                };
                await db.Modules.AddAsync(module);
                await db.SaveChangesAsync();
            }

            // Lessons
            if (!await db.Lessons.AnyAsync())
            {
                var lesson = new Lesson
                {
                    Id = 1,
                    Title = "Introduction to Programming Languages",
                    Order = 1,
                    ModuleId = 1
                };
                await db.Lessons.AddAsync(lesson);
                await db.SaveChangesAsync();
            }

            // Materials
            if (!await db.Materials.AnyAsync())
            {
                var material = new Material
                {
                    Id = 1,
                    Title = "Lecture Slides",
                    Url = "https://portal.com/materials/slides1.pdf",
                    LessonId = 1
                };
                await db.Materials.AddAsync(material);
                await db.SaveChangesAsync();
            }

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
