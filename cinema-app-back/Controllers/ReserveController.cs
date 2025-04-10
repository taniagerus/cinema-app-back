using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using cinema_app_back.Data;
using Microsoft.Extensions.Logging;
using cinema_app_back.Repositories;
using System.Security.Claims;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReserveController : ControllerBase
    {
        private readonly IReservesRepository _reservesRepository;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReserveController> _logger;

        public ReserveController(
            IReservesRepository reservesRepository,
            IShowtimesRepository showtimesRepository,
            DataContext context, 
            IMapper mapper, 
            ILogger<ReserveController> logger)
        {
            _reservesRepository = reservesRepository;
            _showtimesRepository = showtimesRepository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/reserve
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReserveDto>>> GetReserves()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Getting reservations for user ID: {UserId}", userId);
                
                var reserves = await _reservesRepository.GetReservesByUserIdAsync(userId);
                return Ok(_mapper.Map<List<ReserveDto>>(reserves));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of reservations");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReserveDto>>> GetActiveReserves()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Getting active reservations for user ID: {UserId}", userId);
                
                var reserves = await _reservesRepository.GetActiveReservesByUserIdAsync(userId);
                return Ok(_mapper.Map<List<ReserveDto>>(reserves));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of active reservations");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReserveDto>> GetReserve(int id)
        {
            try
            {
                _logger.LogInformation("Getting reservation with ID: {Id}", id);
                var reserve = await _reservesRepository.GetReserveByIdWithDetailsAsync(id);

                if (reserve == null)
                {
                    _logger.LogWarning("Reservation with ID {Id} not found", id);
                    return NotFound(new { error = $"Reservation with ID {id} not found" });
                }

                // Check if user is authorized to view this reservation
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                
               // if (reserve.UserId != userId && !isAdmin)
               // {
               //     _logger.LogWarning("User {UserId} unauthorized to access reservation {Id}", userId, id);
               //     return Forbid();
               // }

                return _mapper.Map<ReserveDto>(reserve);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        private async Task<bool> IsSeatAvailable(int seatId, int showtimeId)
        {
            return !await _context.Reserves
                .AnyAsync(r => r.SeatId == seatId && 
                              r.ShowtimeId == showtimeId && 
                              r.IsActive == true);
        }

        // POST: api/reserve
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReserveDto>> CreateReserve([FromBody] CreateReserveDto createReserveDto)
        {
            try
            {
                if (createReserveDto == null)
                {
                    _logger.LogWarning("CreateReserveDto is null");
                    return BadRequest(new { error = "Invalid request data" });
                }

                _logger.LogInformation("Attempting to create reservation: {ReserveData}", 
                    System.Text.Json.JsonSerializer.Serialize(createReserveDto));

                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == createReserveDto.UserId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", createReserveDto.UserId);
                    return BadRequest(new { error = $"User with ID {createReserveDto.UserId} not found" });
                }

                // Check if seat exists
                var seat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.Id == createReserveDto.SeatId);

                if (seat == null)
                {
                    return BadRequest(new { error = $"Seat with ID {createReserveDto.SeatId} not found" });
                }

                var reserve = new Reserve
                {
                    ShowtimeId = createReserveDto.ShowtimeId,
                    UserId = createReserveDto.UserId,
                    SeatId = createReserveDto.SeatId,
                    IsActive = true
                };

                try
                {
                    await _reservesRepository.AddAsync(reserve);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Seat is already reserved") == true)
                {
                    return BadRequest(new { error = "This seat is already reserved for this showtime" });
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("showtime has already started") == true)
                {
                    return BadRequest(new { error = "Cannot reserve seats for past showtimes" });
                }

                // Get the complete reserve with details for the response
                var createdReserve = await _reservesRepository.GetReserveByIdWithDetailsAsync(reserve.Id);
                var resultDto = _mapper.Map<ReserveDto>(createdReserve);
                
                _logger.LogInformation("Reservation successfully created with ID: {Id}", reserve.Id);

                return CreatedAtAction(nameof(GetReserve), new { id = reserve.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // DELETE: api/reserve/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReserve(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to cancel reservation with ID: {Id}", id);

                var reserve = await _reservesRepository.GetReserveByIdWithDetailsAsync(id);

                if (reserve == null)
                {
                    _logger.LogWarning("Reservation with ID {Id} not found", id);
                    return NotFound(new { error = $"Reservation with ID {id} not found" });
                }

                // Check if user is authorized to cancel this reservation
               //// var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //var isAdmin = User.IsInRole("Admin");
                
//                if (reserve.UserId != userId && !isAdmin)
//                {
  //                  _logger.LogWarning("User {UserId} unauthorized to cancel reservation {Id}", userId, id);
    //                return Forbid();
      //          }

                // Check if reservation has expired
                var isExpired = await _reservesRepository.IsReserveExpiredAsync(id);
                if (isExpired)
                {
                    return BadRequest(new { error = "Cannot cancel an expired reservation" });
                }

                // Check if reservation already has a payment or ticket
                if (reserve.Payment != null || reserve.Ticket != null)
                {
                    return BadRequest(new { error = "Cannot cancel a reservation with associated payment or ticket" });
                }

                var success = await _reservesRepository.CancelReserveAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to cancel reservation" });
                }

                _logger.LogInformation("Reservation with ID {Id} successfully cancelled", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation with ID: {Id}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
        
        [HttpGet("showtime/{showtimeId}/seats")]
        public async Task<ActionResult<IEnumerable<int>>> GetReservedSeatsForShowtime(int showtimeId)
        {
            try
            {
                _logger.LogInformation("Getting reserved seats for showtime ID: {ShowtimeId}", showtimeId);
                
                var seats = await _context.Reserves
                    .Where(r => r.ShowtimeId == showtimeId && r.IsActive == true)
                    .Include(r => r.Seat)
                    .Select(r => r.Seat)
                    .ToListAsync();

                return Ok(seats.Select(s => s.Id).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reserved seats for showtime ID: {ShowtimeId}", showtimeId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("user/{userId}/future")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReserveDto>>> GetFutureReserves(string userId)
        {
            try
            {
                // Check if user is authorized to view these reservations
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (currentUserId != userId && !isAdmin)
                {
                    _logger.LogWarning("User {CurrentUserId} unauthorized to access reservations for user {RequestedUserId}", 
                        currentUserId, userId);
                    return Forbid();
                }

                _logger.LogInformation("Getting future reservations for user ID: {UserId}", userId);

                var reserves = await _context.Reserves
                    .Include(r => r.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Include(r => r.Showtime)
                        .ThenInclude(s => s.Hall)
                    .Include(r => r.Seat)
                    .Include(r => r.Payment)
                    .Include(r => r.Ticket)
                    .Where(r => r.UserId == userId &&
                           r.IsActive == true &&
                           r.Showtime.StartTime > DateTime.UtcNow &&
                           r.Payment != null &&
                           r.Ticket != null)
                    .OrderBy(r => r.Showtime.StartTime)
                    .ToListAsync();

                var reserveDtos = _mapper.Map<List<ReserveDto>>(reserves);

                // Add additional information needed by the frontend
                foreach (var dto in reserveDtos)
                {
                    if (dto.Showtime?.Movie != null)
                    {
                        // Ensure the image path is properly set
                        dto.Showtime.Movie.Image = !string.IsNullOrEmpty(dto.Showtime.Movie.Image) 
                            ? dto.Showtime.Movie.Image 
                            : "/placeholder-movie.jpg";
                    }
                }

                return Ok(reserveDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting future reservations for user ID: {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
