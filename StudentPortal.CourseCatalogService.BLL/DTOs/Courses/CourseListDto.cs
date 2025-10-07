namespace StudentPortal.CourseCatalogService.BLL.DTOs.Courses;


    public class CourseListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InstructorName { get; set; } = null!;
        public int ModulesCount { get; set; }
        public int EnrollmentsCount { get; set; }
    }
