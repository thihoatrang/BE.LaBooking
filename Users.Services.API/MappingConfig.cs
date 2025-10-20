using AutoMapper;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;


namespace Users.Services.API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<User, UserDTO>().ReverseMap();
        }
    }
}
