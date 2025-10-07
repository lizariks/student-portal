namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;


    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            CreateMap<Module, ModuleDto>();
            CreateMap<Module, ModuleListDto>();
            CreateMap<ModuleCreateDto, Module>();
            CreateMap<ModuleUpdateDto, Module>();
        }
    }
