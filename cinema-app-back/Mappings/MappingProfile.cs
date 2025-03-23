using AutoMapper;
using cinema_app_back.Models;
using cinema_app_back.DTOs;

namespace cinema_app_back.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Showtime маппінги
            CreateMap<Showtime, ShowtimeDto>();
            CreateMap<ShowtimeDto, Showtime>();
            
            // Можна додати інші маппінги за потреби
        }
    }
} 