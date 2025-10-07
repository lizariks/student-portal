namespace StudentPortal.CourseCatalogService.DAL.Specifications;
using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


public class MaterialsWithFilterSpecification: Specification<Material>
{
    
        public MaterialsWithFilterSpecification(MaterialParameters parameters)
        {
            if (parameters.LessonId.HasValue)
                Query.Where(m => m.LessonId == parameters.LessonId.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Title))
                Query.Where(m => m.Title.Contains(parameters.Title));

            switch (parameters.OrderBy?.ToLower())
            {
                case "title":
                    Query.OrderBy(m => m.Title);
                    break;
                case "type":
                    Query.OrderBy(m => m.Type);
                    break;
                case "order":
                default:
                    Query.OrderBy(m => m.Order);
                    break;
            }

            Query.Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }
    }