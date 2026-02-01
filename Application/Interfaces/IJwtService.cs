using System.Security.Claims;
using backend.Domain.User.Entities;

namespace backend.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Domain.User.Entities.User user, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
