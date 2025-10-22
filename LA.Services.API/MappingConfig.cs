using AutoMapper;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;

namespace LA.Services.API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<LawyerProfile, LawyerProfileDTO>()
                .ForMember(dest => dest.PracticeAreas, opt => opt.MapFrom(src => src.LawyerPracticeAreas.Select(lpa => lpa.PracticeArea)))
                .ReverseMap()
                .ForMember(dest => dest.LawyerPracticeAreas, opt => opt.Ignore());
            CreateMap<LawyerDiploma, LawyerDiplomaDTO>().ReverseMap();
            CreateMap<LawyerDiplomaCreateDto, LawyerDiploma>();
            CreateMap<LawyerDiplomaUpdateDto, LawyerDiploma>();
            CreateMap<WorkSlot, WorkSlotDto>().ReverseMap();
            CreateMap<WorkSlot, CreateWorkSlotDto>().ReverseMap();
            CreateMap<WorkSlot, UpdateWorkSlotDto>().ReverseMap();
            CreateMap<UpdateWorkSlotDtoNoId, WorkSlot>();
            
            // Practice Area mappings
            CreateMap<PracticeArea, PracticeAreaDTO>().ReverseMap();
            CreateMap<PracticeAreaCreateDTO, PracticeArea>();
            CreateMap<PracticeAreaUpdateDTO, PracticeArea>();
            
            // Service mappings
            CreateMap<Service, ServiceDTO>()
                .ForMember(dest => dest.PracticeArea, opt => opt.MapFrom(src => src.PracticeArea))
                .ReverseMap()
                .ForMember(dest => dest.PracticeArea, opt => opt.Ignore());
            CreateMap<ServiceCreateDTO, Service>();
            CreateMap<ServiceUpdateDTO, Service>();
            
            // Lawyer Practice Area mappings
            CreateMap<LawyerPracticeArea, LawyerPracticeAreaDTO>().ReverseMap();
        }
    }
}
