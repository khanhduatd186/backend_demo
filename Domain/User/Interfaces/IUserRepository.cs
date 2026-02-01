using UserEntity = backend.Domain.User.Entities.User;

namespace backend.Domain.User.Interfaces;

/// <summary>
/// Repository interface cho User entity
/// </summary>
public interface IUserRepository
{
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<UserEntity?> GetByIdAsync(string id);
    Task<UserEntity> CreateAsync(UserEntity user, string password);
    Task<bool> CheckPasswordAsync(UserEntity user, string password);
    Task<IList<string>> GetRolesAsync(UserEntity user);
    Task AddToRoleAsync(UserEntity user, string role);
    Task<bool> RoleExistsAsync(string role);
    Task CreateRoleAsync(string role);
    Task UpdateLanguageAsync(UserEntity user, string language);
}
