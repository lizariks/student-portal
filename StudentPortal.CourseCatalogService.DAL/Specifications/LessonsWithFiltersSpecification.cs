using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

namespace StudentPortal.CourseCatalogService.DAL.Specifications
{
    public class LessonsWithFiltersSpecification : Specification<Lesson>
    {
        public LessonsWithFiltersSpecification(LessonParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
            {
                Query.Where(l =>
                    l.Title.Contains(parameters.SearchKeyword) ||
                    (l.Content != null && l.Content.Contains(parameters.SearchKeyword))
                );
            }

            if (parameters.ModuleId.HasValue)
            {
                Query.Where(l => l.ModuleId == parameters.ModuleId.Value);
            }

            Query.Include(l => l.Module)
                .Include(l => l.Materials)
                .AsSplitQuery();

            Query.Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }
    }
}