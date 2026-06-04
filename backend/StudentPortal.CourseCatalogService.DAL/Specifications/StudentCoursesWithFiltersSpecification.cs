namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    public class StudentCoursesWithFiltersSpecification : Specification<StudentCourse>
    {
        public StudentCoursesWithFiltersSpecification(StudentCourseParameters parameters)
        {
            if (parameters.UserId.HasValue)
                Query.Where(sc => sc.UserId == parameters.UserId.Value);

            if (parameters.CourseId.HasValue)
                Query.Where(sc => sc.CourseId == parameters.CourseId.Value);

            if (parameters.EnrolledAfter.HasValue)
                Query.Where(sc => sc.EnrolledAt >= parameters.EnrolledAfter.Value);

            if (parameters.EnrolledBefore.HasValue)
                Query.Where(sc => sc.EnrolledAt <= parameters.EnrolledBefore.Value);

            Query.Include(sc => sc.Course)
                .Include(sc => sc.User)
                .AsSplitQuery();

            switch (parameters.OrderBy?.ToLower())
            {
                case "userid":
                    Query.OrderBy(sc => sc.UserId);
                    break;
                case "courseid":
                    Query.OrderBy(sc => sc.CourseId);
                    break;
                case "enrolledat":
                default:
                    Query.OrderBy(sc => sc.EnrolledAt);
                    break;
            }

            Query.Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }
    }