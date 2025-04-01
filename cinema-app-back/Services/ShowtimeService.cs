using cinema_app_back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Data;

namespace cinema_app_back.Services
{
    public class ShowtimeService
    {
        private readonly DataContext _context;

        public ShowtimeService(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesAsync()
        {
            return await _context.Showtimes.ToListAsync();
        }
    }
}
