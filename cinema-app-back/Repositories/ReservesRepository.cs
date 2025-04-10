using cinema_app_back.Data;
using cinema_app_back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace cinema_app_back.Repositories
{
    public class ReservesRepository : GenericRepository<Reserve>, IReservesRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<ReservesRepository> _logger;

        public ReservesRepository(DataContext context, ILogger<ReservesRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Reserve>> GetReservesByUserIdAsync(string userId)
        {
            _logger.LogInformation("Getting reserves for user with ID: {UserId}", userId);
            return await _context.Reserves
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(r => r.Seat)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserve>> GetActiveReservesByUserIdAsync(string userId)
        {
            _logger.LogInformation("Getting active reserves for user with ID: {UserId}", userId);
            return await _context.Reserves
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(r => r.Seat)
                .Where(r => r.UserId == userId && r.IsActive && r.Showtime.StartTime > DateTime.Now)
                .OrderBy(r => r.Showtime.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserve>> GetActiveReservesByShowtimeIdAsync(int showtimeId)
        {
            _logger.LogInformation("Getting active reserves for showtime with ID: {ShowtimeId}", showtimeId);
            return await _context.Reserves
                .Include(r => r.Seat)
                .Where(r => r.ShowtimeId == showtimeId && r.IsActive)
                .ToListAsync();
        }

        public async Task<Reserve> GetReserveByIdWithDetailsAsync(int id)
        {
            _logger.LogInformation("Getting reserve with details by ID: {Id}", id);
            return await _context.Reserves
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Showtime)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(r => r.Seat)
                .Include(r => r.Payment)
                .Include(r => r.Ticket)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> IsReserveExpiredAsync(int id)
        {
            _logger.LogInformation("Checking if reserve with ID: {Id} is expired", id);
            var reserve = await _context.Reserves
                .Include(r => r.Showtime)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserve == null)
                return true;

            // Check if reservation has expired (more than 15 minutes old without payment)
            if (reserve.ReservationTime.AddMinutes(15) < DateTime.Now && !reserve.IsPaid)
            {
                _logger.LogInformation("Reserve {Id} has expired (15 minute window passed without payment)", id);
                return true;
            }

            // Check if showtime has already passed
            if (reserve.Showtime.StartTime < DateTime.Now)
            {
                _logger.LogInformation("Reserve {Id} has expired (showtime already started)", id);
                return true;
            }

            return false;
        }

        public async Task<bool> IsSeatReservedAsync(int seatId, int showtimeId)
        {
            _logger.LogInformation("Checking if seat {SeatId} is reserved for showtime {ShowtimeId}", seatId, showtimeId);
            return await _context.Reserves
                .AnyAsync(r => r.SeatId == seatId && r.ShowtimeId == showtimeId && r.IsActive);
        }

        public async Task<IEnumerable<Seat>> GetReservedSeatsByShowtimeIdAsync(int showtimeId)
        {
            _logger.LogInformation("Getting reserved seats for showtime with ID: {ShowtimeId}", showtimeId);
            return await _context.Reserves
                .Where(r => r.ShowtimeId == showtimeId && r.IsActive)
                .Select(r => r.Seat)
                .ToListAsync();
        }

        public async Task<bool> CancelReserveAsync(int id)
        {
            _logger.LogInformation("Cancelling reserve with ID: {Id}", id);
            var reserve = await _context.Reserves
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserve == null)
            {
                _logger.LogWarning("Failed to cancel reserve with ID: {Id} - reserve not found", id);
                return false;
            }

            reserve.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reserve with ID: {Id} successfully cancelled", id);
            return true;
        }
        
        public new async Task AddAsync(Reserve reserve)
        {
            _logger.LogInformation("Adding new reservation for user ID: {UserId}, seat ID: {SeatId}, showtime ID: {ShowtimeId}", 
                reserve.UserId, reserve.SeatId, reserve.ShowtimeId);
            
            await _context.Reserves.AddAsync(reserve);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully added reservation with ID: {Id}", reserve.Id);
        }
    }
} 