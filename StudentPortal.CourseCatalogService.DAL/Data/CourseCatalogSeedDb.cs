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

            // Roles
            if (!await db.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role { Id = 1, Name = "Admin" },
                    new Role { Id = 2, Name = "Instructor" },
                    new Role { Id = 3, Name = "Student" }
                };
                await db.Roles.AddRangeAsync(roles);
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

            // UserRoles
            if (!await db.UserRoles.AnyAsync())
            {
                await db.UserRoles.AddRangeAsync(new List<UserRole>
                {
                    new UserRole { UserId = 1, RoleId = 1 },
                    new UserRole { UserId = 2, RoleId = 2 },
                    new UserRole { UserId = 3, RoleId = 3 }
                });
                await db.SaveChangesAsync();
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
