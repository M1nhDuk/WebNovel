using NovelService.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NovelService.Mappings;
using NovelService.Service.Interfaces;
using NovelService.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IUserService, UserServiceClient>(client =>
{
    var userServiceUrl = builder.Configuration["ServiceUrls:UserService"];
    client.BaseAddress = new Uri(userServiceUrl ?? "https://localhost:7048"); 
});


builder.Services.AddHttpClient(Options.DefaultName)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            
            if (builder.Environment.IsDevelopment())
            {
                return true;
            }
            return errors == System.Net.Security.SslPolicyErrors.None;
        }
    });


var interactionServiceUrl = builder.Configuration["ServiceUrls:InteractionService"] ??
                              throw new InvalidOperationException("ServiceUrls:InteractionService is not configured.");


builder.Services.AddHttpClient("InteractionServiceClient", client =>
{
    client.BaseAddress = new Uri(interactionServiceUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        if (builder.Environment.IsDevelopment())
        {
            return true;
        }
        return errors == System.Net.Security.SslPolicyErrors.None;
    }
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' + your token "
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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



// Lấy connection string từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");


// Thêm Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };

        option.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("----- JWT Authentication Failed -----");
                Console.WriteLine("Exception: " + context.Exception.ToString()); 
                Console.WriteLine("-------------------------------------");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {

                Console.WriteLine("----- JWT Token Validated -----");
                Console.WriteLine("User: " + context.Principal?.Identity?.Name);
                Console.WriteLine("-----------------------------");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {

                Console.WriteLine("----- JWT Challenge Triggered -----");
                if (context.AuthenticateFailure != null)
                {
                    Console.WriteLine("AuthenticateFailure: " + context.AuthenticateFailure.Message);
                }
                Console.WriteLine("Error: " + context.Error);
                Console.WriteLine("ErrorDescription: " + context.ErrorDescription);
                Console.WriteLine("---------------------------------");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();



// Đăng ký DbContext với MySQL
builder.Services.AddDbContext<NovelDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//Đăng kí AutoMapp
builder.Services.AddAutoMapper(typeof(Mapping));

// Đăng ký service 
builder.Services.AddScoped<INovelSeriesService, NovelSeriesService>();
builder.Services.AddScoped<INovelService, NovelService.Service.NovelService>();
builder.Services.AddScoped<IChapterService, NovelService.Service.ChapterService>();
builder.Services.AddScoped<IClassicSeries, ClassicSeriesService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICategory, CategoryService>();
builder.Services.AddScoped<IStatusService, StatusService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();