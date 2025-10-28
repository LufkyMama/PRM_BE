using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRM_BE.Data;
using PRM_BE.Service;
using PRM_BE.Model.Momo;
using PRM_BE.Service.Momo;
using PRM_BE.Model;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Firebase Admin SDK initialization
var firebaseServiceAccount = "flower-shop-af959-firebase-adminsdk-fbsvc-6308580216.json";
var firebaseCredentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, firebaseServiceAccount);
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseCredentialPath);

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.GetApplicationDefault(),
});
// Connect MomoAPI
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// DI
builder.Services.AddScoped<PRM_BE.Service.UserService>();

builder.Services.AddScoped<PRM_BE.Data.Repository.UserRepository>();

builder.Services.AddScoped<PRM_BE.Service.FlowerService>();

builder.Services.AddScoped<PRM_BE.Data.Repository.FlowerRepo>();

builder.Services.AddScoped< OrderService>();

builder.Services.AddScoped<PRM_BE.Data.Repository.OrderRepository>();

builder.Services.AddScoped<PRM_BE.Data.Repository.OrderItemRepository>();

builder.Services.AddScoped< PaymentService>();

builder.Services.AddScoped<PRM_BE.Data.Repository.PaymentRepository>();

builder.Services.AddScoped< DeliveryService>();

builder.Services.AddScoped<PRM_BE.Data.Repository.DeliveryRepository>();

builder.Services.AddScoped<FirebaseStorageService>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));

builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== JWT AUTH =====
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var key = jwtSection["Key"] ?? throw new Exception("JWT Key is missing");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Bật nhận token qua header "Authorization: Bearer {token}"
        options.RequireHttpsMetadata = true; // để false nếu dev không dùng HTTPS
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            // Không cho phép trễ hạn (mặc định 5 phút). Đặt 0 để test cho dễ predict.
            ClockSkew = TimeSpan.Zero,

            // Nếu bạn đặt claim "role" trong token, map để policy RequireRole hoạt động đúng
            RoleClaimType = ClaimTypes.Role // hoặc "role" nếu bạn tự đặt claim name = "role"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", p => p.RequireRole("Admin"))
    .AddPolicy("staff", p => p.RequireRole("Staff"))
    .AddPolicy("user", p => p.RequireRole("User"));

// ===== Swagger + JWT =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "EXE", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Serve static files
app.UseStaticFiles(); // Serves files from wwwroot by default
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Frontend")),
    RequestPath = "/Frontend"
});

// THỨ TỰ ĐÚNG: Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
