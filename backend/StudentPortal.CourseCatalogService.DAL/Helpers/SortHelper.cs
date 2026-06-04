namespace StudentPortal.CourseCatalogService.DAL.Helpers;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;


    public class SortHelper<T> : ISortHelper<T>
    {
        public IQueryable<T> ApplySort(IQueryable<T> entities, string orderByQueryString)
        {
            if (!entities.Any() || string.IsNullOrWhiteSpace(orderByQueryString))
                return entities;

            var orderParams = orderByQueryString.Trim().Split(',');
            var propertyInfos = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var orderQueryBuilder = "";

            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param)) continue;

                var propertyFromQueryName = param.Split(' ')[0];
                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objectProperty == null) continue;

                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                orderQueryBuilder += $"{objectProperty.Name} {direction}, ";
            }

            var orderQuery = orderQueryBuilder.TrimEnd(',', ' ');
            return string.IsNullOrWhiteSpace(orderQuery) ? entities : entities.OrderBy(orderQuery);
        }
    }
