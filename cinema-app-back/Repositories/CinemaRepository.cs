using cinema_app_back.Data;
using cinema_app_back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cinema_app_back.Repositories
{
    public class CinemaRepository : GenericRepository<Cinema>, ICinemaRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<CinemaRepository> _logger;

        public CinemaRepository(DataContext context, ILogger<CinemaRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Cinema>> GetAllWithDetailsAsync()
        {
            _logger.LogInformation("Getting all cinemas with details");
            return await _context.Cinemas
                .Include(c => c.Halls)
                .ToListAsync();
        }

        public async Task<Cinema> GetByIdWithDetailsAsync(int id)
        {
            _logger.LogInformation("Getting cinema with details by ID: {Id}", id);
            return await _context.Cinemas
                .Include(c => c.Halls)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CinemaExistsAsync(int id)
        {
            _logger.LogInformation("Checking if cinema exists with ID: {Id}", id);
            return await _context.Cinemas.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Cinema>> GetCinemasWithHallsAsync()
        {
            _logger.LogInformation("Getting cinemas with halls");
            return await _context.Cinemas
                .Include(c => c.Halls)
                .Where(c => c.Halls.Any())
                .ToListAsync();
        }
    }
} 