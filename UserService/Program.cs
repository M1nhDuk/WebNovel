using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Data;
using Microsoft.OpenApi.Models;
using UserService.Services.Interfaces;
using UserService.UserSettingService.Interface;
using UserService.UserSettingService;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


var novelServiceUrl = builder.Configuration["ServiceUrls:NovelService"] ??
                    throw new InvalidOperationException("ServiceUrls:NovelService is not configured.");

builder.Services.AddHttpClient("NovelServiceClient", client =>
{
    client.BaseAddress = new Uri(novelServiceUrl);
});

builder.Services.AddScoped<IUserFavoriteService, UserFavoriteService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IReadingHistoryService, ReadingHistoryService>();

builder.Services.AddHttpClient();

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
                Console.WriteLine("Exception: " + context.Exception.ToString()); // In toàn b? exception
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllers();

app.Run();
