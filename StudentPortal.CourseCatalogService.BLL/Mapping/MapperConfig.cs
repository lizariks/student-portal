namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using System.Reflection;

public static class MapperConfig
{
    public static MapperConfiguration RegisterMappings()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });
    }
}