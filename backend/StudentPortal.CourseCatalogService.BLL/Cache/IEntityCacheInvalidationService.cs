namespace StudentPortal.CourseCatalogService.BLL.Cache;

public interface IEntityCacheInvalidationService<T>
{
    Task InvalidateByIdAsync(int entityId);
    Task InvalidateAllAsync();
    Type EntityType => typeof(T);
}