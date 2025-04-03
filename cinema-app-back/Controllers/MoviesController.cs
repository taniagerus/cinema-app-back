using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using cinema_app_back.DTOs;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using cinema_app_back.Repositories;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(
            IMovieRepository movieRepository,
            IWebHostEnvironment webHostEnvironment, 
            ILogger<MoviesController> logger,
            IMapper mapper)
        {
            _movieRepository = movieRepository;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
        {
            try
            {
                var movies = await _movieRepository.GetAllWithDetailsAsync();
                var movieDtos = _mapper.Map<List<MovieDto>>(movies);
                return Ok(movieDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            try
            {
                var movie = await _movieRepository.GetByIdWithDetailsAsync(id);
                if (movie == null)
                {
                    return NotFound(new { error = $"Фільм з ID {id} не знайдено" });
                }

                var movieDto = _mapper.Map<MovieDto>(movie);
                return Ok(movieDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting movie with id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/movies
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieDto>> CreateMovie([FromForm] MovieDto movieDto, IFormFile imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    _logger.LogWarning("No image file provided");
                    return BadRequest("Зображення обов'язкове");
                }

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"Invalid file extension: {fileExtension}");
                    return BadRequest("Тільки .jpg, .jpeg та .png.");
                }

                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "movies");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                var movie = _mapper.Map<Movie>(movieDto);
                movie.Image = $"/images/movies/{uniqueFileName}";

                await _movieRepository.AddAsync(movie);
                
                var resultDto = _mapper.Map<MovieDto>(movie);
                return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/movies/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieDto>> UpdateMovie(int id, [FromForm] MovieDto movieDto, IFormFile imageFile = null)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(new { error = $"Фільм з ID {id} не знайдено" });
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
                    string fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Тільки .jpg, .jpeg та .png.");
                    }

                    if (!string.IsNullOrEmpty(movie.Image))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "movies");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    movie.Image = $"/images/movies/{uniqueFileName}";
                }

                // Оновлюємо властивості фільму окремо, не чіпаючи Id
                movie.Title = movieDto.Title;
                movie.Description = movieDto.Description;
                movie.DurationInMinutes = movieDto.DurationInMinutes;
                movie.AgeRating = movieDto.AgeRating;
                movie.Language = movieDto.Language;
                movie.Genre = movieDto.Genre;
                movie.Director = movieDto.Director;

                await _movieRepository.UpdateAsync(movie);

                var resultDto = _mapper.Map<MovieDto>(movie);
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating movie with id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE: api/movies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(new { error = $"Фільм з ID {id} не знайдено" });
                }

                if (!string.IsNullOrEmpty(movie.Image))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.Image.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                await _movieRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting movie with id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
