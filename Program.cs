using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using backend.Infrastructure;
using backend.Infrastructure.Data;
using backend.Infrastructure.Middleware;
using backend.Infrastructure.Filters;
using backend.Models;
using backend.Domain.User.Entities;
using backend.Domain.User.Interfaces;
using backend.Domain.Permission.Interfaces;
using backend.Domain.Language.Interfaces;
using backend.Domain.Category.Interfaces;
using backend.Domain.Product.Interfaces;
using backend.Domain.Common;

var builder = WebApplication.CreateBuilder(args);

// ========== CẤU HÌNH LOCALIZATION ==========
// Cấu hình localization với tiếng Việt làm mặc định
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "vi", "en" };
    options.SetDefaultCulture("vi")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new Microsoft.AspNetCore.Localization.QueryStringRequestCultureProvider());
});

// Đăng ký các services vào DI container
builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure layer (Database, Repositories, Services)
builder.Services.AddApplication(); // Application layer (Application Services, AutoMapper)

// ========== CẤU HÌNH JWT AUTHENTICATION ==========
// Lấy cấu hình JWT từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var secretKey = Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JwtSettings:SecretKey is required"));

// Cấu hình Authentication với JWT Bearer
builder.Services.AddAuthentication(options =>
{
    // Sử dụng JWT Bearer làm authentication scheme mặc định
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Lưu token vào AuthenticationProperties sau khi xác thực
    options.RequireHttpsMetadata = false; // Cho phép HTTP trong development (nên đặt true trong production)
    
    // Cấu hình validation cho JWT token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Kiểm tra issuer (người phát hành token)
        ValidateAudience = true, // Kiểm tra audience (đối tượng sử dụng token)
        ValidateLifetime = true, // Kiểm tra thời gian hết hạn của token
        ValidateIssuerSigningKey = true, // Kiểm tra signing key
        ValidIssuer = jwtSettings.Issuer, // Issuer hợp lệ
        ValidAudience = jwtSettings.Audience, // Audience hợp lệ
        IssuerSigningKey = new SymmetricSecurityKey(secretKey), // Key để verify token signature
        ClockSkew = TimeSpan.Zero, // Không cho phép sai lệch thời gian (0 giây)
        // Map claims từ JWT token vào User.Identity
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier, // Map Name claim
        RoleClaimType = System.Security.Claims.ClaimTypes.Role // Map Role claim
    };
    
    // Xử lý lỗi authentication để log chi tiết hơn
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            logger.LogError(context.Exception, "JWT Authentication failed. Authorization header: {AuthHeader}", authHeader);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            logger.LogWarning("JWT Challenge triggered. Authorization header: {AuthHeader}, Error: {Error}", 
                authHeader, context.Error);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Log khi token được validate thành công (để debug)
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var permissions = context.Principal?.FindAll("Permission").Select(c => c.Value).ToList();
            var roles = context.Principal?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            logger.LogInformation("JWT Token validated successfully. UserId: {UserId}, Roles: {Roles}, Permissions: {Permissions}", 
                userId, string.Join(", ", roles ?? new List<string>()), string.Join(", ", permissions ?? new List<string>()));
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Log để xem token có được nhận không
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].ToString();
            logger.LogInformation("JWT Message received. Authorization header: {AuthHeader}", authHeader);
            return Task.CompletedTask;
        }
    };
});

// Đăng ký Controllers và API Explorer
builder.Services.AddControllers(options =>
{
    // Thêm filter để dịch validation error messages
    options.Filters.Add<LocalizedModelStateFilter>();
});
builder.Services.AddEndpointsApiExplorer();

// ========== CẤU HÌNH SWAGGER VỚI JWT SUPPORT ==========
builder.Services.AddSwaggerGen(c =>
{
    // Thông tin API
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend API",
        Version = "v1",
        Description = "API with JWT Authentication"
    });

    // Định nghĩa JWT authentication cho Swagger UI
    // Cho phép nhập token trực tiếp trong Swagger để test API
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Chỉ nhập TOKEN (không cần 'Bearer').\nExample: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization", // Tên header
        In = ParameterLocation.Header, // Token được gửi trong header
        Type = SecuritySchemeType.Http, // Sử dụng Http scheme để tự động thêm "Bearer "
        Scheme = "Bearer", // Scheme name
        BearerFormat = "JWT" // Format của token
    });

    // Thêm security requirement để Swagger tự động gửi token cho các endpoints có [Authorize]
    // Nhưng không bắt buộc (empty array) để login/register không cần token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>() // Không yêu cầu scope cụ thể
        }
    });
});

// ========== CẤU HÌNH CORS ==========
// Cho phép frontend gọi API từ các domain khác
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Cho phép tất cả origins (nên giới hạn trong production)
              .AllowAnyMethod() // Cho phép tất cả HTTP methods
              .AllowAnyHeader(); // Cho phép tất cả headers
    });
});

var app = builder.Build();

// ========== MIGRATE DATABASE VÀ SEED DATA KHI STARTUP ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Tự động chạy migrations nếu có thay đổi
        context.Database.Migrate();

        // Tạo các roles mặc định nếu chưa tồn tại
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "User", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Tạo role Admin1 nếu chưa tồn tại (role không có quyền Product)
        if (!await roleManager.RoleExistsAsync("Admin1"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin1"));
        }

        // Seed database: tạo permissions, gán permissions cho roles, tạo users mặc định
        var seeder = new DatabaseSeeder(
            context,
            services.GetRequiredService<UserManager<backend.Domain.User.Entities.User>>(),
            roleManager,
            services.GetRequiredService<IPermissionRepository>(),
            services.GetRequiredService<IRolePermissionRepository>(),
            services.GetRequiredService<ILanguageRepository>(),
            services.GetRequiredService<ITranslationRepository>(),
            services.GetRequiredService<ICategoryRepository>(),
            services.GetRequiredService<IProductRepository>(),
            services.GetRequiredService<IUnitOfWork>()
        );
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ========== CẤU HÌNH HTTP REQUEST PIPELINE ==========
// Bật Swagger UI trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint
    app.UseSwaggerUI(); // Swagger UI interface
}

app.UseHttpsRedirection(); // Redirect HTTP sang HTTPS

app.UseCors("AllowAll"); // Áp dụng CORS policy

// ========== LOCALIZATION MIDDLEWARE ==========
// Middleware để xử lý ngôn ngữ dựa trên user preference hoặc request
app.UseMiddleware<LocalizationMiddleware>();
app.UseRequestLocalization(); // Áp dụng localization

app.UseAuthentication(); // Xác thực user (kiểm tra JWT token)
app.UseAuthorization(); // Phân quyền (kiểm tra roles và permissions)

app.MapControllers(); // Map các API controllers

app.Run(); // Chạy ứng dụng
