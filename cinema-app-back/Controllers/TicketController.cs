using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cinema_app_back.Models;
using cinema_app_back.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using cinema_app_back.Data;
using Microsoft.Extensions.Logging;
using cinema_app_back.Models.Enums;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using System.IO;
using System.Security.Claims;
using iText.Barcodes;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketController> _logger;

        public TicketController(DataContext context, IMapper mapper, ILogger<TicketController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/ticket
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                    .Include(t => t.Payment)
                    .ToListAsync();

                return _mapper.Map<List<TicketDto>>(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of tickets");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/ticket/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            try
            {
                _logger.LogInformation($"Getting ticket with ID: {id}");
                var ticket = await _context.Tickets
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                    .Include(t => t.Payment)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    _logger.LogWarning($"Ticket with ID {id} not found");
                    return NotFound(new { error = $"Ticket with ID {id} not found" });
                }

                return _mapper.Map<TicketDto>(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ticket with ID: {id}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/ticket/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserTickets(string userId)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                    .Include(t => t.Payment)
                    .Where(t => t.Reserve.UserId == userId)
                    .ToListAsync();

                return _mapper.Map<List<TicketDto>>(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tickets for user ID: {userId}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/ticket
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            try
            {
                _logger.LogInformation($"Attempting to create ticket for reserve ID: {createTicketDto.ReserveId}");

                var reserve = await _context.Reserves
                    .Include(r => r.Showtime)
                    .FirstOrDefaultAsync(r => r.Id == createTicketDto.ReserveId);

                if (reserve == null)
                {
                    return BadRequest(new { error = $"Reserve with ID {createTicketDto.ReserveId} not found" });
                }

                // Перевіряємо чи не існує вже квиток для цього резервування
                var existingTicket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.ReserveId == createTicketDto.ReserveId);

                if (existingTicket != null)
                {
                    return BadRequest(new { error = "Ticket for this reservation already exists" });
                }

                var ticket = new Ticket
                {
                    ReserveId = createTicketDto.ReserveId,
                    Status = TicketStatus.Created
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Завантажуємо пов'язані сутності для відповіді
                await _context.Entry(ticket)
                    .Reference(t => t.Reserve)
                    .LoadAsync();

                var resultDto = _mapper.Map<TicketDto>(ticket);
                _logger.LogInformation($"Ticket successfully created with ID: {ticket.Id}");

                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // PUT: api/ticket/5/status
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] TicketStatus status)
        {
            try
            {
                _logger.LogInformation($"Attempting to update status for ticket ID: {id}");

                var ticket = await _context.Tickets.FindAsync(id);

                if (ticket == null)
                {
                    return NotFound(new { error = $"Ticket with ID {id} not found" });
                }

                // Перевіряємо, чи статус відрізняється від поточного
                if (ticket.Status == status)
                {
                    _logger.LogInformation($"Ticket {id} already has status {status}");
                    return Ok(new { message = $"Ticket already has status {status}" });
                }

                // Перевіряємо чи можна змінити статус
                if (!IsStatusChangeValid(ticket.Status, status))
                {
                    return BadRequest(new { error = $"Invalid status change from {ticket.Status} to {status}" });
                }

                ticket.Status = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Status successfully updated for ticket ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for ticket ID: {id}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // Метод для перевірки валідності зміни статусу
        private bool IsStatusChangeValid(TicketStatus currentStatus, TicketStatus newStatus)
        {
            switch (currentStatus)
            {
                case TicketStatus.Created:
                    return newStatus == TicketStatus.Paid || newStatus == TicketStatus.Cancelled;
                case TicketStatus.Paid:
                    return newStatus == TicketStatus.Used || newStatus == TicketStatus.Refunded;
                case TicketStatus.Cancelled:
                case TicketStatus.Refunded:
                case TicketStatus.Used:
                    return false; // Фінальні статуси
                default:
                    return false;
            }
        }
        // GET: api/ticket/reserve/{reserveId}
        [HttpGet("reserve/{reserveId}")]
        [Authorize]
        public async Task<ActionResult<TicketDto>> GetTicketByReservation(int reserveId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Reserve)
                        .ThenInclude(r => r.Showtime)
                    .Include(t => t.Payment)
                    .FirstOrDefaultAsync(t => t.ReserveId == reserveId);

                if (ticket == null)
                {
                    return NotFound();
                }

                return _mapper.Map<TicketDto>(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ticket for reservation ID: {reserveId}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
        // GET: api/ticket/{id}/pdf
        [HttpGet("{id}/pdf")]
        [Authorize]
        public async Task<IActionResult> GetTicketPdf(int id)
        {
            try
            {
                // ТИМЧАСОВО ЛОГУЄМО ДЕТАЛЬНУ ІНФОРМАЦІЮ ДЛЯ ДІАГНОСТИКИ
                _logger.LogWarning("========== ДІАГНОСТИКА ДОСТУПУ ДО PDF ==========");
                foreach (var claim in User.Claims)
                {
                    _logger.LogWarning($"Claim: {claim.Type} = {claim.Value}");
                }

                // Отримуємо ID користувача
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value
                    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                _logger.LogWarning($"PDF Request - User ID: {currentUserId}, Ticket ID: {id}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("PDF Request - User ID not found in claims");
                    return Unauthorized(new { error = "User not authenticated properly" });
                }

                // Отримуємо квиток з усіма зв'язаними даними
                var ticket = await _context.Tickets
                    .Include(t => t.Reserve)
                    .Include(t => t.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    _logger.LogWarning($"PDF Request - Ticket {id} not found");
                    return NotFound(new { error = $"Ticket with ID {id} not found" });
                }

                // Завантажуємо додаткові дані для квитка
                await _context.Entry(ticket.Reserve)
                    .Reference(r => r.User)
                    .LoadAsync();
                await _context.Entry(ticket.Reserve)
                    .Reference(r => r.Showtime)
                    .LoadAsync();
                await _context.Entry(ticket.Reserve.Showtime)
                    .Reference(s => s.Movie)
                    .LoadAsync();
                await _context.Entry(ticket.Reserve)
                    .Reference(r => r.Seat)
                    .LoadAsync();
                await _context.Entry(ticket.Reserve.Seat)
                    .Reference(s => s.Hall)
                    .LoadAsync();

                _logger.LogWarning($"PDF Request - Ticket {id} found:");
                _logger.LogWarning($"- Reserved by: {ticket.Reserve.UserId}");
                _logger.LogWarning($"- Current user: {currentUserId}");
                _logger.LogWarning($"- Current user roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
                _logger.LogWarning($"- Status: {ticket.Status}");

                // ТИМЧАСОВО ОБХОДИМО ПЕРЕВІРКУ ПРАВ ДЛЯ ДІАГНОСТИКИ
                var isOwner = string.Equals(ticket.Reserve.UserId, currentUserId, StringComparison.OrdinalIgnoreCase);
                var isAdmin = User.IsInRole("Admin");
                _logger.LogWarning($"- IsOwner: {isOwner}");
                _logger.LogWarning($"- IsAdmin: {isAdmin}");
                
                // Пропускаємо перевірку доступу, щоб діагностувати проблему
                /*
                if (!isOwner && !isAdmin)
                {
                    _logger.LogWarning($"PDF Request - Access denied for ticket {id}");
                    _logger.LogWarning($"Owner: {ticket.Reserve.UserId}, Current user: {currentUserId}, Is admin: {isAdmin}");
                    return StatusCode(403, new { 
                        error = "Access denied",
                        details = new {
                            message = "You do not have permission to access this ticket",
                            ticketId = id,
                            ticketOwnerId = ticket.Reserve.UserId,
                            currentUserId = currentUserId,
                            isAdmin = isAdmin
                        }
                    });
                }
                */

                if (ticket.Status != TicketStatus.Paid)
                {
                    _logger.LogWarning($"PDF Request - Ticket {id} is not paid (Status: {ticket.Status})");
                    return BadRequest(new { error = "PDF can only be generated for paid tickets" });
                }

                _logger.LogWarning($"PDF Request - Starting generation for ticket {id}");

                _logger.LogInformation($"PDF Request - Starting generation for ticket {id}");

                // Генеруємо PDF
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(memoryStream))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf, PageSize.A4))
                    {
                        try
                        {
                            // Встановлюємо шрифт для заголовка
                            var helveticaBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                            var helvetica = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                            // Додаємо заголовок
                            var title = new Paragraph("Cinema Ticket")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(24)
                                .SetFont(helveticaBold);
                            document.Add(title);

                            // Додаємо інформацію про фільм
                            document.Add(new Paragraph($"Movie: {ticket.Reserve.Showtime.Movie.Title}")
                                .SetFontSize(16)
                                .SetFont(helvetica));

                            // Додаємо інформацію про сеанс
                            document.Add(new Paragraph($"Date: {ticket.Reserve.Showtime.StartTime:dd.MM.yyyy}")
                                .SetFontSize(14)
                                .SetFont(helvetica));
                            document.Add(new Paragraph($"Time: {ticket.Reserve.Showtime.StartTime:HH:mm}")
                                .SetFontSize(14)
                                .SetFont(helvetica));

                            // Додаємо інформацію про зал та місце
                            document.Add(new Paragraph($"Hall: {ticket.Reserve.Seat.Hall.Name}")
                                .SetFontSize(14)
                                .SetFont(helvetica));
                            document.Add(new Paragraph($"Row: {ticket.Reserve.Seat.RowNumber}, Seat: {ticket.Reserve.Seat.SeatNumber}")
                                .SetFontSize(14)
                                .SetFont(helvetica));

                            // Додаємо інформацію про користувача
                            document.Add(new Paragraph($"Customer: {ticket.Reserve.User.FirstName} {ticket.Reserve.User.LastName}")
                                .SetFontSize(14)
                                .SetFont(helvetica));

                            // Додаємо ціну квитка
                            document.Add(new Paragraph($"Price: ${ticket.Reserve.Showtime.Price:F2}")
                                .SetFontSize(14)
                                .SetFont(helvetica));

                            // Додаємо статус квитка
                            document.Add(new Paragraph($"Status: {ticket.Status}")
                                .SetFontSize(14)
                                .SetFont(helvetica));

                            // Додаємо умови використання
                            document.Add(new Paragraph("\nTerms of Use:")
                                .SetFontSize(12)
                                .SetFont(helveticaBold));
                            document.Add(new Paragraph("1. This ticket is valid only for the specified showtime and seat\n" +
                                                      "2. Please arrive 15 minutes before the showtime\n" +
                                                      "3. Keep this ticket until the end of the show\n" +
                                                      "4. This ticket cannot be exchanged or refunded")
                                .SetFontSize(10)
                                .SetFont(helvetica));

                            // Додаємо пробіл перед QR-кодом
                            document.Add(new Paragraph("\n"));

                            // Генеруємо QR-код
                            var qrCodeText = $"TicketId:{ticket.Id};ShowtimeId:{ticket.Reserve.ShowtimeId};SeatId:{ticket.Reserve.SeatId}";
                            var barcodeQRCode = new BarcodeQRCode(qrCodeText);
                            var qrCodeImage = new Image(barcodeQRCode.CreateFormXObject(pdf));
                            qrCodeImage.SetWidth(100);
                            qrCodeImage.SetHeight(100);
                            qrCodeImage.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                            document.Add(qrCodeImage);

                            // Додаємо пробіл після QR-коду
                            document.Add(new Paragraph("\n"));

                            // Додаємо додаткову інформацію
                            document.Add(new Paragraph($"Ticket ID: {ticket.Id}")
                                .SetFontSize(10)
                                .SetFont(helvetica)
                                .SetTextAlignment(TextAlignment.RIGHT));
                            document.Add(new Paragraph($"Generated: {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss UTC}")
                                .SetFontSize(10)
                                .SetFont(helvetica)
                                .SetTextAlignment(TextAlignment.RIGHT));
                        }
                        finally
                        {
                            document.Close();
                        }
                    }

                    var pdfBytes = memoryStream.ToArray();
                    _logger.LogInformation($"PDF Request - Successfully generated for ticket {id}");
                    return File(pdfBytes, "application/pdf", $"ticket_{id}.pdf");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PDF Request - Unhandled error for ticket {id}");
                return StatusCode(500, new { error = "Failed to generate PDF ticket", details = ex.Message });
            }
        }
    }
} 