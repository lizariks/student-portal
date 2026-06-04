namespace StudentPortal.DiscussionService.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}