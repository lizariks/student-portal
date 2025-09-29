namespace StudentPortal.Enrollment.Domain;

public class Student
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
