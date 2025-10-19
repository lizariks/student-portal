using MongoDB.Driver;
using StudentPortal.DiscussionService.Domain.Entities;

namespace StudentPortal.DiscussionService.Infrastructure.Indexes;

public class MongoIndexCreation : IIndexCreation
{
    private readonly MongoDbContext _context;

    public MongoIndexCreation(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        await CreateDiscussionThreadIndexesAsync(cancellationToken);
        await CreateCommentIndexesAsync(cancellationToken);
        await CreateCourseReviewIndexesAsync(cancellationToken);
    }

    private async Task CreateDiscussionThreadIndexesAsync(CancellationToken cancellationToken)
    {
        await _context.DiscussionThreads.Indexes.CreateOneAsync(
            new CreateIndexModel<DiscussionThread>(
                Builders<DiscussionThread>.IndexKeys
                    .Ascending(t => t.TargetId)
                    .Ascending(t => t.TargetType),
                new CreateIndexOptions { Name = "idx_target_id_type" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.DiscussionThreads.Indexes.CreateOneAsync(
            new CreateIndexModel<DiscussionThread>(
                Builders<DiscussionThread>.IndexKeys
                    .Ascending(t => t.TargetId)
                    .Ascending(t => t.TargetType)
                    .Ascending(t => t.IsClosed),
                new CreateIndexOptions { Name = "idx_target_closed" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.DiscussionThreads.Indexes.CreateOneAsync(
            new CreateIndexModel<DiscussionThread>(
                Builders<DiscussionThread>.IndexKeys.Text(t => t.Title),
                new CreateIndexOptions { Name = "idx_title_text" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.DiscussionThreads.Indexes.CreateOneAsync(
            new CreateIndexModel<DiscussionThread>(
                Builders<DiscussionThread>.IndexKeys.Ascending("createdBy.userId")
            ),
            cancellationToken: cancellationToken
        );

        await _context.DiscussionThreads.Indexes.CreateOneAsync(
            new CreateIndexModel<DiscussionThread>(
                Builders<DiscussionThread>.IndexKeys.Descending("createdAt")
            ),
            cancellationToken: cancellationToken
        );
    }

    private async Task CreateCommentIndexesAsync(CancellationToken cancellationToken)
    {
        await _context.Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Ascending("author.userId")
            ),
            cancellationToken: cancellationToken
        );

        await _context.Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Text(c => c.Content),
                new CreateIndexOptions { Name = "idx_content_text" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Ascending(c => c.ParentCommentId)
            ),
            cancellationToken: cancellationToken
        );

        await _context.Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Descending(c => c.CreatedAt)
            ),
            cancellationToken: cancellationToken
        );

        await _context.Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys
                    .Ascending(c => c.IsResolved)
                    .Descending(c => c.CreatedAt)
            ),
            cancellationToken: cancellationToken
        );
    }

    private async Task CreateCourseReviewIndexesAsync(CancellationToken cancellationToken)
    {
        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys
                    .Ascending(r => r.TargetId)
                    .Ascending(r => r.TargetType),
                new CreateIndexOptions { Name = "idx_target_id_type" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys.Ascending("reviewer.userId")
            ),
            cancellationToken: cancellationToken
        );

        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys
                    .Ascending("reviewer.userId")
                    .Ascending(r => r.TargetId)
                    .Ascending(r => r.TargetType),
                new CreateIndexOptions 
                { 
                    Name = "idx_reviewer_target_unique",
                    Unique = true 
                }
            ),
            cancellationToken: cancellationToken
        );

        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys.Ascending("rating.value")
            ),
            cancellationToken: cancellationToken
        );

        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys.Text(r => r.Comment),
                new CreateIndexOptions { Name = "idx_comment_text" }
            ),
            cancellationToken: cancellationToken
        );

        await _context.CourseReviews.Indexes.CreateOneAsync(
            new CreateIndexModel<CourseReview>(
                Builders<CourseReview>.IndexKeys.Descending("createdAt")
            ),
            cancellationToken: cancellationToken
        );
    }
}