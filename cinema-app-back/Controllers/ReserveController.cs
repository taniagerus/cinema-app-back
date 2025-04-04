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

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReserveController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReserveController> _logger;

        public ReserveController(DataContext context, IMapper mapper, ILogger<ReserveController> logger)
        {
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
                var reserves = await _context.Reserves
                    .Include(r => r.Showtime)
                    .Include(r => r.User)
                    .Include(r => r.Seat)
                    .ToListAsync();

                return _mapper.Map<List<ReserveDto>>(reserves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of reservations");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReserveDto>> GetReserve(int id)
        {
            try
            {
                _logger.LogInformation($"Getting reservation with ID: {id}");
                var reserve = await _context.Reserves
                    .Include(r => r.Showtime)
                    .Include(r => r.User)
                    .Include(r => r.Seat)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reserve == null)
                {
                    _logger.LogWarning($"Reservation with ID {id} not found");
                    return NotFound(new { error = $"Reservation with ID {id} not found" });
                }

                return _mapper.Map<ReserveDto>(reserve);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reservation with ID: {id}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
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

                _logger.LogInformation($"Attempting to create reservation: {System.Text.Json.JsonSerializer.Serialize(createReserveDto)}");

                // Check if showtime exists and is not past
                var showtime = await _context.Showtimes
                    .FirstOrDefaultAsync(s => s.Id == createReserveDto.ShowtimeId);

                if (showtime == null)
                {
                    return BadRequest(new { error = $"Showtime with ID {createReserveDto.ShowtimeId} not found" });
                }

                if (showtime.StartTime < DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Cannot reserve seats for past showtimes" });
                }

                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == createReserveDto.UserId);

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {createReserveDto.UserId} not found");
                    return BadRequest(new { error = $"User with ID {createReserveDto.UserId} not found" });
                }

                // Check if seat exists and is available
                var seat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.Id == createReserveDto.SeatId);

                if (seat == null)
                {
                    return BadRequest(new { error = $"Seat with ID {createReserveDto.SeatId} not found" });
                }

                // Check if seat is already reserved for this showtime
                var existingReservation = await _context.Reserves
                    .FirstOrDefaultAsync(r => r.ShowtimeId == createReserveDto.ShowtimeId && 
                                            r.SeatId == createReserveDto.SeatId);

                if (existingReservation != null)
                {
                    return BadRequest(new { error = "This seat is already reserved for this showtime" });
                }

                var reserve = new Reserve
                {
                    ShowtimeId = createReserveDto.ShowtimeId,
                    UserId = createReserveDto.UserId,
                    SeatId = createReserveDto.SeatId
                };

                _context.Reserves.Add(reserve);
                await _context.SaveChangesAsync();

                // Load related entities for the response
                await _context.Entry(reserve)
                    .Reference(r => r.Showtime)
                    .LoadAsync();
                await _context.Entry(reserve)
                    .Reference(r => r.User)
                    .LoadAsync();
                await _context.Entry(reserve)
                    .Reference(r => r.Seat)
                    .LoadAsync();

                var resultDto = _mapper.Map<ReserveDto>(reserve);
                _logger.LogInformation($"Reservation successfully created with ID: {reserve.Id}");

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
                _logger.LogInformation($"Attempting to delete reservation with ID: {id}");

                var reserve = await _context.Reserves
                    .Include(r => r.Payment)
                    .Include(r => r.Ticket)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reserve == null)
                {
                    _logger.LogWarning($"Reservation with ID {id} not found");
                    return NotFound(new { error = $"Reservation with ID {id} not found" });
                }

                // Check if reservation has associated payment or ticket
                if (reserve.Payment != null || reserve.Ticket != null)
                {
                    return BadRequest(new { error = "Cannot delete reservation with associated payment or ticket" });
                }

                _context.Reserves.Remove(reserve);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Reservation with ID {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting reservation with ID: {id}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
