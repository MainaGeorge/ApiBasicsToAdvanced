using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace CompanyEmployees.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                    opt => opt.MapFrom(x => $"{x.Address} {x.Country}"));
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CompanyForCreationDto, Company>();
            CreateMap<EmployeeForCreationDto, Employee>();
            CreateMap<EmployeeForUpdatingDto, Employee>().ReverseMap();
            CreateMap<CompanyForUpdatingDto, Company>().ReverseMap();
            CreateMap<UserForCreationDto, User>();
        }
    }
}
