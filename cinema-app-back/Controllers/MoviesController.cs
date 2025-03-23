using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_app_back;
using cinema_app_back.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using cinema_app_back.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(DataContext context, IWebHostEnvironment webHostEnvironment, ILogger<MoviesController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            return await _context.Movies.ToListAsync();
        }

        // GET: api/movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return movie;
        }

       
        // POST: api/movies
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Movie>> CreateMovie([FromForm] MovieDto movieDto, IFormFile imageFile)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"Invalid file extension: {fileExtension}");
                    return BadRequest("Onlyy .jpg, .jpeg та .png.");
                }

                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "movies");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    _logger.LogInformation($"Creating directory: {uploadsFolder}");
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                // Збереження файлу
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Image saved to: {filePath}");

                // Створення нового фільму з даними з DTO
                var movie = new Movie
                {
                    Title = movieDto.Title,
                    Genre = movieDto.Genre,
                    Image = $"/images/movies/{uniqueFileName}", // Шлях до зображення
                    Description = movieDto.Description,
                    AgeRating = movieDto.AgeRating,
                    DurationInMinutes = movieDto.DurationInMinutes,
                    Director = movieDto.Director,
                    Language = movieDto.Language
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Movie created successfully with ID: {movie.Id}");
                return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // PUT: api/movies/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMovie(int id, [FromForm] MovieDto movieDto, IFormFile imageFile = null)
        {
            var movie = await _context.Movies.FindAsync(id);
            
            if (movie == null)
            {
                return NotFound();
            }

            // Оновлення зображення, якщо воно надане
            if (imageFile != null && imageFile.Length > 0)
            {
                // Перевірка типу файлу
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Непідтримуваний формат файлу. Підтримуються лише .jpg, .jpeg та .png.");
                }

                // Видалення старого зображення, якщо воно існує
                if (!string.IsNullOrEmpty(movie.Image))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Створення унікальної назви файлу
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                
                // Шлях для зберігання зображень
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "movies");
                
                // Створення директорії, якщо вона не існує
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                // Збереження файлу
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Оновлення шляху до зображення
                movie.Image = $"/images/movies/{uniqueFileName}";
            }

            // Оновлення інших даних фільму
            movie.Title = movieDto.Title;
            movie.Genre = movieDto.Genre;
            movie.Description = movieDto.Description;
            movie.AgeRating = movieDto.AgeRating;
            movie.DurationInMinutes = movieDto.DurationInMinutes;
            movie.Director = movieDto.Director;
            movie.Language = movieDto.Language;

            _context.Entry(movie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/movies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            // Видалення зображення, якщо воно існує
            if (!string.IsNullOrEmpty(movie.Image))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
