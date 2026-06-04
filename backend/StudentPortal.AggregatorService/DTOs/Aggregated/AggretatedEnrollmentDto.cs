namespace StudentPortal.AggregatorService.DTOs.Aggregated;
using StudentPortal.AggregatorService.DTOs.Enrollment;

public class AggregatedEnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; } 
    public string CurrentStatus { get; set; } = null!;
    public DateTime EnrolledAt { get; set; }

    public int CourseId { get; set; } 
    public string CourseTitle { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public int? InstructorId { get; set; }

    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    public ICollection<EnrollmentStatusHistoryDto> StatusHistory { get; set; } = new List<EnrollmentStatusHistoryDto>();
}