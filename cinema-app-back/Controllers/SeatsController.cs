using AutoMapper;
using cinema_app_back.Data;
using cinema_app_back.DTOs;
using cinema_app_back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(DataContext context, IMapper mapper, ILogger<SeatsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/seats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetSeats()
        {
            try
            {
                _logger.LogInformation("Getting seats info");
                var seats = await _context.Seats
                    .Include(s => s.Hall)
                    .ToListAsync();

                return Ok(_mapper.Map<List<SeatDto>>(seats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seats");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/seats/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SeatDto>> GetSeat(int id)
        {
            try
            {
                _logger.LogInformation($"Getting seat info with ID: {id}");
                var seat = await _context.Seats
                    .Include(s => s.Hall)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (seat == null)
                {
                    _logger.LogWarning($"Seat with ID {id} not found");
                    return NotFound();
                }

                return _mapper.Map<SeatDto>(seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding seat with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/seats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSeat(int id, SeatDto seatDto)
        {
            try
            {
                _logger.LogInformation($"Trying to update seat with ID: {id}");

                var seat = await _context.Seats.FindAsync(id);
                if (seat == null)
                {
                    _logger.LogWarning($"Seat with ID {id} not found");
                    return NotFound();
                }

                _mapper.Map(seatDto, seat);
                _context.Update(seat);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Seat with ID {id} successfully updated");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeatExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating seat with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/seats/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            try
            {
                _logger.LogInformation($"Trying to delete seat with ID: {id}");

                var seat = await _context.Seats.FindAsync(id);
                if (seat == null)
                {
                    _logger.LogWarning($"Seat with ID {id} not found");
                    return NotFound();
                }

                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Seat with ID {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting seat with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
    }
}
