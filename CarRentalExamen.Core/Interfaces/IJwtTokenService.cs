using CarRentalExamen.Core.Entities;

namespace CarRentalExamen.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
