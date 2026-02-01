# Backend API

ASP.NET Core 8.0 Web API với JWT Authentication, PostgreSQL, và Clean Architecture.

## Tính năng

- 🔐 **JWT Authentication & Authorization** - Xác thực và phân quyền dựa trên JWT token
- 🌍 **Localization** - Hỗ trợ đa ngôn ngữ (Tiếng Việt, Tiếng Anh)
- 🔑 **Permission-based Authorization** - Phân quyền chi tiết theo permission
- 📦 **Clean Architecture** - Kiến trúc phân lớp rõ ràng (Domain, Application, Infrastructure)
- 🗄️ **PostgreSQL Database** - Sử dụng Entity Framework Core với PostgreSQL
- 📝 **Swagger/OpenAPI** - Tài liệu API tự động với Swagger UI
- 🔄 **AutoMapper** - Mapping tự động giữa DTOs và Entities
- 📄 **Paging & Filtering** - Hỗ trợ phân trang và lọc dữ liệu

## Công nghệ sử dụng

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **PostgreSQL** (Npgsql.EntityFrameworkCore.PostgreSQL)
- **JWT Bearer Authentication**
- **ASP.NET Core Identity**
- **AutoMapper**
- **Swagger/OpenAPI**

## Cấu trúc dự án

```
backend/
├── Application/          # Application Layer
│   ├── DTOs/            # Data Transfer Objects
│   ├── Interfaces/      # Service Interfaces
│   ├── Services/        # Application Services
│   ├── Mappings/        # AutoMapper Profiles
│   └── Helpers/         # Helper Classes
├── Domain/              # Domain Layer
│   ├── Entities/        # Domain Entities
│   ├── Interfaces/      # Repository Interfaces
│   └── Common/          # Base Classes & Interfaces
├── Infrastructure/      # Infrastructure Layer
│   ├── Data/            # DbContext & Database Seeding
│   ├── Repositories/    # Repository Implementations
│   ├── Services/        # Infrastructure Services (JWT, etc.)
│   ├── Middleware/      # Custom Middleware
│   └── Filters/         # Action Filters
├── Controllers/         # API Controllers
├── Attributes/          # Custom Attributes
├── Models/              # Configuration Models
├── Resources/           # Localization Resources
└── Migrations/          # EF Core Migrations
```

## Yêu cầu hệ thống

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 / VS Code / Rider

## Cài đặt

### 1. Clone repository

```bash
git clone https://github.com/khanhduatd186/backend_demo.git
cd backend_demo
```

### 2. Cấu hình Database

Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=backend_db;Username=postgres;Password=your_password"
  }
}
```

### 3. Cấu hình JWT

Cập nhật JWT settings trong `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "BackendAPI",
    "Audience": "BackendAPIUsers",
    "ExpirationInMinutes": 60
  }
}
```

### 4. Chạy Migrations

Database sẽ tự động migrate khi ứng dụng khởi động. Hoặc chạy thủ công:

```bash
dotnet ef database update
```

### 5. Chạy ứng dụng

```bash
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication

- `POST /api/auth/register` - Đăng ký tài khoản mới
- `POST /api/auth/login` - Đăng nhập và nhận JWT token
- `POST /api/auth/refresh-token` - Làm mới access token
- `GET /api/auth/me` - Lấy thông tin user hiện tại (yêu cầu authentication)

### Categories

- `POST /api/category` - Tạo category mới (yêu cầu permission: `Category.Create`)
- `GET /api/category/{id}` - Lấy category theo ID (yêu cầu permission: `Category.Read`)
- `GET /api/category` - Lấy danh sách category có phân trang (yêu cầu permission: `Category.Read`)
- `GET /api/category/filtered` - Lọc category (yêu cầu permission: `Category.Read`)
- `PUT /api/category/{id}` - Cập nhật category (yêu cầu permission: `Category.Update`)
- `DELETE /api/category/{id}` - Xóa category (yêu cầu permission: `Category.Delete`)

### Products

- `POST /api/product` - Tạo product mới (yêu cầu permission: `Product.Create`)
- `GET /api/product/{id}` - Lấy product theo ID (yêu cầu permission: `Product.Read`)
- `GET /api/product` - Lấy danh sách product có phân trang (yêu cầu permission: `Product.Read`)
- `GET /api/product/filtered` - Lọc product (yêu cầu permission: `Product.Read`)
- `PUT /api/product/{id}` - Cập nhật product (yêu cầu permission: `Product.Update`)
- `DELETE /api/product/{id}` - Xóa product (yêu cầu permission: `Product.Delete`)

### Permissions

- `POST /api/permission` - Tạo permission mới
- `GET /api/permission/{id}` - Lấy permission theo ID
- `GET /api/permission` - Lấy danh sách permission
- `GET /api/permission/filtered` - Lọc permission
- `PUT /api/permission/{id}` - Cập nhật permission
- `DELETE /api/permission/{id}` - Xóa permission
- `POST /api/permission/assign-to-role` - Gán permissions cho role

### Languages & Localization

- `GET /api/localization/languages` - Lấy danh sách ngôn ngữ hỗ trợ
- `GET /api/localization/translations` - Lấy translations theo ngôn ngữ
- `POST /api/localization/language` - Tạo ngôn ngữ mới
- `POST /api/localization/translation` - Tạo translation mới

## Authentication

### Đăng ký

```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "userName": "username"
}
```

### Đăng nhập

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiration": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "userName": "username"
  }
}
```

### Sử dụng Token

Thêm header vào request:

```
Authorization: Bearer {token}
```

## Localization

API hỗ trợ đa ngôn ngữ thông qua query parameter:

```
GET /api/category?culture=vi
GET /api/category?culture=en
```

Mặc định là tiếng Việt (`vi`).

## Permissions

Hệ thống sử dụng permission-based authorization. Các permissions mặc định:

- `Category.Create`, `Category.Read`, `Category.Update`, `Category.Delete`
- `Product.Create`, `Product.Read`, `Product.Update`, `Product.Delete`
- `Permission.*` (quản lý permissions)

## Roles mặc định

Khi khởi động, hệ thống tự động tạo các roles:

- **Admin** - Có tất cả quyền
- **User** - Quyền cơ bản
- **Manager** - Quyền quản lý
- **Admin1** - Admin không có quyền Product

## Database Seeding

Khi ứng dụng khởi động lần đầu, hệ thống sẽ tự động:

1. Tạo các roles mặc định
2. Tạo các permissions mặc định
3. Gán permissions cho roles
4. Tạo users mặc định (nếu có)
5. Tạo languages và translations mặc định

## Development

### Tạo Migration mới

```bash
dotnet ef migrations add MigrationName
```

### Cập nhật Database

```bash
dotnet ef database update
```

### Xóa Migration cuối cùng

```bash
dotnet ef migrations remove
```

## Cấu trúc Clean Architecture

- **Domain Layer**: Chứa business entities và interfaces, không phụ thuộc vào bất kỳ layer nào
- **Application Layer**: Chứa business logic, DTOs, và service interfaces
- **Infrastructure Layer**: Chứa implementations của repositories, database context, và external services
- **Presentation Layer**: Controllers và API endpoints

## License

MIT

## Tác giả

khanhduatd186
