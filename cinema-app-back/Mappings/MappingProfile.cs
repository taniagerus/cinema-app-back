using AutoMapper;
using cinema_app_back.Models;
using cinema_app_back.DTOs;

namespace cinema_app_back.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Cinema маппінги
            CreateMap<Cinema, CinemaDto>();
            CreateMap<CinemaDto, Cinema>();

            // Showtime маппінги
            CreateMap<Showtime, ShowtimeDto>();
            CreateMap<ShowtimeDto, Showtime>();

            // Hall маппінги
            CreateMap<Hall, HallDto>();
            CreateMap<HallDto, Hall>();
            CreateMap<CreateHallDto, Hall>();

            // Movie маппінги
            CreateMap<Movie, MovieDto>();
            CreateMap<MovieDto, Movie>();

            // Ticket маппінги
            CreateMap<Ticket, TicketDto>();
            CreateMap<TicketDto, Ticket>();

            // User маппінги
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            // Seat маппінги
            CreateMap<Seat, SeatDto>();
            CreateMap<SeatDto, Seat>();
        }
    }
} 