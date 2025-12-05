using CarRentalExamen.Core.DTOs.Customers;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CustomerCreateUpdateDto dto)
    {
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CinOrPassport = dto.CinOrPassport,
            LicenseNumber = dto.LicenseNumber,
            Phone = dto.Phone,
            Email = dto.Email
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerCreateUpdateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.CinOrPassport = dto.CinOrPassport;
        customer.LicenseNumber = dto.LicenseNumber;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.Include(c => c.Reservations).FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        if (customer.Reservations.Any())
        {
            return BadRequest("Cannot delete customer with reservations history.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(int id)
    {
        var exists = await _context.Customers.AnyAsync(c => c.Id == id);
        if (!exists) return NotFound();

        var reservations = await _context.Reservations
            .Include(r => r.Car)
            .Where(r => r.CustomerId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Ok(reservations);
    }
}
