using cinema_app_back.Models;

namespace cinema_app_back.Repositories
{
    public interface IReservesRepository : IGenericRepository<Reserve>
    {
        Task<IEnumerable<Reserve>> GetReservesByUserIdAsync(string userId);
        Task<IEnumerable<Reserve>> GetActiveReservesByUserIdAsync(string userId);
        Task<IEnumerable<Reserve>> GetActiveReservesByShowtimeIdAsync(int showtimeId);
        Task<Reserve> GetReserveByIdWithDetailsAsync(int id);
        Task<bool> IsReserveExpiredAsync(int id);
        Task<bool> IsSeatReservedAsync(int seatId, int showtimeId);
        Task<IEnumerable<Seat>> GetReservedSeatsByShowtimeIdAsync(int showtimeId);
        Task<bool> CancelReserveAsync(int id);
    }
} 