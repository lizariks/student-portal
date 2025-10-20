using MongoDB.Driver;

namespace StudentPortal.DiscussionService.Domain.Common;
public class MongoSortHelper<T>
{
    public SortDefinition<T> ApplySort(string? orderByQueryString)
    {
        if (string.IsNullOrWhiteSpace(orderByQueryString))
            return Builders<T>.Sort.Ascending("_id");

        var orderParams = orderByQueryString.Trim().Split(',');
        var sortBuilder = Builders<T>.Sort.Combine(new List<SortDefinition<T>>());

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param)) continue;

            var trimmed = param.Trim();
            var isDesc = trimmed.EndsWith(" desc", StringComparison.InvariantCultureIgnoreCase);
            var fieldName = trimmed.Split(" ")[0];

            var sortDef = isDesc
                ? Builders<T>.Sort.Descending(fieldName)
                : Builders<T>.Sort.Ascending(fieldName);

            sortBuilder = Builders<T>.Sort.Combine(sortBuilder, sortDef);
        }

        return sortBuilder;
    }
}