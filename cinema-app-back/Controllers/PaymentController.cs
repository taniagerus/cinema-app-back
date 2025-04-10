using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
                    .Include(r => r.Showtime)
                    .FirstOrDefaultAsync(r => r.Id == createPaymentDto.ReserveId);

                if (reserve == null)
                {
                    _logger.LogWarning($"Reserve with ID {createPaymentDto.ReserveId} not found");
                    return BadRequest(new { error = $"Reserve with ID {createPaymentDto.ReserveId} not found" });
                }

                // Перевіряємо чи не існує вже платіж для цього резервування
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.Ticket != null && p.Ticket.ReserveId == createPaymentDto.ReserveId);

                if (existingPayment != null)
                {
                    _logger.LogWarning($"Payment for reserve ID {createPaymentDto.ReserveId} already exists");
                    return BadRequest(new { error = "Payment for this reservation already exists" });
                }

                // Отримуємо ціну сеансу
                decimal amount = createPaymentDto.Price > 0 
                    ? createPaymentDto.Price 
                    : reserve.Showtime?.Price ?? 0;

                if (amount <= 0)
                {
                    _logger.LogWarning($"Invalid payment amount: {amount} for reserve ID {createPaymentDto.ReserveId}");
                    return BadRequest(new { error = "Invalid payment amount" });
                }

                // Створюємо новий платіж
                var payment = new Payment
                {
                    ReserveId = createPaymentDto.ReserveId,
                    PaymentMethod = createPaymentDto.PaymentMethod,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Completed,
                    TransactionId = Guid.NewGuid().ToString(),
                    Amount = amount // Встановлюємо суму платежу
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment created with amount: {payment.Amount} for reserve ID {createPaymentDto.ReserveId}");

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

        // POST: api/payment/update-amounts
        [HttpPost("update-amounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentAmounts()
        {
            try
            {
                _logger.LogInformation("Starting to update all payment amounts");

                // Отримуємо всі платежі з нульовою сумою
                var paymentsToUpdate = await _context.Payments
                    .Include(p => p.Ticket)
                        .ThenInclude(t => t.Reserve)
                            .ThenInclude(r => r.Showtime)
                    .Where(p => p.Amount == 0)
                    .ToListAsync();

                if (!paymentsToUpdate.Any())
                {
                    _logger.LogInformation("No payments to update found");
                    return Ok(new { message = "No payments need updating" });
                }

                int updatedCount = 0;
                foreach (var payment in paymentsToUpdate)
                {
                    if (payment.Ticket?.Reserve?.Showtime != null)
                    {
                        payment.Amount = payment.Ticket.Reserve.Showtime.Price;
                        updatedCount++;
                        _logger.LogInformation($"Updated payment ID {payment.Id} to amount {payment.Amount}");
                    }
                }

                await _context.SaveChangesAsync();
                
                return Ok(new { 
                    message = $"Successfully updated {updatedCount} payments",
                    totalPayments = paymentsToUpdate.Count,
                    updatedPayments = updatedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment amounts");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/payment/summary
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetPaymentsSummary()
        {
            try
            {
                _logger.LogInformation("Retrieving payments summary");

                var totalPayments = await _context.Payments.CountAsync();
                var totalAmount = await _context.Payments.SumAsync(p => p.Amount);
                var paymentsWithZeroAmount = await _context.Payments.CountAsync(p => p.Amount == 0);
                
                var paymentsByMethod = await _context.Payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToListAsync();
                    
                var paymentsByStatus = await _context.Payments
                    .GroupBy(p => p.Status)
                    .Select(g => new {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToListAsync();

                return Ok(new {
                    TotalPayments = totalPayments,
                    TotalAmount = totalAmount,
                    PaymentsWithZeroAmount = paymentsWithZeroAmount,
                    PaymentsByMethod = paymentsByMethod,
                    PaymentsByStatus = paymentsByStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments summary");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/payment/validate-card
        [HttpPost("validate-card")]
        [Authorize]
        public async Task<IActionResult> ValidateCard([FromBody] CardValidationRequest request)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(request.CardNumber) || 
                    string.IsNullOrEmpty(request.ExpiryDate) || 
                    string.IsNullOrEmpty(request.CVC) ||
                    string.IsNullOrEmpty(request.CardholderName))
                {
                    return BadRequest(new { error = "All card details are required" });
                }

                // Validate card number (Luhn algorithm)
                if (!IsValidCardNumber(request.CardNumber))
                {
                    return BadRequest(new { error = "Invalid card number" });
                }

                // Validate expiry date
                if (!IsValidExpiryDate(request.ExpiryDate))
                {
                    return BadRequest(new { error = "Invalid or expired card" });
                }

                // Validate CVC
                if (!IsValidCVC(request.CVC))
                {
                    return BadRequest(new { error = "Invalid CVC" });
                }

                // In a real application, you would make a call to your payment processor here
                // to validate the card details

                return Ok(new { isValid = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating card");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private bool IsValidCardNumber(string cardNumber)
        {
            // Remove any spaces or hyphens
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");
            
            if (!Regex.IsMatch(cardNumber, @"^\d{16}$")) return false;
            
            // Luhn algorithm implementation
            int sum = 0;
            bool isEven = false;
            
            // Loop through values starting from the rightmost digit
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';
                
                if (isEven)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }
                
                sum += digit;
                isEven = !isEven;
            }
            
            return sum % 10 == 0;
        }

        private bool IsValidExpiryDate(string expiryDate)
        {
            if (!Regex.IsMatch(expiryDate, @"^\d{2}/\d{2}$")) return false;
            
            var parts = expiryDate.Split('/');
            if (parts.Length != 2) return false;
            
            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;
                
            // Convert 2-digit year to 4-digit year
            year += 2000;
            
            // Validate month
            if (month < 1 || month > 12)
                return false;
            
            var today = DateTime.Today;
            var cardDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            
            return cardDate > today;
        }

        private bool IsValidCVC(string cvc)
        {
            return Regex.IsMatch(cvc, @"^\d{3,4}$");
        }
    }
} 