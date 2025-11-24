using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data; // Nhớ dòng này để nhận diện AppDbContext
using System.Text.Json.Serialization;
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

app.UseAuthorization();

app.MapControllers();

// Lệnh này BẮT BUỘC phải nằm cuối cùng của đoạn code thực thi
app.Run();