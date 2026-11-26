using AutoMapper;
using Tis.Application.DTOs.Tenants;
using Tis.Domain.Entities;

namespace Tis.Application.MappingProfiles;

public class TenantProfile : Profile
{
    public TenantProfile()
    {
        // İstifadəçi sayı və dəvət göndərilməsi Tenant-da yoxdur, onları servis ayrıca doldurur.
        CreateMap<Tenant, GetTenantDto>()
            .ForMember(dest => dest.IsPlatform, opt => opt.MapFrom(src => src.Code == Tenant.PlatformCode))
            .ForMember(dest => dest.UserCount, opt => opt.Ignore())
            .ForMember(dest => dest.InviteEmailSent, opt => opt.Ignore());
    }
}
