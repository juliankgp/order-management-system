using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapear entidades a DTOs
/// </summary>
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.Ignore()); // Se llenará desde el Product Service

        CreateMap<CreateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Se llenará desde el Product Service
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // Se calculará
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
