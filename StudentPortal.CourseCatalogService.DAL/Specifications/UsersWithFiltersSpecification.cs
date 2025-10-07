namespace StudentPortal.CourseCatalogService.DAL.Specifications;

using Ardalis.Specification;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.Domain.Entities.Parameters;


    public class UsersWithFiltersSpecification : Specification<User>
    {
        public UsersWithFiltersSpecification(UserParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
            {
                Query.Where(u =>
                    u.Email.Contains(parameters.SearchKeyword) ||
                    u.Nickname.Contains(parameters.SearchKeyword) ||
                    u.FirstName.Contains(parameters.SearchKeyword) ||
                    u.LastName.Contains(parameters.SearchKeyword));
            }

            switch (parameters.OrderBy?.ToLower())
            {
                case "email":
                    Query.OrderBy(u => u.Email);
                    break;
                case "nickname":
                    Query.OrderBy(u => u.Nickname);
                    break;
                case "firstname":
                    Query.OrderBy(u => u.FirstName);
                    break;
                case "lastname":
                    Query.OrderBy(u => u.LastName);
                    break;
                default:
                    Query.OrderBy(u => u.Id);
                    break;
            }

            Query.Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }
    }
