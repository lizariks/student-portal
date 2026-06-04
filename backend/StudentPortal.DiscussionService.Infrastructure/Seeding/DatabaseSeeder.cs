using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;
using StudentPortal.DiscussionService.Domain.ValueObjects;
using StudentPortal.DiscussionService.Domain.Enums;

namespace StudentPortal.DiscussionService.Infrastructure.Seeding;

public class DatabaseSeeder : IDataSeeder
{
    private readonly MongoDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(MongoDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database seeding...");
        await SeedDiscussionThreadsAsync(cancellationToken);
        _logger.LogInformation("Database seeding completed.");
    }

    private async Task SeedDiscussionThreadsAsync(CancellationToken cancellationToken)
    {
        // Matches the CS101 course seeded in CourseCatalogSeedDb (Id = 1)
        const string courseId = "1";

        var instructor = new UserInfo("2", "John Smith", UserRole.Instructor);
        var student    = new UserInfo("3", "Alice Brown", UserRole.Student);

        var seeds = new[]
        {
            new
            {
                Title   = "Welcome to CS101!",
                Creator = instructor,
                Comments = new (UserInfo author, string content)[]
                {
                    (instructor, "Welcome everyone! This is your go-to place for questions and discussion throughout the course."),
                    (student,    "Thanks! Really excited to get started — I've never written code before."),
                    (instructor, "That's exactly who this course is designed for. Ask anything, no question is too basic."),
                }
            },
            new
            {
                Title   = "Question about data types",
                Creator = student,
                Comments = new (UserInfo author, string content)[]
                {
                    (student,    "What is the difference between int and long in C#? When should I use each?"),
                    (instructor, "int is 32-bit and handles values up to ~2.1 billion. long is 64-bit and goes much higher. In practice, use int unless you are counting something truly enormous."),
                    (student,    "Got it! So for a loop counter or a user's age, int is always fine?"),
                    (instructor, "Exactly — int is the right default. You would only reach for long in cases like file sizes in bytes or astronomical distances."),
                }
            },
            new
            {
                Title   = "Week 1 assignment — stuck on loops",
                Creator = student,
                Comments = new (UserInfo author, string content)[]
                {
                    (student,    "I keep getting an infinite loop on exercise 3. My condition never becomes false. Any hints?"),
                    (instructor, "Try tracing through manually with a small starting value like 3 and write out what changes each iteration. A missing increment is the most common cause."),
                    (student,    "Oh, I see it now — I forgot to write i++ inside the loop body. Thank you!"),
                }
            },
        };

        foreach (var seed in seeds)
        {
            var exists = await _context.DiscussionThreads
                .Find(t => t.TargetId == courseId && t.Title == seed.Title)
                .AnyAsync(cancellationToken);

            if (exists)
                continue;

            var thread = new DiscussionThread(courseId, TargetType.Course, seed.Title, seed.Creator);
            foreach (var (author, content) in seed.Comments)
                thread.AddComment(new Comment(author, content));

            await _context.DiscussionThreads.InsertOneAsync(thread, cancellationToken: cancellationToken);
            _logger.LogInformation("Seeded discussion thread: {Title}", thread.Title);
        }
    }
}