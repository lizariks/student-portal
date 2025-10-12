namespace StudentPortal.DiscussionService.Domain.Exceptions;

public class UnauthorizedActionException : DomainException
{
    public UnauthorizedActionException() : base("User is not authorized to perform this action.") { }
}