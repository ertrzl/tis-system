using AutoMapper;
using Tis.Application.DTOs.Auth;
using Tis.Application.DTOs.Users;
using Tis.Domain.Entities;

namespace Tis.Application.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Rol enum-dan mətnə çevrilir. Universitetin adı User-də yoxdur, onu servis ayrıca doldurur.
        CreateMap<User, GetCurrentUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.TenantName, opt => opt.Ignore());

        // Parol təyin olunmayıbsa (boş hash), istifadəçi hələ dəvəti qəbul etməyib.
        CreateMap<User, GetUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.InvitationPending, opt => opt.MapFrom(src => src.PasswordHash == string.Empty));
    }
}
