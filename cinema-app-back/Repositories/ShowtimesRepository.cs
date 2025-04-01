using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.Data;


namespace cinema_app_back.Repositories
{
    public class ShowtimesRepository : GenericRepository<Showtime>, IShowtimesRepository
    {
        private readonly DataContext _context;

        public ShowtimesRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetAllWithDetailsAsync()
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Showtime> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> HasOverlappingShowtimeAsync(DateTime startTime, DateTime endTime, int hallId, int? existingId = null)
        {
            var query = _context.Showtimes
                .Where(s => s.HallId == hallId);

            // Виключаємо поточний сеанс при перевірці перекриття
            if (existingId.HasValue)
            {
                query = query.Where(s => s.Id != existingId.Value);
            }

            return await query.AnyAsync(s =>
                ((startTime >= s.StartTime && startTime < s.EndTime) || // Новий сеанс починається під час існуючого
                 (endTime > s.StartTime && endTime <= s.EndTime) || // Новий сеанс закінчується під час існуючого
                 (startTime <= s.StartTime && endTime >= s.EndTime))); // Новий сеанс повністю охоплює існуючий
        }

        public async Task<Hall> GetHallWithCinemaAsync(int hallId)
        {
            return await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(h => h.Id == hallId);
        }
    }
}
