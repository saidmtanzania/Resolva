using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Resolva.Core.Entities;
using Resolva.Infrastructure.Tenancy;

namespace Resolva.Infrastructure.Data;

public class ResolvaDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantContext _tenant;

    public Guid? CurrentTenantId => _tenant.TenantId;
    public ResolvaDbContext(DbContextOptions<ResolvaDbContext> options, ITenantContext tenant) : base(options) 
    {
        _tenant = tenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Slug).IsRequired();

            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.Name);
        });

        foreach (var entityType in builder.Model.GetEntityTypes()){
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType)){
                // Build expression: e => CurrentTenantId.HasValue && e.TenantId == CurrentTenantId.Value
                var method = typeof(ResolvaDbContext)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder, this });
            }
        }
        // Put Identity users in a nicer table name (optional)
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("user_tokens");
    }
    private static void SetTenantFilter<TEntity>(ModelBuilder builder, ResolvaDbContext db)
    where TEntity : class, ITenantScoped{
        builder.Entity<TEntity>().HasQueryFilter(e =>
        db.CurrentTenantId.HasValue && e.TenantId == db.CurrentTenantId.Value);
        }
    public override int SaveChanges(){
        ApplyTenantIdToNewEntities();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default){
        ApplyTenantIdToNewEntities();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void ApplyTenantIdToNewEntities(){
        if (!CurrentTenantId.HasValue) return;
        
        foreach (var entry in ChangeTracker.Entries<ITenantScoped>()){
            if (entry.State == EntityState.Added){
                if (entry.Entity.TenantId == Guid.Empty)
                entry.Entity.TenantId = CurrentTenantId.Value;
            }
        }
    }
}


