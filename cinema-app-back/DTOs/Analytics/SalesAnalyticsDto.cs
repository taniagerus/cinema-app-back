using System;
using System.Collections.Generic;

namespace cinema_app_back.DTOs
{
    public class SalesAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TicketsSold { get; set; }
        public MovieRevenueDto? MostPopularMovie { get; set; }
        public ShowtimeAnalyticsDto? MostPopularShowtime { get; set; }
        public List<DailySalesDto> LastWeekSales { get; set; } = new List<DailySalesDto>();
        public List<MovieRevenueDto> MovieRevenue { get; set; } = new List<MovieRevenueDto>();
        
        // Додаткові властивості для розширеної аналітики
        public decimal RevenueChange { get; set; } // Зміна прибутку у відсотках
        public DailySalesDto? PeakDay { get; set; } // Найприбутковіший день
    }

    public class MovieRevenueDto
    {
        public string Title { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int TicketsSold { get; set; }
        public string? Image { get; set; }
    }

    public class ShowtimeAnalyticsDto
    {
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
    }

    public class DailySalesDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Tickets { get; set; }
    }
} 