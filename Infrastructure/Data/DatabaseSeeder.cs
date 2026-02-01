using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using backend.Domain.User.Entities;
using backend.Domain.User.Interfaces;
using backend.Domain.Permission.Entities;
using backend.Domain.Permission.Interfaces;
using backend.Domain.Language.Entities;
using backend.Domain.Language.Interfaces;
using backend.Domain.Category.Interfaces;
using backend.Domain.Product.Interfaces;
using backend.Domain.Common;

namespace backend.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Domain.User.Entities.User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ITranslationRepository _translationRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<Domain.User.Entities.User> userManager,
        RoleManager<IdentityRole> roleManager,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        ILanguageRepository languageRepository,
        ITranslationRepository translationRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync()
    {
        // Seed Languages và Translations (phải seed trước)
        await SeedLanguagesAsync();
        await SeedTranslationsAsync();

        // Seed Permissions
        await SeedPermissionsAsync();

        // Seed Roles
        await SeedRolesAsync();

        // Seed Users
        await SeedUsersAsync();

        // Seed Categories
        await SeedCategoriesAsync();

        // Seed Products
        await SeedProductsAsync();
    }

    private async Task SeedLanguagesAsync()
    {
        var languages = new List<(string Code, string Name, bool IsDefault)>
        {
            ("vi", "Tiếng Việt", true),
            ("en", "English", false)
        };

        foreach (var (code, name, isDefault) in languages)
        {
            var existing = await _languageRepository.GetByCodeAsync(code);
            if (existing == null)
            {
                var language = new Language
                {
                    Code = code,
                    Name = name,
                    IsActive = true,
                    IsDefault = isDefault
                };
                await _languageRepository.AddAsync(language);
            }
            else
            {
                // Cập nhật nếu cần
                existing.Name = name;
                existing.IsDefault = isDefault;
                existing.IsActive = true;
                await _languageRepository.UpdateAsync(existing);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SeedTranslationsAsync()
    {
        // Đọc JSON files và import vào database
        var jsonFiles = new[] { "translations.vi.json", "translations.en.json" };

        // Lấy path đến Resources folder
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        if (!Directory.Exists(resourcesPath))
        {
            // Thử path khác nếu không tìm thấy
            resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        }

        foreach (var jsonFile in jsonFiles)
        {
            var filePath = Path.Combine(resourcesPath, jsonFile);
            if (!File.Exists(filePath))
            {
                // Log warning nhưng không throw exception
                Console.WriteLine($"Warning: Translation file not found: {filePath}");
                continue;
            }

            var languageCode = jsonFile.Replace("translations.", "").Replace(".json", "");
            var language = await _languageRepository.GetByCodeAsync(languageCode);
            if (language == null)
            {
                continue;
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            if (translations != null)
            {
                foreach (var (key, value) in translations)
                {
                    var existing = await _translationRepository.GetByKeyAndLanguageIdAsync(key, language.Id);
                    if (existing == null)
                    {
                        var translation = new Translation
                        {
                            Key = key,
                            Value = value,
                            LanguageId = language.Id
                        };
                        await _translationRepository.AddAsync(translation);
                    }
                    else
                    {
                        // Cập nhật nếu giá trị thay đổi
                        if (existing.Value != value)
                        {
                            existing.Value = value;
                            await _translationRepository.UpdateAsync(existing);
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        var permissions = new List<(string Name, string Description, string Resource, string Action)>
        {
            // Product permissions
            ("Product.Create", "Create product", "Product", "Create"),
            ("Product.Read", "Read product", "Product", "Read"),
            ("Product.Update", "Update product", "Product", "Update"),
            ("Product.Delete", "Delete product", "Product", "Delete"),

            // Category permissions
            ("Category.Create", "Create category", "Category", "Create"),
            ("Category.Read", "Read category", "Category", "Read"),
            ("Category.Update", "Update category", "Category", "Update"),
            ("Category.Delete", "Delete category", "Category", "Delete"),

            // Permission permissions
            ("Permission.Create", "Create permission", "Permission", "Create"),
            ("Permission.Read", "Read permission", "Permission", "Read"),
            ("Permission.Update", "Update permission", "Permission", "Update"),
            ("Permission.Delete", "Delete permission", "Permission", "Delete"),

            // User permissions
            ("User.Create", "Create user", "User", "Create"),
            ("User.Read", "Read user", "User", "Read"),
            ("User.Update", "Update user", "User", "Update"),
            ("User.Delete", "Delete user", "User", "Delete"),

            // Auth permissions
            ("Auth.Login", "Login", "Auth", "Login"),
            ("Auth.Register", "Register", "Auth", "Register"),
        };

        foreach (var (name, description, resource, action) in permissions)
        {
            var existing = await _permissionRepository.GetByNameAsync(name);
            if (existing == null)
            {
                var permission = new Domain.Permission.Entities.Permission
                {
                    Name = name,
                    Description = description,
                    Resource = resource,
                    Action = action
                };
                await _permissionRepository.AddAsync(permission);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        // Create Admin role if not exists
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create Admin1 role if not exists
        if (!await _roleManager.RoleExistsAsync("Admin1"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin1"));
        }

        // Get all permissions
        var allPermissions = await _permissionRepository.GetAllAsync();
        var permissionList = allPermissions.ToList();

        // Get Admin role
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            // Assign ALL permissions to Admin role (including Product)
            await AssignPermissionsToRoleAsync(adminRole.Id, permissionList);
        }

        // Get Admin1 role
        var admin1Role = await _roleManager.FindByNameAsync("Admin1");
        if (admin1Role != null)
        {
            // Assign all permissions EXCEPT Product permissions to Admin1 role
            var permissionsWithoutProduct = permissionList
                .Where(p => !p.Resource.Equals("Product", StringComparison.OrdinalIgnoreCase))
                .ToList();
            await AssignPermissionsToRoleAsync(admin1Role.Id, permissionsWithoutProduct);
        }
    }

    private async Task AssignPermissionsToRoleAsync(string roleId, List<Domain.Permission.Entities.Permission> permissions)
    {
        // Get existing permissions for this role (only active ones)
        var existingRolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(roleId);
        var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        // Get permission IDs that should be assigned
        var targetPermissionIds = permissions.Select(p => p.Id).ToHashSet();

        // Remove permissions that are no longer needed (soft delete)
        var permissionsToRemove = existingRolePermissions
            .Where(rp => !targetPermissionIds.Contains(rp.PermissionId))
            .ToList();
        
        foreach (var rolePermission in permissionsToRemove)
        {
            await _rolePermissionRepository.DeleteAsync(rolePermission);
        }

        // Add or restore permissions
        foreach (var permission in permissions)
        {
            if (!existingPermissionIds.Contains(permission.Id))
            {
                // Check if exists but soft deleted
                var existingDeleted = await _rolePermissionRepository.GetByRoleIdAndPermissionIdIncludingDeletedAsync(roleId, permission.Id);
                
                if (existingDeleted != null && existingDeleted.IsDeleted)
                {
                    // Restore soft deleted record
                    existingDeleted.IsDeleted = false;
                    existingDeleted.UpdatedAt = DateTime.UtcNow;
                    await _rolePermissionRepository.UpdateAsync(existingDeleted);
                }
                else if (existingDeleted == null)
                {
                    // Create new record
                    var rolePermission = new Domain.Permission.Entities.RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permission.Id
                    };
                    await _rolePermissionRepository.AddAsync(rolePermission);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        // Create admin user
        var adminUser = await _userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            adminUser = new Domain.User.Entities.User
            {
                UserName = "admin",
                Email = "admin@example.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                Language = "vi" // Mặc định tiếng Việt
            };

            var result = await _userManager.CreateAsync(adminUser, "1q2w3E*");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Ensure admin user has Admin role
            var roles = await _userManager.GetRolesAsync(adminUser);
            if (!roles.Contains("Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create admin1 user
        var admin1User = await _userManager.FindByEmailAsync("admin1@example.com");
        if (admin1User == null)
        {
            admin1User = new Domain.User.Entities.User
            {
                UserName = "admin1",
                Email = "admin1@example.com",
                EmailConfirmed = true,
                FirstName = "Admin1",
                LastName = "User",
                IsActive = true,
                Language = "vi" // Mặc định tiếng Việt
            };

            var result = await _userManager.CreateAsync(admin1User, "1q2w3E*");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin1User, "Admin1");
            }
        }
        else
        {
            // Ensure admin1 user has Admin1 role
            var roles = await _userManager.GetRolesAsync(admin1User);
            if (!roles.Contains("Admin1"))
            {
                await _userManager.AddToRoleAsync(admin1User, "Admin1");
            }
        }
    }

    private async Task SeedCategoriesAsync()
    {
        var categories = new List<(string Code, string Description)>
        {
            ("ELEC", "Điện tử"),
            ("CLOTH", "Quần áo"),
            ("FOOD", "Thực phẩm"),
            ("BOOK", "Sách"),
            ("TOY", "Đồ chơi"),
            ("FURN", "Nội thất"),
            ("SPORT", "Thể thao"),
            ("BEAUTY", "Làm đẹp"),
            ("HOME", "Đồ gia dụng"),
            ("AUTO", "Ô tô & Xe máy")
        };

        foreach (var (code, description) in categories)
        {
            var existing = await _categoryRepository.GetByCodeAsync(code);
            if (existing == null)
            {
                var category = new Domain.Category.Entities.Category
                {
                    Code = code,
                    Description = description
                };
                await _categoryRepository.AddAsync(category);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SeedProductsAsync()
    {
        // Lấy tất cả categories
        var allCategories = await _categoryRepository.GetAllAsync();
        var categoryList = allCategories.ToList();

        if (!categoryList.Any())
        {
            // Nếu chưa có category, không tạo products
            return;
        }

        var random = new Random();
        var productNames = new List<string>
        {
            "Sản phẩm", "Hàng hóa", "Mặt hàng", "Đồ dùng", "Vật phẩm",
            "Thiết bị", "Dụng cụ", "Phụ kiện", "Linh kiện", "Nguyên liệu"
        };

        var productTypes = new List<string>
        {
            "Cao cấp", "Tiêu chuẩn", "Phổ thông", "Chuyên dụng", "Đặc biệt",
            "Cao cấp", "Bình dân", "Thương hiệu", "Nhập khẩu", "Nội địa"
        };

        // Tạo 100 products
        for (int i = 1; i <= 100; i++)
        {
            var productCode = $"PROD{i:D4}"; // PROD0001, PROD0002, ...
            var existing = await _productRepository.GetByProductCodeAsync(productCode);
            
            if (existing == null)
            {
                // Chọn category ngẫu nhiên
                var randomCategory = categoryList[random.Next(categoryList.Count)];
                
                // Tạo tên sản phẩm ngẫu nhiên
                var productName = $"{productNames[random.Next(productNames.Count)]} {productTypes[random.Next(productTypes.Count)]} {i}";
                
                var product = new Domain.Product.Entities.Product
                {
                    ProductCode = productCode,
                    ProductName = productName,
                    CategoryId = randomCategory.Id,
                    Image = $"https://example.com/images/product_{i}.jpg"
                };
                
                await _productRepository.AddAsync(product);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
