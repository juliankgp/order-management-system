using AutoMapper;
using CustomerService.Application.DTOs;
using CustomerService.Domain.Entities;

namespace CustomerService.Application.Mappings;

/// <summary>
/// Perfil de mapeo para entidades de clientes
/// </summary>
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<Customer, CustomerSummaryDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<Customer, AuthenticatedCustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.TokenExpires, opt => opt.Ignore());

        CreateMap<RegisterCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Addresses, opt => opt.Ignore());

        CreateMap<UpdateCustomerProfileDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.EmailVerified, opt => opt.Ignore())
            .ForMember(dest => dest.EmailVerifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailVerificationToken, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.InternalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Addresses, opt => opt.Ignore());

        CreateMap<CustomerAddress, CustomerAddressDto>();

        CreateMap<CreateUpdateAddressDto, CustomerAddress>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}