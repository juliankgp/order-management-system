using AutoMapper;
using LoggingService.Application.Commands.CreateLog;
using LoggingService.Application.DTOs;
using LoggingService.Domain.Entities;

namespace LoggingService.Application.Mappings;

public class LogEntryMappingProfile : Profile
{
    public LogEntryMappingProfile()
    {
        CreateMap<LogEntry, LogEntryDto>();
        CreateMap<CreateLogEntryDto, LogEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow))
            .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.MachineName ?? Environment.MachineName))
            .ForMember(dest => dest.Environment, opt => opt.MapFrom(src => src.Environment ?? "Development"));

        CreateMap<CreateLogCommand, LogEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow))
            .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.MachineName ?? Environment.MachineName))
            .ForMember(dest => dest.Environment, opt => opt.MapFrom(src => src.Environment ?? "Development"));
    }
}