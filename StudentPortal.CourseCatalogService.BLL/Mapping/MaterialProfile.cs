namespace StudentPortal.CourseCatalogService.BLL.Mapping;

using AutoMapper;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
using StudentPortal.CourseCatalogService.Domain.Entities;
using StudentPortal.CourseCatalogService.BLL.DTOs.Materials;


    public class MaterialProfile : Profile
    {
        public MaterialProfile()
        {
            CreateMap<Material, MaterialDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<Material, MaterialCreateDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Material, MaterialUpdateDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<MaterialCreateDto, Material>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<MaterialType>(src.Type)));

            CreateMap<MaterialUpdateDto, Material>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<MaterialType>(src.Type)));
        }
    }
