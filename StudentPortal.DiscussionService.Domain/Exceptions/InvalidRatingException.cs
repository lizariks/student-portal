namespace StudentPortal.DiscussionService.Domain.Exceptions;

public class InvalidRatingException : DomainException
{
    public InvalidRatingException() : base("Rating must be between 1 and 5.") { }
}