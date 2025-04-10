using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using cinema_app_back.DTOs;

namespace cinema_app_back.Repositories
{
    public class ShowtimesRepository : GenericRepository<Showtime>, IShowtimesRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<ShowtimesRepository> _logger;

        public ShowtimesRepository(DataContext context, ILogger<ShowtimesRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public new IQueryable<Showtime> GetAllAsync()
        {
            _logger.LogInformation("Getting showtimes as queryable");
            return _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .OrderBy(s => s.StartTime);
        }

        public async Task<IEnumerable<Showtime>> GetAllWithDetailsAsync()
        {
            _logger.LogInformation("Getting all showtimes with details");
            return await GetAllAsync().ToListAsync();
        }

        public async Task<Showtime> GetByIdWithDetailsAsync(int id)
        {
            _logger.LogInformation("Getting showtime with details by ID: {Id}", id);
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> HasOverlappingShowtimeAsync(DateTime startTime, DateTime endTime, int hallId, int? existingId = null)
        {
            _logger.LogInformation("Checking for overlapping showtimes in hall {HallId} from {StartTime} to {EndTime}", hallId, startTime, endTime);
            
            var query = _context.Showtimes
                .Where(s => s.HallId == hallId);

            // Exclude the current showtime when checking for overlap
            if (existingId.HasValue)
            {
                query = query.Where(s => s.Id != existingId.Value);
            }

            return await query.AnyAsync(s =>
                ((startTime >= s.StartTime && startTime < s.EndTime) || // New showtime starts during an existing one
                 (endTime > s.StartTime && endTime <= s.EndTime) || // New showtime ends during an existing one
                 (startTime <= s.StartTime && endTime >= s.EndTime))); // New showtime completely overlaps an existing one
        }

        public async Task<Hall> GetHallWithCinemaAsync(int hallId)
        {
            _logger.LogInformation("Getting hall with cinema by ID: {HallId}", hallId);
            return await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(h => h.Id == hallId);
        }
        
        public IQueryable<Showtime> GetByMovieAsync(int movieId)
        {
            _logger.LogInformation("Getting showtimes for movie: {MovieId}", movieId);
            return _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.StartTime);
        }
        
        public IQueryable<Showtime> GetByCinemaAsync(int cinemaId)
        {
            _logger.LogInformation("Getting showtimes for cinema: {CinemaId}", cinemaId);
            return _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .Where(s => s.CinemaId == cinemaId)
                .OrderBy(s => s.StartTime);
        }
        
        public IQueryable<Showtime> GetActiveShowtimesAsync()
        {
            var currentTime = DateTime.UtcNow;
            _logger.LogInformation("Getting active showtimes after: {Now}", currentTime);
            return _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .Where(s => s.StartTime > currentTime)
                .OrderBy(s => s.StartTime);
        }
        
        public IQueryable<Showtime> GetPastShowtimesAsync()
        {
            var currentTime = DateTime.UtcNow;
            _logger.LogInformation("Getting past showtimes before: {Now}", currentTime);
            return _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .Where(s => s.StartTime <= currentTime)
                .OrderByDescending(s => s.StartTime);
        }
        
        public new async Task<Showtime> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting showtime by ID: {Id}", id);
            return await _context.Showtimes.FindAsync(id);
        }
        
        public new async Task AddAsync(Showtime showtime)
        {
            _logger.LogInformation("Adding new showtime for movie ID: {MovieId} in hall ID: {HallId}", showtime.MovieId, showtime.HallId);
            await _context.Showtimes.AddAsync(showtime);
            await _context.SaveChangesAsync();
        }
        
        public new async Task UpdateAsync(Showtime showtime)
        {
            _logger.LogInformation("Updating showtime ID: {Id}", showtime.Id);
            _context.Showtimes.Update(showtime);
            await _context.SaveChangesAsync();
        }
        
        public new async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting showtime ID: {Id}", id);
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
            }
        }
    }
}
