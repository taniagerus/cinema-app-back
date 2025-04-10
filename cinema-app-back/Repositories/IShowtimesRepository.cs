using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cinema_app_back.Models;
using cinema_app_back.DTOs;

namespace cinema_app_back.Repositories
{
    public interface IShowtimesRepository : IGenericRepository<Showtime>
    {
        Task<IEnumerable<Showtime>> GetAllWithDetailsAsync();
        new IQueryable<Showtime> GetAllAsync();
        new Task<Showtime> GetByIdAsync(int id);
        Task<Showtime> GetByIdWithDetailsAsync(int id);
        Task<Hall> GetHallWithCinemaAsync(int hallId);
        Task<bool> HasOverlappingShowtimeAsync(DateTime startTime, DateTime endTime, int hallId, int? existingId = null);
        new Task AddAsync(Showtime showtime);
        new Task UpdateAsync(Showtime showtime);
        new Task DeleteAsync(int id);
        
        // New methods for filtering showtimes
        IQueryable<Showtime> GetByMovieAsync(int movieId);
        IQueryable<Showtime> GetByCinemaAsync(int cinemaId);
        IQueryable<Showtime> GetActiveShowtimesAsync(); // Showtimes that haven't started yet
        IQueryable<Showtime> GetPastShowtimesAsync(); // Past showtimes
    }
}
