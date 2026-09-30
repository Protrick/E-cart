using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Infrastructure.Data;
using ECart.Infrastructure.Repositories;
using ECart.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration["Database:ConnectionString"];

        var isPlaceholder = string.IsNullOrWhiteSpace(connectionString) 
            || connectionString.Contains("your_neon_password") 
            || connectionString.Contains("ep-sweet-resonance");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (!isPlaceholder && !string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
            }
            else
            {
                // Fallback to InMemory for instant plug-and-play testing until user adds NeonDB credentials
                options.UseInMemoryDatabase("ECartInMemoryDb");
            }
        });

        // Repositories & UnitOfWork
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Security & Tokens
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
