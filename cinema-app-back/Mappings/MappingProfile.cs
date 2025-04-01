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
            CreateMap<HallDto, Hall>();
            CreateMap<Hall, HallDto>();
            CreateMap<Movie, MovieDto>();
            CreateMap<MovieDto, Movie>();
            CreateMap<Ticket, TicketDto>();
            CreateMap<TicketDto, Ticket>();
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<Seat, SeatDto>();
            CreateMap<SeatDto, Seat>();

            // Можна додати інші маппінги за потреби
        }
    }
} 