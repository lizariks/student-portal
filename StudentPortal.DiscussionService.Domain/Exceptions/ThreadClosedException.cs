namespace StudentPortal.DiscussionService.Domain.Exceptions;

public class ThreadClosedException : DomainException
{
    public ThreadClosedException() : base("Cannot add messages to a closed thread.") { }
}