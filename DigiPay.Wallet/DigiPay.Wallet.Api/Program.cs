using DigiPay.Wallet.Api.Data;
using DigiPay.Wallet.Api.Messaging.EventHandlers;
using DigiPay.Wallet.Api.Messaging.RabbitMQ;
using DigiPay.Wallet.Api.Repositories;
using DigiPay.Wallet.Api.Repositories.Contracts;
using DigiPay.Wallet.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"]!);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure repositories & services
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<JwtService>();

// Configure RabbitMQ
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<TransactionEventHandler>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DigiPay Wallet API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DigiPay Wallet API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "DigiPay Wallet API";
        c.DefaultModelsExpandDepth(-1);
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

app.UseCors("DevPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Services.GetRequiredService<TransactionEventHandler>();

app.Run();