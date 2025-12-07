using CarRentalExamen.Core.DTOs.Cars;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Cars controller - thin controller that delegates to CarService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Car>>> GetAll([FromQuery] CarStatus? status)
    {
        var cars = await _carService.GetAllAsync(status);
        return Ok(cars);
    }

    [HttpPost("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Car>>> Search([FromBody] CarSearchRequestDto request)
    {
        var cars = await _carService.SearchAsync(request);
        return Ok(cars);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        var car = await _carService.GetByIdWithReservationsAsync(id);
        if (car is null) return NotFound();
        return Ok(car);
    }

    [HttpPost]
    public async Task<ActionResult<Car>> Create([FromBody] CarCreateUpdateDto dto)
    {
        var (success, error, car) = await _carService.CreateAsync(dto);
        if (!success)
        {
            return Conflict(error);
        }
        return CreatedAtAction(nameof(GetById), new { id = car!.Id }, car);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CarCreateUpdateDto dto)
    {
        var (success, error) = await _carService.UpdateAsync(id, dto);
        if (!success)
        {
            return error == "Car not found." ? NotFound() : Conflict(error);
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _carService.DeleteAsync(id);
        if (!success)
        {
            return error == "Car not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] CarStatus status)
    {
        var (success, error) = await _carService.UpdateStatusAsync(id, status);
        if (!success)
        {
            return error == "Car not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }
}
