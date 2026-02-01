using Microsoft.AspNetCore.Identity;
using backend.Domain.User.Entities;
using backend.Domain.User.Interfaces;

namespace backend.Infrastructure.Repositories.User;

/// <summary>
/// Repository implementation cho User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserManager<Domain.User.Entities.User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(UserManager<Domain.User.Entities.User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Domain.User.Entities.User?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<Domain.User.Entities.User?> GetByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    /// <summary>
    /// Tìm user theo username hoặc email - hỗ trợ đăng nhập bằng cả hai
    /// </summary>
    public async Task<Domain.User.Entities.User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        // Thử tìm theo username trước
        var user = await _userManager.FindByNameAsync(usernameOrEmail);
        if (user != null)
        {
            return user;
        }

        // Nếu không tìm thấy, thử tìm theo email
        return await _userManager.FindByEmailAsync(usernameOrEmail);
    }

    public async Task<Domain.User.Entities.User?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<Domain.User.Entities.User> CreateAsync(Domain.User.Entities.User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }
        return user;
    }

    public async Task<bool> CheckPasswordAsync(Domain.User.Entities.User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<string>> GetRolesAsync(Domain.User.Entities.User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task AddToRoleAsync(Domain.User.Entities.User user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add role: {errors}");
        }
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        return await _roleManager.RoleExistsAsync(role);
    }

    public async Task CreateRoleAsync(string role)
    {
        var result = await _roleManager.CreateAsync(new IdentityRole(role));
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role: {errors}");
        }
    }

    public async Task UpdateLanguageAsync(Domain.User.Entities.User user, string language)
    {
        user.Language = language;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update language: {errors}");
        }
    }
}
