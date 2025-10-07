namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;

    public class ModulesWithFiltersSpecification : Specification<Module>
    {
        public ModulesWithFiltersSpecification(ModuleParameters parameters)
        {
            if (parameters.CourseId.HasValue)
                Query.Where(m => m.CourseId == parameters.CourseId.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Title))
                Query.Where(m => m.Title.Contains(parameters.Title));

            switch (parameters.OrderBy?.ToLower())
            {
                case "title":
                    Query.OrderBy(m => m.Title);
                    break;
                case "courseid":
                    Query.OrderBy(m => m.CourseId);
                    break;
                case "order":
                default:
                    Query.OrderBy(m => m.Order);
                    break;
            }

            Query.Include(m => m.Lessons)
                .AsSplitQuery();

            Query.Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }
    }
