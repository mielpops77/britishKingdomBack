using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Ajout de cette ligne
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    });

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string connectionString = configuration.GetConnectionString("DefaultConnection");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        builder =>
     {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
      /*   {
            //http://192.168.1.23 backend coté azure
            //178.32.77.113 british kingdom.fr ovh
            builder.WithOrigins("http://localhost:4200", "http://localhost:65255", "http://localhost:65256", "http://192.168.1.23", "https://www.chatterie-british-kingdom.fr", "https://www.eleveur-connect.fr", "https://www.demo-eleveur-connect.fr", "https://chatterie-britishkingdom.netlify.app")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }); */
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add authentication — registered after AddIdentity so JWT Bearer wins as the
// default authenticate/challenge scheme instead of Identity's cookie scheme
// (otherwise unauthenticated API calls 302-redirect instead of returning 401).
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        ClockSkew = TimeSpan.FromMinutes(2)
    };
    options.SaveToken = true;
});

// Add email service
builder.Services.AddSingleton<IEmailService, EmailService>();

// Add Azure Storage configuration
var azureStorageSettings = configuration.GetSection("AzureStorage").Get<AzureStorageSettings>();
builder.Services.AddSingleton(azureStorageSettings);

// Add Azure Blob Service client
builder.Services.AddScoped(serviceProvider =>
{
    var blobServiceClient = new BlobServiceClient($"DefaultEndpointsProtocol=https;AccountName={azureStorageSettings.AccountName};AccountKey={azureStorageSettings.AccountKey};EndpointSuffix=core.windows.net");
    return blobServiceClient;
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngularFrontend"); // Cors should be placed before UseRouting

app.UseRouting();

app.UseAuthentication(); // Add this line for authentication
app.UseAuthorization();

app.MapControllers();

app.Run();
