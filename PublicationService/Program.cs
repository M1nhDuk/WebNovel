using NovelService.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NovelService.Mappings;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
    
var app = builder.Build();

// L?y connection string t? appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");


// ??ng ký DbContext v?i MySQL
builder.Services.AddDbContext<NovelDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//??ng kí AutoMapp
builder.Services.AddAutoMapper(typeof(Mapping));


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
