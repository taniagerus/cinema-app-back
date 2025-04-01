using cinema_app_back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cinema_app_back.Repositories
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        Task<IEnumerable<Movie>> GetAllWithDetailsAsync();
        Task<Movie> GetByIdWithDetailsAsync(int id);
        Task<Movie> GetByTitleAsync(string title);
    }
}