using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using InteractionService.Data;
using InteractionService.Service.Inteface;
using InteractionService.Service;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// C?u hình HttpClient M?C ??NH ?? b? qua l?i SSL (n?u có)
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


var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

// L?y URL t? appsettings
var authServiceUrl = builder.Configuration["ServiceUrls:AuthService"] ??
                    throw new InvalidOperationException("ServiceUrls:AuthService is not configured.");

var novelServiceUrl = builder.Configuration["ServiceUrls:NovelService"] ??
                    throw new InvalidOperationException("ServiceUrls:NovelService is not configured.");



builder.Services.AddHttpClient("AuthServiceClient", client =>
{
    client.BaseAddress = new Uri(authServiceUrl);
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


builder.Services.AddHttpClient("NovelServiceClient", client =>
{
    client.BaseAddress = new Uri(novelServiceUrl);
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

builder.Services.AddScoped<ICommentService, CommentService>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDbContext<InteracDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));




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

    });
builder.Services.AddAuthorization();


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