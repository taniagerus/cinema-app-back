using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_app_back;
using cinema_app_back.Models;
using cinema_app_back.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowtimesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ShowtimesController> _logger;

        public ShowtimesController(DataContext context, IMapper mapper, ILogger<ShowtimesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/showtimes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowtimeDto>>> GetShowtimes()
        {
            try 
            {
                _logger.LogInformation("Отримання списку сеансів");
                var showtimes = await _context.Showtimes
                    .Include(s => s.Movie)
                    .ToListAsync();
                    
                return _mapper.Map<List<ShowtimeDto>>(showtimes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку сеансів");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // GET: api/showtimes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShowtimeDto>> GetShowtime(int id)
        {
            try
            {
                _logger.LogInformation($"Отримання інформації про сеанс з ID: {id}");
                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound();
                }

                return _mapper.Map<ShowtimeDto>(showtime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні інформації про сеанс з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // POST: api/showtimes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShowtimeDto>> CreateShowtime(ShowtimeDto showtimeDto)
        {
            try
            {
                
                var movie = await _context.Movies.FindAsync(showtimeDto.MovieId);
                if (movie == null)
                {
                    return BadRequest($"Фільм з ID {showtimeDto.MovieId} не знайдено");
                }
                

                var showtime = _mapper.Map<Showtime>(showtimeDto);
                _context.Showtimes.Add(showtime);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetShowtime), new { id = showtime.Id }, _mapper.Map<ShowtimeDto>(showtime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка створення сеансу");
                return StatusCode(500, $"Внутрішня помилка сервера: {ex.Message}");
            }
        }

        // PUT: api/showtimes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateShowtime(int id, ShowtimeDto showtimeDto)
        {
            try
            {
                _logger.LogInformation($"Спроба оновлення сеансу з ID: {id}");
                
                var showtime = await _context.Showtimes.FindAsync(id);
                
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound();
                }

                // Оновлення властивостей сеансу
                showtime.StartTime = showtimeDto.StartTime;
                showtime.EndTime = showtimeDto.EndTime;
                showtime.MovieId = showtimeDto.MovieId;
                showtime.Price = showtimeDto.Price;

                _context.Update(showtime);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Сеанс з ID {id} успішно оновлено");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShowtimeExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка оновлення сеансу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // DELETE: api/showtimes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShowtime(int id)
        {
            try
            {
                _logger.LogInformation($"Спроба видалення сеансу з ID: {id}");
                
                var showtime = await _context.Showtimes.FindAsync(id);
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound();
                }

                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Сеанс з ID {id} успішно видалено");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка видалення сеансу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.Id == id);
        }
    }
}
