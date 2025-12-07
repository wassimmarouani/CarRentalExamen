using CarRentalExamen.Core.DTOs.Customers;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Customers controller - thin controller that delegates to CustomerService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<Customer>> GetMe()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var customer = await _customerService.GetByUserIdAsync(userId);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerCreateUpdateDto dto)
    {
        var customer = await _customerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerCreateUpdateDto dto)
    {
        var (success, error) = await _customerService.UpdateAsync(id, dto);
        if (!success)
        {
            return error == "Customer not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpPut("me")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateMe([FromBody] CustomerCreateUpdateDto dto)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var customer = await _customerService.GetByUserIdAsync(userId);
        if (customer is null) return NotFound();

        var (success, error) = await _customerService.UpdateAsync(customer.Id, dto);
        if (!success)
        {
            return BadRequest(error);
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _customerService.DeleteAsync(id);
        if (!success)
        {
            return error == "Customer not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer is null) return NotFound();

        var reservations = await _customerService.GetReservationsAsync(id);
        return Ok(reservations);
    }

    [HttpGet("me/reservations")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetMyReservations()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var customer = await _customerService.GetByUserIdAsync(userId);
        if (customer is null) return NotFound();

        var reservations = await _customerService.GetReservationsAsync(customer.Id);
        return Ok(reservations);
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out userId);
    }
}
