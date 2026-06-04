namespace StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

  public class LessonParameters : QueryStringParameters
    {
        public string? SearchKeyword { get; set; }
        public int? ModuleId { get; set; }
        public string? OrderBy { get; set; }
    }

