namespace StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;
using StudentPortal.CourseCatalogService.BLL.DTOs.StudentCourses;
using System.Collections.Generic;


    public class CourseDetailsDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? InstructorId { get; set; }
        public string? InstructorName { get; set; }

        public ICollection<ModuleDto> Modules { get; set; } = new List<ModuleDto>();
        public ICollection<StudentCourseDto> Enrollments { get; set; } = new List<StudentCourseDto>();
    }
