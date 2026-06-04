using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Modules;

public class ModuleProfile : Profile
{
    public ModuleProfile()
    {
        CreateMap<Module, ModuleDto>();

        CreateMap<ModuleDto, Module>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Lessons, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore());

        CreateMap<ModuleCreateDto, Module>(MemberList.Source)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Lessons, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore());

        CreateMap<ModuleUpdateDto, Module>(MemberList.Source)
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CourseId, o => o.Ignore())
            .ForMember(d => d.Lessons, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}