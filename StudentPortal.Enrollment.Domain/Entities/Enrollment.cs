namespace StudentPortal.Enrollment.Domain;
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = null!;

        public Student? Student { get; set; }
        public Course? Course { get; set; }
        public ICollection<EnrollmentStatusHistory> StatusHistories { get; set; } = new List<EnrollmentStatusHistory>();
    }