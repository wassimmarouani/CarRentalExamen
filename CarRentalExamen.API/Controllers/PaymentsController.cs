using CarRentalExamen.Core.DTOs.Payments;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Payments controller - thin controller that delegates to PaymentService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> Create([FromBody] PaymentCreateDto dto)
    {
        var (success, error, payment) = await _paymentService.CreateAsync(dto);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound(error) : BadRequest(error);
        }
        return CreatedAtAction(nameof(GetByReservation), new { reservationId = payment!.ReservationId }, payment);
    }

    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetByReservation(int reservationId)
    {
        var payments = await _paymentService.GetByReservationAsync(reservationId);
        return Ok(payments);
    }
}
