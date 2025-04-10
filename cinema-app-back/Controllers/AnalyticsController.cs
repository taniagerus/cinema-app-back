using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using cinema_app_back.Data;
using cinema_app_back.Models;
using cinema_app_back.Models.Enums;
using cinema_app_back.DTOs;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(DataContext context, ILogger<AnalyticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesData([FromQuery] string period = "week")
        {
            try
            {
                _logger.LogInformation($"Fetching sales analytics for period: {period}");

                DateTime startDate;
                DateTime endDate = DateTime.UtcNow;

                // Визначаємо період для аналізу
                switch (period.ToLower())
                {
                    case "week":
                        startDate = endDate.AddDays(-7);
                        break;
                    case "month":
                        startDate = endDate.AddMonths(-1);
                        break;
                    case "year":
                        startDate = endDate.AddYears(-1);
                        break;
                    default:
                        startDate = endDate.AddDays(-7); // Default to week
                        break;
                }

                // Отримуємо всі оплачені квитки за період
                var tickets = await _context.Tickets
                    .Include(t => t.Payment)
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                            .ThenInclude(s => s.Movie)
                    .Where(t => t.Status == TicketStatus.Paid)
                    .Where(t => t.Payment.PaymentDate >= startDate && t.Payment.PaymentDate <= endDate)
                    .ToListAsync();

                if (!tickets.Any())
                {
                    return Ok(new SalesAnalyticsDto
                    {
                        TotalRevenue = 0,
                        TicketsSold = 0,
                        MostPopularMovie = null,
                        MostPopularShowtime = null,
                        LastWeekSales = GenerateEmptyDateRange(startDate, endDate),
                        MovieRevenue = new List<MovieRevenueDto>(),
                        RevenueChange = 0,
                        PeakDay = null
                    });
                }

                // Обчислюємо загальні показники
                decimal totalRevenue = tickets.Sum(t => t.Payment.Amount);
                int ticketsSold = tickets.Count;

                // Групуємо продажі за фільмами
                var movieRevenue = tickets
                    .GroupBy(t => new { t.Reserve.Showtime.Movie.Id, t.Reserve.Showtime.Movie.Title, t.Reserve.Showtime.Movie.Image })
                    .Select(g => {
                        // Знаходимо дату першого сеансу для фільму (як ReleaseDate)
                        var movieId = g.Key.Id;
                        var imageUrl = !string.IsNullOrEmpty(g.Key.Image) && g.Key.Image.StartsWith("/")
                            ? g.Key.Image
                            : "/images/movies/placeholder.jpg";
                        
                        return new MovieRevenueDto
                        {
                            Title = g.Key.Title,
                            Revenue = g.Sum(t => t.Payment.Amount),
                            TicketsSold = g.Count(),
                            Image = imageUrl
                        };
                    })
                    .OrderByDescending(m => m.Revenue)
                    .ToList();

                // Знаходимо найпопулярніший фільм
                var mostPopularMovie = movieRevenue.FirstOrDefault();

                // Групуємо продажі за сеансами
                var showtimeSales = tickets
                    .GroupBy(t => new { 
                        t.Reserve.Showtime.Id, 
                        t.Reserve.Showtime.StartTime, 
                        MovieTitle = t.Reserve.Showtime.Movie.Title 
                    })
                    .Select(g => new
                    {
                        ShowtimeId = g.Key.Id,
                        Date = g.Key.StartTime.Date,
                        Time = g.Key.StartTime.ToString("HH:mm"),
                        g.Key.MovieTitle,
                        TicketsSold = g.Count()
                    })
                    .OrderByDescending(s => s.TicketsSold)
                    .ToList();

                // Знаходимо найпопулярніший сеанс
                var mostPopularShowtime = showtimeSales.FirstOrDefault();
                ShowtimeAnalyticsDto popularShowtime = null;
                
                if (mostPopularShowtime != null)
                {
                    popularShowtime = new ShowtimeAnalyticsDto
                    {
                        Date = mostPopularShowtime.Date.ToString("yyyy-MM-dd"),
                        Time = mostPopularShowtime.Time,
                        MovieTitle = mostPopularShowtime.MovieTitle,
                        TicketsSold = mostPopularShowtime.TicketsSold
                    };
                }

                // Отримуємо продажі по днях
                var dailySales = tickets
                    .GroupBy(t => t.Payment.PaymentDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Revenue = g.Sum(t => t.Payment.Amount),
                        Tickets = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Заповнюємо пропущені дні
                var result = new List<DailySalesDto>();
                var currentDate = startDate.Date;
                while (currentDate <= endDate.Date)
                {
                    var dateStr = currentDate.ToString("yyyy-MM-dd");
                    var existingSales = dailySales.FirstOrDefault(d => d.Date == dateStr);
                    
                    if (existingSales != null)
                    {
                        result.Add(existingSales);
                    }
                    else
                    {
                        result.Add(new DailySalesDto
                        {
                            Date = dateStr,
                            Revenue = 0,
                            Tickets = 0
                        });
                    }
                    
                    currentDate = currentDate.AddDays(1);
                }

                // Розрахунок тренду прибутку 
                decimal revenueChange = 0;
                if (result.Count > 0 && result[0].Revenue > 0)
                {
                    var firstDayRevenue = result[0].Revenue;
                    var lastDayRevenue = result[result.Count - 1].Revenue;
                    revenueChange = (lastDayRevenue - firstDayRevenue) / firstDayRevenue * 100;
                }

                // Знаходимо найприбутковіший день
                var peakDay = result.OrderByDescending(d => d.Revenue).FirstOrDefault();

                // Формуємо відповідь
                var response = new SalesAnalyticsDto
                {
                    TotalRevenue = totalRevenue,
                    TicketsSold = ticketsSold,
                    MostPopularMovie = mostPopularMovie,
                    MostPopularShowtime = popularShowtime,
                    LastWeekSales = result,
                    MovieRevenue = movieRevenue,
                    RevenueChange = revenueChange,
                    PeakDay = peakDay
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales analytics");
                return StatusCode(500, new { error = "Failed to retrieve sales analytics", details = ex.Message });
            }
        }

        [HttpGet("movies")]
        public async Task<IActionResult> GetMovieStats([FromQuery] string period = "all")
        {
            try
            {
                _logger.LogInformation($"Fetching movie statistics for period: {period}");

                DateTime? startDate = null;
                DateTime endDate = DateTime.UtcNow;

                // Визначаємо період для аналізу
                switch (period.ToLower())
                {
                    case "week":
                        startDate = endDate.AddDays(-7);
                        break;
                    case "month":
                        startDate = endDate.AddMonths(-1);
                        break;
                    case "year":
                        startDate = endDate.AddYears(-1);
                        break;
                    case "all":
                    default:
                        // Не обмежуємо дату
                        break;
                }

                // Базовий запит для отримання квитків
                IQueryable<Ticket> ticketsQuery = _context.Tickets
                    .Include(t => t.Payment)
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                            .ThenInclude(s => s.Movie)
                    .Where(t => t.Status == TicketStatus.Paid);

                // Додаємо фільтр за датою, якщо потрібно
                if (startDate.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.Payment.PaymentDate >= startDate.Value && t.Payment.PaymentDate <= endDate);
                }

                // Отримуємо квитки
                var tickets = await ticketsQuery.ToListAsync();

                if (!tickets.Any())
                {
                    return Ok(new List<MovieStatsDto>());
                }

                // Групуємо квитки за фільмами і обчислюємо статистику
                var movieStats = tickets
                    .GroupBy(t => new { 
                        t.Reserve.Showtime.Movie.Id, 
                        t.Reserve.Showtime.Movie.Title,
                        t.Reserve.Showtime.Movie.Image
                    })
                    .Select(g => {
                        // Знаходимо дату першого сеансу для фільму
                        var releaseDate = _context.Showtimes
                            .Where(s => s.MovieId == g.Key.Id)
                            .OrderBy(s => s.StartTime)
                            .Select(s => s.StartTime)
                            .FirstOrDefault();
                            
                        return new MovieStatsDto
                        {
                            Id = g.Key.Id,
                            Title = g.Key.Title,
                            Image = g.Key.Image,
                            ReleaseDate = releaseDate,
                            TicketsSold = g.Count(),
                            Revenue = g.Sum(t => t.Payment.Amount),
                            AverageTicketPrice = g.Count() > 0 ? g.Sum(t => t.Payment.Amount) / g.Count() : 0,
                            ShowtimeCount = g.Select(t => t.Reserve.ShowtimeId).Distinct().Count()
                        };
                    })
                    .OrderByDescending(m => m.TicketsSold)
                    .ToList();

                return Ok(movieStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie statistics");
                return StatusCode(500, new { error = "Failed to retrieve movie statistics", details = ex.Message });
            }
        }
        
        [HttpGet("days")]
        public async Task<IActionResult> GetDailyStats([FromQuery] int days = 30)
        {
            try
            {
                _logger.LogInformation($"Fetching daily statistics for last {days} days");

                if (days <= 0 || days > 365)
                {
                    return BadRequest(new { error = "Days parameter must be between 1 and 365" });
                }

                DateTime startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
                DateTime endDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1); // End of today

                // Отримуємо всі оплачені квитки за вказаний період
                var tickets = await _context.Tickets
                    .Include(t => t.Payment)
                    .Where(t => t.Status == TicketStatus.Paid)
                    .Where(t => t.Payment.PaymentDate >= startDate && t.Payment.PaymentDate <= endDate)
                    .ToListAsync();

                // Групуємо квитки за днями
                var dailyStats = tickets
                    .GroupBy(t => t.Payment.PaymentDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Revenue = g.Sum(t => t.Payment.Amount),
                        Tickets = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Заповнюємо пропущені дні
                var result = new List<DailySalesDto>();
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    var dateStr = currentDate.ToString("yyyy-MM-dd");
                    var existingSales = dailyStats.FirstOrDefault(d => d.Date == dateStr);
                    
                    if (existingSales != null)
                    {
                        result.Add(existingSales);
                    }
                    else
                    {
                        result.Add(new DailySalesDto
                        {
                            Date = dateStr,
                            Revenue = 0,
                            Tickets = 0
                        });
                    }
                    
                    currentDate = currentDate.AddDays(1);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily statistics");
                return StatusCode(500, new { error = "Failed to retrieve daily statistics", details = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryStats()
        {
            try
            {
                _logger.LogInformation($"Fetching summary statistics for the cinema");

                // Загальна кількість фільмів
                var totalMovies = await _context.Movies.CountAsync();
                
                // Загальна кількість сеансів
                var totalShowtimes = await _context.Showtimes.CountAsync();
                
                // Загальна кількість кінозалів
                var totalHalls = await _context.Halls.CountAsync();
                
                // Загальна кількість місць
                var totalSeats = await _context.Seats.CountAsync();
                
                // Загальна кількість проданих квитків
                var totalTickets = await _context.Tickets
                    .Where(t => t.Status == TicketStatus.Paid)
                    .CountAsync();
                
                // Загальний дохід
                var totalRevenue = await _context.Tickets
                    .Where(t => t.Status == TicketStatus.Paid)
                    .Include(t => t.Payment)
                    .SumAsync(t => t.Payment.Amount);
                
                // Кількість користувачів
                var totalUsers = await _context.Users.CountAsync();
                
                // Найпопулярніший фільм (за кількістю проданих квитків)
                var mostPopularMovie = await _context.Tickets
                    .Where(t => t.Status == TicketStatus.Paid)
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                            .ThenInclude(s => s.Movie)
                    .GroupBy(t => new { t.Reserve.Showtime.Movie.Id, t.Reserve.Showtime.Movie.Title, t.Reserve.Showtime.Movie.Image })
                    .Select(g => new MovieRevenueDto
                    {
                        Title = g.Key.Title,
                        Revenue = g.Sum(t => t.Payment.Amount),
                        TicketsSold = g.Count(),
                        Image = g.Key.Image
                    })
                    .OrderByDescending(m => m.TicketsSold)
                    .FirstOrDefaultAsync();
                
                return Ok(new
                {
                    TotalMovies = totalMovies,
                    TotalShowtimes = totalShowtimes,
                    TotalHalls = totalHalls,
                    TotalSeats = totalSeats,
                    TotalTickets = totalTickets,
                    TotalRevenue = totalRevenue,
                    TotalUsers = totalUsers,
                    MostPopularMovie = mostPopularMovie
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary statistics");
                return StatusCode(500, new { error = "Failed to retrieve summary statistics", details = ex.Message });
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] string period = "week")
        {
            try
            {
                _logger.LogInformation($"Fetching performance metrics for period: {period}");

                DateTime startDate;
                DateTime endDate = DateTime.UtcNow;

                // Визначаємо період для аналізу
                switch (period.ToLower())
                {
                    case "week":
                        startDate = endDate.AddDays(-7);
                        break;
                    case "month":
                        startDate = endDate.AddMonths(-1);
                        break;
                    case "year":
                        startDate = endDate.AddYears(-1);
                        break;
                    default:
                        startDate = endDate.AddDays(-7); // Default to week
                        break;
                }

                // Отримуємо всі оплачені квитки за вказаний період
                var tickets = await _context.Tickets
                    .Include(t => t.Payment)
                    .Where(t => t.Status == TicketStatus.Paid)
                    .Where(t => t.Payment.PaymentDate >= startDate && t.Payment.PaymentDate <= endDate)
                    .ToListAsync();

                if (!tickets.Any())
                {
                    return Ok(new
                    {
                        RevenueTrend = 0,
                        TotalRevenue = 0,
                        TotalTickets = 0,
                        AverageTicketPrice = 0,
                        AverageDailyRevenue = 0,
                        BestDay = new { Date = (string)null, Revenue = 0, Tickets = 0 },
                        DailyData = GenerateEmptyDateRange(startDate, endDate)
                    });
                }

                // Групуємо продажі за днями
                var dailySales = tickets
                    .GroupBy(t => t.Payment.PaymentDate.Date)
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Revenue = g.Sum(t => t.Payment.Amount),
                        Tickets = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Заповнюємо пропущені дні
                var result = new List<DailySalesDto>();
                var currentDate = startDate.Date;
                while (currentDate <= endDate.Date)
                {
                    var dateStr = currentDate.ToString("yyyy-MM-dd");
                    var existingSales = dailySales.FirstOrDefault(d => d.Date == dateStr);
                    
                    if (existingSales != null)
                    {
                        result.Add(existingSales);
                    }
                    else
                    {
                        result.Add(new DailySalesDto
                        {
                            Date = dateStr,
                            Revenue = 0,
                            Tickets = 0
                        });
                    }
                    
                    currentDate = currentDate.AddDays(1);
                }

                // Обчислюємо показники ефективності
                var totalRevenue = result.Sum(d => d.Revenue);
                var totalTickets = result.Sum(d => d.Tickets);
                var avgTicketPrice = totalTickets > 0 ? totalRevenue / totalTickets : 0;
                var avgDailyRevenue = result.Count > 0 ? totalRevenue / result.Count : 0;
                
                // Розрахунок тренду прибутку
                decimal revenueTrend = 0;
                if (result.Count > 0 && result[0].Revenue > 0)
                {
                    var firstDayRevenue = result[0].Revenue;
                    var lastDayRevenue = result[result.Count - 1].Revenue;
                    revenueTrend = (lastDayRevenue - firstDayRevenue) / firstDayRevenue * 100;
                }
                
                // Знаходимо найприбутковіший день
                var bestDay = result.OrderByDescending(d => d.Revenue).FirstOrDefault();

                return Ok(new
                {
                    RevenueTrend = revenueTrend,
                    TotalRevenue = totalRevenue,
                    TotalTickets = totalTickets,
                    AverageTicketPrice = avgTicketPrice,
                    AverageDailyRevenue = avgDailyRevenue,
                    BestDay = bestDay,
                    DailyData = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return StatusCode(500, new { error = "Failed to retrieve performance metrics", details = ex.Message });
            }
        }

        // Helper method to generate empty date range
        private List<DailySalesDto> GenerateEmptyDateRange(DateTime startDate, DateTime endDate)
        {
            var result = new List<DailySalesDto>();
            var currentDate = startDate.Date;
            
            while (currentDate <= endDate.Date)
            {
                result.Add(new DailySalesDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    Revenue = 0,
                    Tickets = 0
                });
                
                currentDate = currentDate.AddDays(1);
            }
            
            return result;
        }
    }
} 