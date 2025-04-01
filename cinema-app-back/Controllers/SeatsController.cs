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
                return StatusCode(500, "Inner server error");
            }
        }

        // GET: api/seats/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HallDto>> GetSeat(int id)
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

                return _mapper.Map<HallDto>(seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні інформації про зал з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

       
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateHall(int id, HallDto hallDto)
        {
            try
            {
                _logger.LogInformation($"Спроба оновлення залу з ID: {id}");

                var hall = await _context.Halls.FindAsync(id);
                if (hall == null)
                {
                    _logger.LogWarning($"Зал з ID {id} не знайдено");
                    return NotFound();
                }

                var cinema = await _context.Cinemas.FindAsync(hallDto.CinemaId);
                if (cinema == null)
                {
                    return BadRequest($"Кінотеатр з ID {hallDto.CinemaId} не знайдено");
                }

                // Оновлення властивостей залу
                hall.Name = hallDto.Name;
                hall.CinemaId = hallDto.CinemaId;

                _context.Update(hall);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Зал з ID {id} успішно оновлено");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HallExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка оновлення залу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // DELETE: api/halls/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHall(int id)
        {
            try
            {
                _logger.LogInformation($"Спроба видалення залу з ID: {id}");

                var hall = await _context.Halls.FindAsync(id);
                if (hall == null)
                {
                    _logger.LogWarning($"Зал з ID {id} не знайдено");
                    return NotFound();
                }

                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Зал з ID {id} успішно видалено");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка видалення залу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.Id == id);
        }
    }
}
