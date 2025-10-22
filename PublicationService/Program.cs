using NovelService.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NovelService.Mappings;
using NovelService.Service.Interfaces;
using NovelService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// L?y connection string t? appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");


// ??ng ký DbContext v?i MySQL
builder.Services.AddDbContext<NovelDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//??ng kí AutoMapp
builder.Services.AddAutoMapper(typeof(Mapping));

// Đăng ký service 
builder.Services.AddScoped<INovelSeriesService, NovelSeriesService>();
builder.Services.AddScoped<INovelService, NovelService.Service.NovelService>();
builder.Services.AddScoped<IChapterService, NovelService.Service.ChapterService>();
builder.Services.AddScoped<IClassicSeries, ClassicSeriesService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();


app.Run();
