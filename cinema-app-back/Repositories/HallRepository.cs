using cinema_app_back.Data;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;

namespace cinema_app_back.Repositories
{
    public class HallRepository: GenericRepository<Hall>, IHallRepository
    {
        private readonly DataContext _context;

        public HallRepository(DataContext context) : base(context)
        {
            _context = context;
        }
       public async Task<Hall> GetByIdWithSeatsAsync(int id)
        {
            var hall = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == id);
            return hall;
        }
        public async Task<IEnumerable<Hall>> GetAllWithSeatsAsync()
        {
            var halls = await _context.Halls
                .Include(h => h.Seats)
                .ToListAsync();
            return halls;
        }
    }
}
