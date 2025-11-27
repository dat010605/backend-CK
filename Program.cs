using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using MiniOrderAPI.Services;
using System.Text;
using Microsoft.OpenApi.Models; // <-- QUAN TRỌNG: Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==========================================

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers với JSON options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
// 👇 CẤU HÌNH MỚI ĐỂ HIỆN NÚT AUTHORIZE 👇
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MiniOrderAPI", Version = "v1" });
    
    // Định nghĩa bảo mật Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Yêu cầu bảo mật
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
            new string[] {}
        }
    });
});

// Cấu hình Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình JWT Authentication
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
            ),
            ClockSkew = TimeSpan.Zero 
        };
    });

// Authorization
builder.Services.AddAuthorization();

// TokenService DI
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// ==========================================
// 2. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==========================================

// Swagger UI (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Đã tắt để tránh lỗi CORS khi chạy local

// CORS phải trước Authentication
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================================
// 3. DATABASE MIGRATION & SEEDING
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Áp dụng Migration
    context.Database.Migrate();
    
    // Chạy Seeder (Code trong AppDbContext.cs)
    UserSeeder.Seed(context);
    // Nếu có ProductSeeder thì thêm ở đây
}

app.Run();