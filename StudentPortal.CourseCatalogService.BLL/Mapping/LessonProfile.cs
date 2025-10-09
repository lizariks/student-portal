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
            CreateMap<Lesson, LessonDetailDto>();
            CreateMap<LessonCreateDto, Lesson>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Materials, opt => opt.Ignore());
            CreateMap<LessonUpdateDto, Lesson>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}