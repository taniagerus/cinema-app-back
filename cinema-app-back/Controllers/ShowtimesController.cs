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

        public ShowtimesController(
            IShowtimesRepository showtimesRepository,
            IMapper mapper,
            ILogger<ShowtimesController> logger)
        {
            _showtimesRepository = showtimesRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowtimeDto>>> GetShowtimes(
     [FromQuery] int? movieId,
     [FromQuery] string? date,
     [FromQuery] string? startTimeFrom,
     [FromQuery] string? startTimeTo)
        {
            try
            {
                _logger.LogInformation("Отримання списку сеансів з фільтрами");
                var showtimes = await _showtimesRepository.GetAllWithDetailsAsync();

                // Фільтруємо за movieId, якщо вказано
                if (movieId.HasValue)
                {
                    showtimes = showtimes.Where(s => s.MovieId == movieId.Value);
                }

                // Фільтруємо за датою, якщо вказано
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime filterDate))
                {
                    showtimes = showtimes.Where(s =>
                        s.StartTime.Date == filterDate.Date);
                }

                // Фільтруємо за часом початку "від", якщо вказано
                if (!string.IsNullOrEmpty(startTimeFrom) && TimeSpan.TryParse(startTimeFrom, out TimeSpan fromTime))
                {
                    showtimes = showtimes.Where(s =>
                        s.StartTime.TimeOfDay >= fromTime);
                }

                // Фільтруємо за часом початку "до", якщо вказано
                if (!string.IsNullOrEmpty(startTimeTo) && TimeSpan.TryParse(startTimeTo, out TimeSpan toTime))
                {
                    showtimes = showtimes.Where(s =>
                        s.StartTime.TimeOfDay <= toTime);
                }

                _logger.LogInformation($"Знайдено {showtimes.Count()} сеансів після фільтрації");

                var showtimeDtos = _mapper.Map<List<ShowtimeDto>>(showtimes);

                // Перевіряємо, чи всі зв'язані сутності завантажені правильно
                foreach (var dto in showtimeDtos)
                {
                    if (dto.Movie == null)
                    {
                        _logger.LogWarning($"Для сеансу {dto.Id} не знайдено фільм з ID {dto.MovieId}");
                    }
                    if (dto.Hall == null)
                    {
                        _logger.LogWarning($"Для сеансу {dto.Id} не знайдено зал з ID {dto.HallId}");
                    }
                    if (dto.Cinema == null)
                    {
                        _logger.LogWarning($"Для сеансу {dto.Id} не знайдено кінотеатр з ID {dto.CinemaId}");
                    }
                }

                return showtimeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку сеансів");
                return StatusCode(500, new { error = "Внутрішня помилка сервера", details = ex.Message });
            }
        }
        // GET: api/showtimes/5
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

        // POST: api/showtimes
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
                var currentTimeUtc = DateTime.UtcNow;

                // Перевірка чи дата не в минулому
                if (startTimeUtc <= currentTimeUtc)
                {
                    return BadRequest("Час початку сеансу має бути в майбутньому");
                }

                // Перевірка дат
                if (endTimeUtc <= startTimeUtc)
                {
                    return BadRequest("Час завершення має бути пізніше часу початку");
                }

                // Перевірка на перекриття сеансів в тому ж залі
                var hasOverlap = await _showtimesRepository.HasOverlappingShowtimeAsync(
                    startTimeUtc, 
                    endTimeUtc, 
                    showtimeDto.HallId);
                if (hasOverlap)
                {
                    return BadRequest("На цей час в цьому залі вже заплановано інший сеанс");
                }

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
                
                await _showtimesRepository.AddAsync(showtime);
                
                _logger.LogInformation($"Сеанс успішно створено з ID: {showtime.Id}");
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

                await _showtimesRepository.UpdateAsync(showtime);
                
                _logger.LogInformation($"Сеанс з ID {id} успішно оновлено");
                return NoContent();
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
    }
}
