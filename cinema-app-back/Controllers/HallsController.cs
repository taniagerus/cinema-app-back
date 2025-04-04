using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using cinema_app_back.Data;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HallsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HallsController> _logger;

        public HallsController(DataContext context, IMapper mapper, ILogger<HallsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/halls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HallDto>>> GetHalls()
        {
            try
            {
                _logger.LogInformation("Отримання списку залів");
                var halls = await _context.Halls
                    .Include(h => h.Cinema)
                    .ToListAsync();
                    
                return _mapper.Map<List<HallDto>>(halls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку залів");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HallDto>> GetHall(int id)
        {
            try
            {
                _logger.LogInformation($"Отримання інформації про зал з ID: {id}");
                var hall = await _context.Halls
                    .Include(h => h.Cinema)
                    .Include(h => h.Seats)  // Додаємо включення місць
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hall == null)
                {
                    _logger.LogWarning($"Зал з ID {id} не знайдено");
                    return NotFound();
                }

                var hallDto = _mapper.Map<HallDto>(hall);
                _logger.LogInformation($"Знайдено {hallDto.Seats?.Count ?? 0} місць для залу {id}");

                return hallDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні інформації про зал з ID: {id}");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // POST: api/halls
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HallDto>> CreateHall(CreateHallDto createHallDto)
        {
            try
            {
                _logger.LogInformation($"Спроба створення нового залу: {System.Text.Json.JsonSerializer.Serialize(createHallDto)}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning($"Помилка валідації моделі: {string.Join(", ", errors)}");
                    return BadRequest(new { errors = ModelState });
                }

                var cinema = await _context.Cinemas.FindAsync(createHallDto.CinemaId);
                if (cinema == null)
                {
                    _logger.LogWarning($"Кінотеатр з ID {createHallDto.CinemaId} не знайдено");
                    return BadRequest(new { errors = new { Cinema = new[] { $"Кінотеатр з ID {createHallDto.CinemaId} не знайдено" } } });
                }

                var hall = new Hall
                {
                    Name = createHallDto.Name,
                    Rows = createHallDto.Rows,
                    SeatsPerRow = createHallDto.SeatsPerRow,
                    CinemaId = createHallDto.CinemaId
                };
                
                // Створюємо місця для залу
                for (int row = 1; row <= createHallDto.Rows; row++)
                {
                    for (int seatNum = 1; seatNum <= createHallDto.SeatsPerRow; seatNum++)
                    {
                        var seat = new Seat
                        {
                            RowNumber = row,
                            SeatNumber = seatNum,
                            DisplayNumber = $"{(char)(64 + row)}{seatNum}",
                            IsAvailable = true,
                            IsReserved = false
                        };
                        hall.Seats.Add(seat);
                    }
                }

                _context.Halls.Add(hall);
                await _context.SaveChangesAsync();
                
                // Завантажуємо створені місця для повернення в DTO
                await _context.Entry(hall)
                    .Collection(h => h.Seats)
                    .LoadAsync();
                
                var resultDto = _mapper.Map<HallDto>(hall);
                _logger.LogInformation($"Зал успішно створено з ID: {hall.Id}");
                
                return CreatedAtAction(nameof(GetHall), new { id = hall.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка створення залу");
                return StatusCode(500, new { error = "Внутрішня помилка сервера" });
            }
        }

        // PUT: api/halls/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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
