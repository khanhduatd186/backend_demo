using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Application.Interfaces;
using backend.Application.Services;
using backend.Domain.Common;
using backend.Domain.User.Interfaces;
using backend.Domain.Product.Interfaces;
using backend.Domain.Permission.Interfaces;
using backend.Domain.Example.Interfaces;
using backend.Domain.Language.Interfaces;
using backend.Domain.Category.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.User;
using backend.Infrastructure.Repositories.Product;
using backend.Infrastructure.Repositories.Permission;
using backend.Infrastructure.Repositories.Example;
using backend.Infrastructure.Repositories.Language;
using backend.Infrastructure.Repositories.Category;
using backend.Infrastructure.Repositories.Common;
using backend.Infrastructure.Services;

namespace backend.Infrastructure;

/// <summary>
/// Extension methods để đăng ký các services vào DI Container
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký Infrastructure services (Database, Identity, Repositories, Infrastructure Services)
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ========== DATABASE CONFIGURATION ==========
        // Lấy connection string từ appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // Đăng ký DbContext với PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ========== IDENTITY CONFIGURATION ==========
        // Cấu hình ASP.NET Core Identity với custom User entity
        services.AddIdentity<Domain.User.Entities.User, IdentityRole>(options =>
        {
            // Cấu hình password requirements
            options.Password.RequireDigit = true; // Yêu cầu có số
            options.Password.RequireLowercase = true; // Yêu cầu có chữ thường
            options.Password.RequireUppercase = true; // Yêu cầu có chữ hoa
            options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
            options.Password.RequiredLength = 6; // Độ dài tối thiểu 6 ký tự
            
            // Cấu hình lockout (khóa tài khoản sau nhiều lần đăng nhập sai)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
            options.Lockout.MaxFailedAccessAttempts = 5; // Tối đa 5 lần thử sai
            options.Lockout.AllowedForNewUsers = true; // Áp dụng lockout cho user mới
            
            // Cấu hình user
            options.User.RequireUniqueEmail = true; // Email phải unique
            options.SignIn.RequireConfirmedEmail = false; // Không yêu cầu xác nhận email
        })
        .AddEntityFrameworkStores<ApplicationDbContext>() // Lưu trữ Identity trong database
        .AddDefaultTokenProviders(); // Thêm token providers mặc định (cho password reset, email confirmation)

        // ========== REPOSITORIES ==========
        // Đăng ký các repositories với lifetime Scoped (một instance mới cho mỗi HTTP request)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExampleRepository, ExampleRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // Unit of Work pattern

        // ========== MEMORY CACHE ==========
        services.AddMemoryCache(); // Cần cho LocalizationService

        // ========== INFRASTRUCTURE SERVICES ==========
        services.AddScoped<IJwtService, JwtService>(); // JWT token service

        return services;
    }

    /// <summary>
    /// Đăng ký Application services (Application Services, AutoMapper)
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ========== AUTOMAPPER ==========
        // Tự động scan và đăng ký tất cả Mapping Profiles trong Assembly
        services.AddAutoMapper(typeof(Application.Mappings.MappingProfile).Assembly);

        // ========== APPLICATION SERVICES ==========
        // Đăng ký các application services với lifetime Scoped
        services.AddScoped<IAuthService, Application.Services.AuthService>(); // Authentication service
        services.AddScoped<Application.Interfaces.IExampleService, Application.Services.ExampleService>(); // Example service
        services.AddScoped<Application.Interfaces.IProductService, Application.Services.ProductService>(); // Product service
        services.AddScoped<Application.Interfaces.ICategoryService, Application.Services.CategoryService>(); // Category service
        services.AddScoped<Application.Interfaces.IPermissionService, Application.Services.PermissionService>(); // Permission service
        services.AddScoped<Application.Interfaces.ILocalizationService, Application.Services.LocalizationService>(); // Localization service
        services.AddScoped<Application.Interfaces.ILanguageService, Application.Services.LanguageService>(); // Language service

        // ========== HTTP CONTEXT ACCESSOR ==========
        // Cần cho LocalizationService
        services.AddHttpContextAccessor();

        return services;
    }
}
