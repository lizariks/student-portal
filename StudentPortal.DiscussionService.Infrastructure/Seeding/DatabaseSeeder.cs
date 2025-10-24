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
        await SeedCourseReviewsAsync(cancellationToken);
        await SeedCommentsAsync(cancellationToken);

        _logger.LogInformation("Database seeding completed.");
    }

    private string GenerateId() => Guid.NewGuid().ToString("N").Substring(0, 23);

    private async Task SeedDiscussionThreadsAsync(CancellationToken cancellationToken)
    {
        var threads = new List<DiscussionThread>
        {
            new DiscussionThread(GenerateId(), TargetType.Course, "Introduction to MongoDB",
                new UserInfo(GenerateId(), "Alice", UserRole.Instructor)),
            new DiscussionThread(GenerateId(), TargetType.Lesson, "Lesson 1 Discussion",
                new UserInfo(GenerateId(), "Bob", UserRole.Student))
        };

        foreach (var thread in threads)
        {
            var exists = await _context.DiscussionThreads
                .Find(t => t.Id == thread.Id)
                .AnyAsync(cancellationToken);

            if (!exists)
            {
                await _context.DiscussionThreads.InsertOneAsync(thread, cancellationToken: cancellationToken);
                _logger.LogInformation("Inserted discussion thread: {Title}", thread.Title);
            }
        }
    }

    private async Task SeedCourseReviewsAsync(CancellationToken cancellationToken)
    {
        var reviews = new List<CourseReview>
        {
            new CourseReview(GenerateId(), TargetType.Course,
                new UserInfo(GenerateId(), "Charlie", UserRole.Student), new RatingValue(5), "Great course!"),
            new CourseReview(GenerateId(), TargetType.Module,
                new UserInfo(GenerateId(), "Dana", UserRole.Student), new RatingValue(4), "Very informative.")
        };

        foreach (var review in reviews)
        {
            var exists = await _context.CourseReviews
                .Find(r => r.Id == review.Id)
                .AnyAsync(cancellationToken);

            if (!exists)
            {
                await _context.CourseReviews.InsertOneAsync(review, cancellationToken: cancellationToken);
                _logger.LogInformation("Inserted course review for Id: {Id}", review.Id);
            }
        }
    }

    private async Task SeedCommentsAsync(CancellationToken cancellationToken)
    {
        var comments = new List<Comment>
        {
            new Comment(new UserInfo(GenerateId(), "Eve", UserRole.Student), "This is a test comment"),
            new Comment(new UserInfo(GenerateId(), "Frank", UserRole.Instructor), "Instructor feedback")
        };

        foreach (var comment in comments)
        {
            var exists = await _context.Comments
                .Find(c => c.Id == comment.Id)
                .AnyAsync(cancellationToken);

            if (!exists)
            {
                await _context.Comments.InsertOneAsync(comment, cancellationToken: cancellationToken);
                _logger.LogInformation("Inserted comment by {Author}", comment.Author.UserName);
            }
        }
    }
}
