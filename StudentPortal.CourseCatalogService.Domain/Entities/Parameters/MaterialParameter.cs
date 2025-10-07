namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    public class MaterialParameters : QueryStringParameters
    {
        public int? LessonId { get; set; }
        public string? Title { get; set; }
        public string? OrderBy { get; set; } = "Order";
    }
