using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PRM_BE.Service.UserService>();
builder.Services.AddScoped<PRM_BE.Data.Repository.UserRepository>();
builder.Services.AddScoped<PRM_BE.Service.FlowerService>();
builder.Services.AddScoped<PRM_BE.Data.Repository.FlowerRepo>();
// Register PostgreSQL DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();   // <-- needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
