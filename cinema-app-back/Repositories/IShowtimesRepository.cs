using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cinema_app_back.Models;
using cinema_app_back.DTOs;

namespace cinema_app_back.Repositories
{
    public interface IShowtimesRepository : IGenericRepository<Showtime>
    {
        Task<IEnumerable<Showtime>> GetAllWithDetailsAsync();
        Task<Showtime> GetByIdAsync(int id);
        Task<Showtime> GetByIdWithDetailsAsync(int id);
        Task<Hall> GetHallWithCinemaAsync(int hallId);
        Task<bool> HasOverlappingShowtimeAsync(DateTime startTime, DateTime endTime, int hallId, int? existingId = null);
        Task AddAsync(Showtime showtime);
        Task UpdateAsync(Showtime showtime);
        Task DeleteAsync(int id);
    }
}
