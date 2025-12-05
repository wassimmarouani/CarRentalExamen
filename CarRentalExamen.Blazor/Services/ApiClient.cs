using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarRentalExamen.Core.DTOs.Auth;
using CarRentalExamen.Core.DTOs.Cars;
using CarRentalExamen.Core.DTOs.Customers;
using CarRentalExamen.Core.DTOs.Payments;
using CarRentalExamen.Core.DTOs.Reservations;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Blazor.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenStorage _tokenStorage;

    public ApiClient(HttpClient httpClient, TokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    private async Task EnsureAuthHeaderAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (auth is not null)
        {
            await _tokenStorage.SetTokenAsync(auth.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        }
        return auth;
    }

    public Task LogoutAsync() => _tokenStorage.ClearAsync();

    public async Task<List<Car>> GetCarsAsync(CarStatus? status = null)
    {
        await EnsureAuthHeaderAsync();
        var url = "api/cars";
        if (status.HasValue) url += $"?status={status}";
        return await _httpClient.GetFromJsonAsync<List<Car>>(url) ?? new List<Car>();
    }

    public async Task<Car?> GetCarAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Car>($"api/cars/{id}");
    }

    public async Task<bool> SaveCarAsync(int? id, CarCreateUpdateDto dto)
    {
        await EnsureAuthHeaderAsync();
        HttpResponseMessage response;
        if (id.HasValue)
        {
            response = await _httpClient.PutAsJsonAsync($"api/cars/{id}", dto);
        }
        else
        {
            response = await _httpClient.PostAsJsonAsync("api/cars", dto);
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCarAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/cars/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCarStatusAsync(int id, CarStatus status)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/cars/{id}/status", status);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<Customer>>("api/customers") ?? new List<Customer>();
    }

    public async Task<Customer?> GetCustomerAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Customer>($"api/customers/{id}");
    }

    public async Task<List<Reservation>> GetCustomerReservationsAsync(int customerId)
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<Reservation>>($"api/customers/{customerId}/reservations") ?? new List<Reservation>();
    }

    public async Task<bool> SaveCustomerAsync(int? id, CustomerCreateUpdateDto dto)
    {
        await EnsureAuthHeaderAsync();
        HttpResponseMessage response = id.HasValue
            ? await _httpClient.PutAsJsonAsync($"api/customers/{id}", dto)
            : await _httpClient.PostAsJsonAsync("api/customers", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ReservationDetailDto>> GetReservationsAsync()
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<ReservationDetailDto>>("api/reservations") ?? new List<ReservationDetailDto>();
    }

    public async Task<ReservationDetailDto?> GetReservationAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<ReservationDetailDto>($"api/reservations/{id}");
    }

    public async Task<bool> CreateReservationAsync(ReservationCreateDto dto)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/reservations", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConfirmReservationAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/reservations/{id}/confirm", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelReservationAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/reservations/{id}/cancel", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PickupReservationAsync(int id, int? mileage, decimal? fuelLevel)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/reservations/{id}/pickup", new
        {
            PickupMileage = mileage,
            PickupFuelLevel = fuelLevel
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CompleteReservationAsync(int id, CompleteReservationDto dto)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/reservations/{id}/complete", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateReturnAsync(ReturnRequestDto dto)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/returns", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddPaymentAsync(PaymentCreateDto dto)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/payments", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<DashboardStats?> GetDashboardAsync()
    {
        await EnsureAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
    }

    public async Task<string?> GetCurrentTokenAsync() => await _tokenStorage.GetTokenAsync();
}

public class ReturnRequestDto
{
    public int ReservationId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int? ReturnMileage { get; set; }
    public decimal? ReturnFuelLevel { get; set; }
    public decimal? LateFees { get; set; }
    public decimal? DamageFees { get; set; }
    public decimal? FuelFees { get; set; }
    public string? Notes { get; set; }
}

public class CompleteReservationDto
{
    public DateTime? ReturnDate { get; set; }
    public int? ReturnMileage { get; set; }
    public decimal? ReturnFuelLevel { get; set; }
    public decimal? LateFees { get; set; }
    public decimal? DamageFees { get; set; }
    public decimal? FuelFees { get; set; }
    public string? Notes { get; set; }
}

public class DashboardStats
{
    public decimal Revenue { get; set; }
    public int ActiveRentals { get; set; }
    public int AvailableCars { get; set; }
    public IEnumerable<RentalMonthStat> RentalsPerMonth { get; set; } = Array.Empty<RentalMonthStat>();
    public IEnumerable<TopCarStat> TopCars { get; set; } = Array.Empty<TopCarStat>();
}

public class RentalMonthStat
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }

    public override string ToString() => $"{Month}/{Year} - {Count}";
}

public class TopCarStat
{
    public int CarId { get; set; }
    public string Car { get; set; } = string.Empty;
    public int Count { get; set; }

    public override string ToString() => $"{Car} ({Count})";
}
