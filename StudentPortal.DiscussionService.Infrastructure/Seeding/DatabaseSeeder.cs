
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

    private async Task SeedDiscussionThreadsAsync(CancellationToken cancellationToken)
    {
        var threads = new List<DiscussionThread>
        {
            new DiscussionThread(Guid.NewGuid(), TargetType.Course, "Introduction to MongoDB",
                new UserInfo(Guid.NewGuid(), "Alice", UserRole.Instructor)),
            new DiscussionThread(Guid.NewGuid(), TargetType.Lesson, "Lesson 1 Discussion",
                new UserInfo(Guid.NewGuid(), "Bob", UserRole.Student))
        };

        foreach (var thread in threads)
        {
            var exists = await _context.DiscussionThreads
                .Find(t => t.TargetId == thread.TargetId)
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
            new CourseReview(Guid.NewGuid(), TargetType.Course,
                new UserInfo(Guid.NewGuid(), "Charlie", UserRole.Student), new RatingValue(5), "Great course!"),
            new CourseReview(Guid.NewGuid(), TargetType.Module, new UserInfo(Guid.NewGuid(), "Dana", UserRole.Student),
                new RatingValue(4), "Very informative.")
        };

        foreach (var review in reviews)
        {
            var exists = await _context.CourseReviews
                .Find(r => r.TargetId == review.TargetId && r.Reviewer.UserId == review.Reviewer.UserId)
                .AnyAsync(cancellationToken);

            if (!exists)
            {
                await _context.CourseReviews.InsertOneAsync(review, cancellationToken: cancellationToken);
                _logger.LogInformation("Inserted course review for TargetId: {TargetId}", review.TargetId);
            }
        }
    }

    private async Task SeedCommentsAsync(CancellationToken cancellationToken)
    {
        var comments = new List<Comment>
        {
            new Comment(new UserInfo(Guid.NewGuid(), "Eve", UserRole.Student), "This is a test comment"),
            new Comment(new UserInfo(Guid.NewGuid(), "Frank", UserRole.Instructor), "Instructor feedback")
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