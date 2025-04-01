using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace cinema_app_back.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        private readonly DataContext _context;

        public MovieRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllWithDetailsAsync()
        {
            return await _context.Movies
                .Include(m => m.Showtimes)
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<Movie> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Movie> GetByTitleAsync(string title)
        {
            return await _context.Movies
                .FirstOrDefaultAsync(m => m.Title == title);
        }
    }
}