namespace StudentPortal.DiscussionService.Infrastructure.Indexes;

public interface IIndexCreation
{
    Task CreateIndexesAsync(CancellationToken cancellationToken = default);
}