using ECommerceApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ECommerceApi.Services;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using ECommerceApi.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "AuthPolicy", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(5);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
});


builder.Services.AddIdentity<User, IdentityRole>( options => { options.User.RequireUniqueEmail = true; })
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})


    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];



        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key!))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("CHALLENGE: " + context.Error + " - " + context.ErrorDescription);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("TOKEN VALIDATED for: " + context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});

builder.Services.AddDbContext<AppDbContext>( options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<JwtService>();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var corsSettings = builder.Configuration.GetRequiredSettings<CorsSettings>(CorsSettings.SectionName);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var keyVaultSettings = builder.Configuration.GetRequiredSettings<KeyVaultSettings>(KeyVaultSettings.SectionName);

builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultSettings.Uri),
    new DefaultAzureCredential());

// Key Vault holds production/live secrets (including a live Stripe key), and the line
// above lets it win over appsettings.Development.json for every key. Re-layer the local
// dev file on top in Development only, so any value you've filled in there (test-mode
// Stripe key, local JWT key, local Redis) overrides Key Vault's; keys you leave blank
// locally still fall through to Key Vault. Keeping AddAzureKeyVault unconditional (rather
// than skipping it in Development) also avoids a regression where skipping it made the
// SQL connection's own Active Directory Default auth the first DefaultAzureCredential
// call in the process — its ManagedIdentityCredential probe then ran into SQL's tighter
// connect timeout and failed before falling back to the Azure CLI login.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
}


var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

using var scope = app.Services.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

var roles = new[] { "Admin", "Customer" };

foreach (var role in roles)
{
    if(!await roleManager.RoleExistsAsync(role))
    {
        await roleManager.CreateAsync(new IdentityRole(role));
    }
}

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Safety net: refuse to start in Development with a live-mode Stripe key, no matter how
// it got resolved (Key Vault precedence, a bad merge, a future refactor of the config
// setup above). A live key means Checkout creates real PaymentIntents that can take real
// money — this check doesn't depend on the ordering logic staying correct.
if (app.Environment.IsDevelopment() && Stripe.StripeConfiguration.ApiKey?.StartsWith("sk_live_") == true)
{
    throw new InvalidOperationException(
        "Refusing to start: a live-mode Stripe secret key (sk_live_...) resolved while running in " +
        "Development. Set a test-mode key (sk_test_...) as Stripe:SecretKey in appsettings.Development.json.");
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


