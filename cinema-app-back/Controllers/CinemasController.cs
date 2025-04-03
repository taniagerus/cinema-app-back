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
    public class CinemasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CinemasController> _logger;

        public CinemasController(DataContext context, IMapper mapper, ILogger<CinemasController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/cinemas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CinemaDto>>> GetCinemas()
        {
            try
            {
                _logger.LogInformation("Getting cinemas info");
                var cinemas = await _context.Cinemas
                    .Include(c => c.Halls)
                    .ToListAsync();

                return Ok(_mapper.Map<List<CinemaDto>>(cinemas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinemas");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/cinemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CinemaDto>> GetCinema(int id)
        {
            try
            {
                _logger.LogInformation($"Getting cinema info with ID: {id}");
                var cinema = await _context.Cinemas
                    .Include(c => c.Halls)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cinema == null)
                {
                    _logger.LogWarning($"Cinema with ID {id} not found");
                    return NotFound();
                }

                return _mapper.Map<CinemaDto>(cinema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding cinema with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/cinemas
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CinemaDto>> CreateCinema(CinemaDto cinemaDto)
        {
            try
            {
                _logger.LogInformation("Creating new cinema");
                var cinema = _mapper.Map<Cinema>(cinemaDto);

                _context.Cinemas.Add(cinema);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCinema), new { id = cinema.Id }, _mapper.Map<CinemaDto>(cinema));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cinema");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/cinemas/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCinema(int id, CinemaDto cinemaDto)
        {
            try
            {
                _logger.LogInformation($"Trying to update cinema with ID: {id}");

                var cinema = await _context.Cinemas.FindAsync(id);
                if (cinema == null)
                {
                    _logger.LogWarning($"Cinema with ID {id} not found");
                    return NotFound();
                }

                _mapper.Map(cinemaDto, cinema);
                _context.Update(cinema);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cinema with ID {id} successfully updated");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CinemaExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cinema with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/cinemas/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCinema(int id)
        {
            try
            {
                _logger.LogInformation($"Trying to delete cinema with ID: {id}");

                var cinema = await _context.Cinemas.FindAsync(id);
                if (cinema == null)
                {
                    _logger.LogWarning($"Cinema with ID {id} not found");
                    return NotFound();
                }

                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cinema with ID {id} successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cinema with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool CinemaExists(int id)
        {
            return _context.Cinemas.Any(e => e.Id == id);
        }
    }
} 