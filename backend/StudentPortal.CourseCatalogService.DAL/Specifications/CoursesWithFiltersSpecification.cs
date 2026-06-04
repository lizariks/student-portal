namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    public class CoursesWithFiltersSpecification : Specification<Course>
    {
        public CoursesWithFiltersSpecification(CourseParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
            {
                Query.Where(c => c.Title.Contains(parameters.SearchKeyword) 
                                 || c.Code.Contains(parameters.SearchKeyword));
            }

            if (parameters.InstructorId.HasValue)
            {
                Query.Where(c => c.InstructorId == parameters.InstructorId.Value);
            }

            if (parameters.IsPublished.HasValue)
            {
                Query.Where(c => c.IsPublished == parameters.IsPublished.Value);
            }

            Query.Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .AsSplitQuery();
        }
    }
