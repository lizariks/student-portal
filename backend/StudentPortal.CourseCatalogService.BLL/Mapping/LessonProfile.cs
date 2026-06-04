using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Lessons;

namespace StudentPortal.CourseCatalogService.BLL.Mapping
{
    public class LessonProfile : Profile
    {
        public LessonProfile()
        {
            CreateMap<Lesson, LessonDto>();

            CreateMap<Lesson, LessonDetailDto>()
                .ForMember(d => d.Materials, o => o.MapFrom(s => s.Materials));

            CreateMap<LessonCreateDto, Lesson>(MemberList.Source)
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Module, o => o.Ignore())
                .ForMember(d => d.Materials, o => o.Ignore());

            CreateMap<LessonUpdateDto, Lesson>(MemberList.Source)
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Module, o => o.Ignore())
                .ForMember(d => d.Materials, o => o.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}