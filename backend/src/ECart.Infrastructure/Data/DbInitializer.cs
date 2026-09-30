using ECart.Application.Interfaces.Services;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECart.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger)
    {
        try
        {
            // Ensure tables are created
            await context.Database.EnsureCreatedAsync();

            // Seed Users
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding default users...");

                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@ecart.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@12345"),
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = UserRole.Admin,
                    IsActive = true
                };

                var customer = new User
                {
                    Username = "johndoe",
                    Email = "customer@ecart.com",
                    PasswordHash = passwordHasher.HashPassword("Customer@12345"),
                    FirstName = "John",
                    LastName = "Doe",
                    Role = UserRole.Customer,
                    IsActive = true
                };

                await context.Users.AddRangeAsync(admin, customer);
                await context.SaveChangesAsync();
                logger.LogInformation("Default users seeded successfully (Admin: admin@ecart.com, Customer: customer@ecart.com).");
            }

            // Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                logger.LogInformation("Seeding categories and products...");

                var catElectronics = new Category
                {
                    Name = "Electronics & Audio",
                    Slug = "electronics-audio",
                    Description = "High performance gadgets, audio systems and premium electronics",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80",
                    IsActive = true
                };

                var catWearables = new Category
                {
                    Name = "Wearable Tech",
                    Slug = "wearable-tech",
                    Description = "Next-gen smartwatches, fitness trackers and accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800&auto=format&fit=crop&q=80",
                    IsActive = true
                };

                var catApparel = new Category
                {
                    Name = "Apparel & Lifestyle",
                    Slug = "apparel-lifestyle",
                    Description = "Modern streetwear, minimalist fashion and premium everyday apparel",
                    ImageUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=800&auto=format&fit=crop&q=80",
                    IsActive = true
                };

                var catWorkspace = new Category
                {
                    Name = "Workspace & Setup",
                    Slug = "workspace-setup",
                    Description = "Ergonomic desk accessories, keyboards, and creator essentials",
                    ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&auto=format&fit=crop&q=80",
                    IsActive = true
                };

                await context.Categories.AddRangeAsync(catElectronics, catWearables, catApparel, catWorkspace);
                await context.SaveChangesAsync();

                // Seed Products
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Aura Pro Wireless ANC Headphones",
                        Description = "Industry-leading active noise cancellation with 40-hour battery life and spatial audio immersion.",
                        Sku = "AUR-ANC-001",
                        Price = 299.99m,
                        StockQuantity = 45,
                        ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catElectronics.Id,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Pulse Ultra Smart Watch Titanium",
                        Description = "Aerospace-grade titanium chassis with sapphire crystal display, ECG sensor, and GPS.",
                        Sku = "PLS-SMW-002",
                        Price = 449.00m,
                        StockQuantity = 30,
                        ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catWearables.Id,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Studio Mechanical Keyboard (RGB)",
                        Description = "Custom mechanical keyboard with hot-swappable tactile switches, sound dampening foam, and per-key RGB.",
                        Sku = "KB-MECH-003",
                        Price = 169.50m,
                        StockQuantity = 60,
                        ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catWorkspace.Id,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Minimalist Merino Wool Hoodie",
                        Description = "Ultra-soft 100% organic merino wool hoodie with moisture-wicking and temperature regulating properties.",
                        Sku = "APP-MRN-004",
                        Price = 119.00m,
                        StockQuantity = 80,
                        ImageUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catApparel.Id,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "HyperGlide Wireless Gaming Mouse",
                        Description = "Featherlight 58g ergonomic gaming mouse with 26K optical sensor and PTFE feet.",
                        Sku = "MOU-HYP-005",
                        Price = 89.99m,
                        StockQuantity = 110,
                        ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catWorkspace.Id,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "SonicBeam Portable Bluetooth Speaker",
                        Description = "IPX7 waterproof 360-degree surround sound speaker with 24-hour battery and deep punchy bass.",
                        Sku = "SPK-SON-006",
                        Price = 129.95m,
                        StockQuantity = 50,
                        ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=800&auto=format&fit=crop&q=80",
                        CategoryId = catElectronics.Id,
                        IsActive = true
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Categories and products seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
