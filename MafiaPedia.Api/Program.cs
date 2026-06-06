using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();

builder.Services.AddDbContext<MafiaDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();

// ۱. تعریف CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5272",
    "https://localhost:7097")

              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.UseStaticFiles(); // این خط باید باشه
// ۲. استفاده از CORS — باید قبل از MapControllers باشه
app.UseCors("AllowFrontend");
//app.UseHttpsRedirection();


app.MapControllers();

app.Run();
