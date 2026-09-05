using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.Data.Sqlite;

namespace ECommerceApi.Tests;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

public static class TestHelpers
{
    public static UserManager<User> CreateUserManager(AppDbContext context)
    {
        var options = Options.Create(new IdentityOptions());
        var store = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<User>(context);
        var validators = new List<IUserValidator<User>> { new UserValidator<User>() };
        var passwordValidators = new List<IPasswordValidator<User>> { new PasswordValidator<User>() };
        return new UserManager<User>(
            store,
            options,
            new PasswordHasher<User>(),
            validators,
            passwordValidators,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<User>>.Instance);
    }

    public static SignInManager<User> CreateSignInManager(UserManager<User> userManager)
    {
        var options = Options.Create(new IdentityOptions());
        var accessor = new HttpContextAccessor();
        var claimsFactory = new UserClaimsPrincipalFactory<User>(userManager, options);
        var schemes = new AuthenticationSchemeProvider(Options.Create(new AuthenticationOptions()));
        var confirmation = new MockUserConfirmation();
        return new SignInManager<User>(
            userManager,
            accessor,
            claimsFactory,
            options,
            NullLogger<SignInManager<User>>.Instance,
            schemes,
            confirmation);
    }

    public static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "this-is-a-test-key-that-is-long-enough-for-hmac-sha256",
            ["Jwt:Issuer"] = "ECommerceApi.Tests",
            ["Jwt:Audience"] = "ECommerceApi.Tests.Client"
        })
        .Build();

    public static void SetUser(ControllerBase controller, string userId, string username = "testuser")
    {
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, username)
                }, "TestAuth"))
            }
        };
    }

    public static Product Product(int id, string name, decimal price = 10m, int stock = 5) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = name.Trim().ToLowerInvariant(),
        Description = $"{name} description",
        Price = price,
        Stock = stock
    };

    private sealed class MockUserConfirmation : IUserConfirmation<User>
    {
        public Task<bool> IsConfirmedAsync(UserManager<User> manager, User user) => Task.FromResult(true);
    }
}
