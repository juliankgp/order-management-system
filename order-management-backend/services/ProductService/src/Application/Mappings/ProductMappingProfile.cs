using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappings;

/// <summary>
/// Perfil de mapeo para productos
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StockMovements, opt => opt.Ignore())
            .ForMember(dest => dest.PriceHistory, opt => opt.Ignore());

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StockMovements, opt => opt.Ignore())
            .ForMember(dest => dest.PriceHistory, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // StockMovement mappings
        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => src.MovementType.ToString()));
    }
}