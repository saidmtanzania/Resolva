using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Resolva.Core.Entities;
using Resolva.Core.Enums;
using Resolva.Infrastructure.Data;

namespace Resolva.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResolvaDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure DB exists/migrated
        await db.Database.MigrateAsync();

        // 1) Tenant
        var tenantSlug = "resolva-demo";
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug);
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Resolva Demo",
                Slug = tenantSlug,
                DefaultLanguage = "en",
                SurveyStyle = "friendly"
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        // 2) Roles
        foreach (var r in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        // 3) Admin user
        var adminEmail = "admin@resolva.com";
        var adminUser = await userManager.Users.FirstOrDefaultAsync(u =>
            u.Email == adminEmail && u.TenantId == tenant.Id);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                TenantId = tenant.Id,
                DisplayName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            // Change this password later!
            var result = await userManager.CreateAsync(adminUser, "Password123!");
            if (!result.Succeeded)
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}
