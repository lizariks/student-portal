namespace StudentPortal.DiscussionService.Domain.Exceptions;

public class InvalidContentException : DomainException
{
    public InvalidContentException() : base("Content cannot be empty or exceed length limits.") { }
}