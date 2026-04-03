namespace StudentPortal.CourseCatalogService.BLL.DTOs.Courses;
using System.ComponentModel.DataAnnotations;

    public class CourseCreateDto
    {
        [MaxLength(20, ErrorMessage = "Code is too long.")]
        public string Code { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        [MinLength(1, ErrorMessage = "Title cannot be empty.")]
        public string Title { get; set; } = null!;
    
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public int? InstructorId { get; set; }
    }
