using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["AppSettings:Issuer"], 
            ValidateAudience = true,
            ValidAudience = config["AppSettings:Audience"], 
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["AppSettings:Token"]!)), 
            ValidateIssuerSigningKey = true
        };
    });



builder.Services.AddHttpClient("AuthServiceClient", client =>
{

    client.BaseAddress = new Uri("https://localhost:7154"); 
});

builder.Services.AddHttpClient("PublicationServiceClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7263"); 
});

builder.Services.AddHttpClient("UserServiceClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7048"); 
});

builder.Services.AddHttpClient("InteractionServiceClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7003"); 
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("/index.html");

app.Run();
