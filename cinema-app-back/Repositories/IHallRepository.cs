using cinema_app_back.Models;

namespace cinema_app_back.Repositories
{

    public interface IHallRepository : IGenericRepository<Hall>
    {
        public  Task<Hall> GetByIdWithSeatsAsync(int id);
        public Task<IEnumerable<Hall>> GetAllWithSeatsAsync();
    }
    

}
