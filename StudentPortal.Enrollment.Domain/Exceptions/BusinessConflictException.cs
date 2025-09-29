namespace StudentPortal.Enrollment.Domain.Exceptions;

public class BusinessConflictException:Exception
{
    public BusinessConflictException(string message) : base(message) { }
}