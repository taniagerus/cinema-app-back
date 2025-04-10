using cinema_app_back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cinema_app_back.Repositories
{
    public interface ICinemaRepository : IGenericRepository<Cinema>
    {
        Task<IEnumerable<Cinema>> GetAllWithDetailsAsync();
        Task<Cinema> GetByIdWithDetailsAsync(int id);
        Task<bool> CinemaExistsAsync(int id);
        Task<IEnumerable<Cinema>> GetCinemasWithHallsAsync();
    }
}
