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
using cinema_app_back.Repositories;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowtimesController : ControllerBase
    {
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ShowtimesController> _logger;
        private readonly IMovieRepository _movieRepository;
        private readonly ICinemaRepository _cinemaRepository;

        public ShowtimesController(
            IShowtimesRepository showtimesRepository,
            IMapper mapper,
            ILogger<ShowtimesController> logger,
            IMovieRepository movieRepository,
            ICinemaRepository cinemaRepository)
        {
            _showtimesRepository = showtimesRepository;
            _mapper = mapper;
            _logger = logger;
            _movieRepository = movieRepository;
            _cinemaRepository = cinemaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowtimeDto>>> GetShowtimes([FromQuery] bool includePast = false)
        {
            try
            {
                IEnumerable<Showtime> showtimes;
                if (includePast)
                {
                    showtimes = await _showtimesRepository.GetAllWithDetailsAsync();
                }
                else
                {
                    showtimes = await _showtimesRepository.GetActiveShowtimesAsync().ToListAsync();
                }

                _logger.LogInformation($"Retrieved {showtimes.Count()} showtimes with includePast={includePast}");
                return Ok(_mapper.Map<IEnumerable<ShowtimeDto>>(showtimes));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving showtimes: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShowtimeDto>> GetShowtime(int id)
        {
            try
            {
                _logger.LogInformation($"Отримання інформації про сеанс з ID: {id}");
                var showtime = await _showtimesRepository.GetByIdWithDetailsAsync(id);
                    
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound(new { error = $"Сеанс з ID {id} не знайдено" });
                }

                var showtimeDto = _mapper.Map<ShowtimeDto>(showtime);

                // Перевіряємо, чи всі зв'язані сутності завантажені правильно
                if (showtimeDto.Movie == null)
                {
                    _logger.LogWarning($"Для сеансу {id} не знайдено фільм з ID {showtimeDto.MovieId}");
                }
                if (showtimeDto.Hall == null)
                {
                    _logger.LogWarning($"Для сеансу {id} не знайдено зал з ID {showtimeDto.HallId}");
                }
                if (showtimeDto.Cinema == null)
                {
                    _logger.LogWarning($"Для сеансу {id} не знайдено кінотеатр з ID {showtimeDto.CinemaId}");
                }

                return showtimeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні інформації про сеанс з ID: {id}");
                return StatusCode(500, new { error = "Внутрішня помилка сервера", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShowtimeDto>> CreateShowtime(ShowtimeDto showtimeDto)
        {
            try
            {
                _logger.LogInformation($"Спроба створення нового сеансу для фільму {showtimeDto.MovieId}");
                
                // Конвертація часу в UTC
                var startTimeUtc = DateTime.SpecifyKind(showtimeDto.StartTime, DateTimeKind.Utc);
                var endTimeUtc = DateTime.SpecifyKind(showtimeDto.EndTime, DateTimeKind.Utc);

                var hall = await _showtimesRepository.GetHallWithCinemaAsync(showtimeDto.HallId);
                if (hall == null)
                {
                    return BadRequest($"Зал з ID {showtimeDto.HallId} не знайдено");
                }

                if (hall.Cinema == null)
                {
                    return BadRequest($"Для залу з ID {showtimeDto.HallId} не знайдено кінотеатр");
                }

                var showtime = _mapper.Map<Showtime>(showtimeDto);
                showtime.StartTime = startTimeUtc;
                showtime.EndTime = endTimeUtc;
                showtime.CinemaId = hall.Cinema.Id;
                
                try
                {
                    await _showtimesRepository.AddAsync(showtime);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("End time must be after start time") == true)
                {
                    return BadRequest("Час завершення має бути пізніше часу початку");
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Showtime overlaps") == true)
                {
                    return BadRequest("На цей час в цьому залі вже заплановано інший сеанс");
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Start time must be in the future") == true)
                {
                    return BadRequest("Час початку сеансу має бути в майбутньому");
                }
                
                _logger.LogInformation($"Сеанс успішно створено з ID: {showtime.Id}");
                return CreatedAtAction(nameof(GetShowtime), new { id = showtime.Id }, _mapper.Map<ShowtimeDto>(showtime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка створення сеансу");
                return StatusCode(500, $"Внутрішня помилка сервера: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateShowtime(int id, ShowtimeDto showtimeDto)
        {
            try
            {
                _logger.LogInformation($"Спроба оновлення сеансу з ID: {id}");
                
                var showtime = await _showtimesRepository.GetByIdAsync(id);
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound();
                }

                // Конвертація часу в UTC
                var startTimeUtc = DateTime.SpecifyKind(showtimeDto.StartTime, DateTimeKind.Utc);
                var endTimeUtc = DateTime.SpecifyKind(showtimeDto.EndTime, DateTimeKind.Utc);

                var hall = await _showtimesRepository.GetHallWithCinemaAsync(showtimeDto.HallId);
                if (hall == null)
                {
                    return BadRequest($"Зал з ID {showtimeDto.HallId} не знайдено");
                }

                if (hall.Cinema == null)
                {
                    return BadRequest($"Для залу з ID {showtimeDto.HallId} не знайдено кінотеатр");
                }

                // Оновлення властивостей сеансу
                showtime.StartTime = startTimeUtc;
                showtime.EndTime = endTimeUtc;
                showtime.MovieId = showtimeDto.MovieId;
                showtime.Price = showtimeDto.Price;
                showtime.HallId = showtimeDto.HallId;
                showtime.CinemaId = hall.Cinema.Id;

                try
                {
                    await _showtimesRepository.UpdateAsync(showtime);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("End time must be after start time") == true)
                {
                    return BadRequest("Час завершення має бути пізніше часу початку");
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Showtime overlaps") == true)
                {
                    return BadRequest("На цей час в цьому залі вже заплановано інший сеанс");
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Start time must be in the future") == true)
                {
                    return BadRequest("Час початку сеансу має бути в майбутньому");
                }
                
                _logger.LogInformation($"Сеанс з ID {id} успішно оновлено");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка оновлення сеансу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShowtime(int id)
        {
            try
            {
                _logger.LogInformation($"Спроба видалення сеансу з ID: {id}");
                
                var showtime = await _showtimesRepository.GetByIdAsync(id);
                if (showtime == null)
                {
                    _logger.LogWarning($"Сеанс з ID {id} не знайдено");
                    return NotFound();
                }

                await _showtimesRepository.DeleteAsync(id);
                
                _logger.LogInformation($"Сеанс з ID {id} успішно видалено");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка видалення сеансу з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        [HttpGet("movie/{id}")]
        public async Task<ActionResult<IEnumerable<ShowtimeDto>>> GetShowtimesByMovie(int id, [FromQuery] bool includePast = false)
        {
            try
            {
                var movie = await _movieRepository.GetByIdWithDetailsAsync(id);
                if (movie == null)
                {
                    return NotFound($"Movie with ID {id} not found");
                }

                IEnumerable<Showtime> showtimes;
                if (includePast)
                {
                    showtimes = await _showtimesRepository.GetByMovieAsync(id).ToListAsync();
                }
                else
                {
                    showtimes = await _showtimesRepository
                        .GetByMovieAsync(id)
                        .Where(s => s.StartTime > DateTime.UtcNow)
                        .ToListAsync();
                }

                _logger.LogInformation($"Retrieved {showtimes.Count()} showtimes for movie ID {id} with includePast={includePast}");
                return Ok(_mapper.Map<IEnumerable<ShowtimeDto>>(showtimes));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving showtimes for movie ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("cinema/{id}")]
        public async Task<ActionResult<IEnumerable<ShowtimeDto>>> GetShowtimesByCinema(int id, [FromQuery] bool includePast = false)
        {
            try
            {
                var cinemaExists = await _cinemaRepository.CinemaExistsAsync(id);
                if (!cinemaExists)
                {
                    return NotFound($"Cinema with ID {id} not found");
                }

                IEnumerable<Showtime> showtimes;
                if (includePast)
                {
                    showtimes = await _showtimesRepository.GetByCinemaAsync(id).ToListAsync();
                }
                else
                {
                    showtimes = await _showtimesRepository
                        .GetByCinemaAsync(id)
                        .Where(s => s.StartTime > DateTime.UtcNow)
                        .ToListAsync();
                }

                _logger.LogInformation($"Retrieved {showtimes.Count()} showtimes for cinema ID {id} with includePast={includePast}");
                return Ok(_mapper.Map<IEnumerable<ShowtimeDto>>(showtimes));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving showtimes for cinema ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
