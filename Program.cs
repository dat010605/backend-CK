using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data; // Nhớ dòng này để nhận diện AppDbContext
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using MiniOrderAPI.Services;
using System.Text; // <-- nhận diện Encoding

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==========================================

// Tìm dòng này: builder.Services.AddControllers();
// Và thay thế bằng đoạn này:

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Cấu hình Swagger (để test API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình Database
// Lưu ý: Nếu dùng SQLite thì đổi .UseSqlServer thành .UseSqlite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//cấu hình JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });
// add AUTHORIZATION
builder.Services.AddAuthorization();
// TokenService DI
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// ==========================================
// 2. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==========================================

// Cấu hình Swagger UI khi chạy ở chế độ Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

// Authentication + Authorization middleware

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    UserSeeder.Seed(context);
}




app.MapControllers();
// Tạo Scope để truy cập dịch vụ (Service)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // 1. Áp dụng Migration (tạo database nếu chưa có)
    context.Database.Migrate();

    // 2. Chạy Seeder (Điền dữ liệu mẫu)
    UserSeeder.Seed(context);
}
// Lệnh này BẮT BUỘC phải nằm cuối cùng của đoạn code thực thi
app.Run();