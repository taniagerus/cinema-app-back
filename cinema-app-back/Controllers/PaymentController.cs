using System;
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

namespace cinema_app_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(DataContext context, IMapper mapper, ILogger<PaymentController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // POST: api/payment/process
        [HttpPost("process")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(CreatePaymentDto createPaymentDto)
        {
            try
            {
                _logger.LogInformation($"Attempting to process payment for reserve ID: {createPaymentDto.ReserveId}");

                // Перевіряємо чи існує резервування
                var reserve = await _context.Reserves
                    .FirstOrDefaultAsync(r => r.Id == createPaymentDto.ReserveId);

                if (reserve == null)
                {
                    _logger.LogWarning($"Reserve with ID {createPaymentDto.ReserveId} not found");
                    return BadRequest(new { error = $"Reserve with ID {createPaymentDto.ReserveId} not found" });
                }

                // Перевіряємо чи не існує вже платіж для цього резервування
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.ReserveId == createPaymentDto.ReserveId);

                if (existingPayment != null)
                {
                    _logger.LogWarning($"Payment for reserve ID {createPaymentDto.ReserveId} already exists");
                    return BadRequest(new { error = "Payment for this reservation already exists" });
                }

                // Створюємо новий платіж
                var payment = new Payment
                {
                    ReserveId = createPaymentDto.ReserveId,
                    PaymentMethod = createPaymentDto.PaymentMethod,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Completed,
                    TransactionId = Guid.NewGuid().ToString()
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Створюємо квиток після успішної оплати
                var ticket = new Ticket
                {
                    ReserveId = createPaymentDto.ReserveId,
                    PaymentId = payment.Id,
                    Status = TicketStatus.Paid
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<PaymentDto>(payment);
                _logger.LogInformation($"Payment successfully processed with ID: {payment.Id}");

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: api/payment/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            try
            {
                _logger.LogInformation($"Getting payment with ID: {id}");
                var payment = await _context.Payments
                    .Include(p => p.Reserve)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment with ID {id} not found");
                    return NotFound();
                }

                return _mapper.Map<PaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/payment/refund/5
        [HttpPost("refund/{paymentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(int paymentId)
        {
            try
            {
                _logger.LogInformation($"Attempting to refund payment with ID: {paymentId}");

                var payment = await _context.Payments
                    .Include(p => p.Ticket)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment with ID {paymentId} not found");
                    return NotFound();
                }

                if (payment.Status == PaymentStatus.Refunded)
                {
                    return BadRequest(new { error = "Payment is already refunded" });
                }

                payment.Status = PaymentStatus.Refunded;
                payment.RefundDate = DateTime.UtcNow;

                // Оновлюємо статус квитка
                if (payment.Ticket != null)
                {
                    payment.Ticket.Status = TicketStatus.Refunded;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment with ID {paymentId} successfully refunded");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error refunding payment with ID: {paymentId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 