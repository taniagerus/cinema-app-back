using System;

namespace cinema_app_back.DTOs
{
    public class MovieStatsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageTicketPrice { get; set; }
        public int ShowtimeCount { get; set; }
    }
} 