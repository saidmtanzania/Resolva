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
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<SurveyTemplate> SurveyTemplates => Set<SurveyTemplate>();
    public DbSet<SurveySession> SurveySessions => Set<SurveySession>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<SurveyOutcome> SurveyOutcomes => Set<SurveyOutcome>();



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

        builder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();

            e.HasIndex(x => new { x.TenantId, x.Name });
            e.HasIndex( x=> new { x.TenantId, x.IsActive });
        });
        builder.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();

            e.HasIndex(x => new { x.TenantId, x.Name });
            e.HasIndex( x=> new { x.TenantId, x.IsActive });
        });

        builder.Entity<Event>(e =>
        {
            e.ToTable("events");
            e.HasKey(x => x.Id);

            e.Property(x => x.EventType).IsRequired();
            e.Property(x => x.ContactPhone).IsRequired();
            e.Property(x => x.Status).IsRequired();

            e.Property(x => x.Metadata).HasColumnType("jsonb");

            e.HasIndex(x => new { x.TenantId, x.OccurredAt });
            e.HasIndex(x => new { x.TenantId, x.EventType, x.OccurredAt });
            e.HasIndex(x => new { x.TenantId, x.Status, x.OccurredAt });
            e.HasIndex(x => new { x.TenantId, x.ProductId, x.OccurredAt });
            e.HasIndex(x => new { x.TenantId, x.ServiceId, x.OccurredAt });
        });

        builder.Entity<SurveyTemplate>(e =>
        {
            e.ToTable("survey_templates");
            e.HasKey(x => x.Id);

            e.Property(x => x.EventType).IsRequired();
            e.Property(x => x.Language).IsRequired();
            e.Property(x => x.SchemaJson).HasColumnType("jsonb");

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Channel).IsRequired();
            e.Property(x => x.ValidationErrors).HasColumnType("jsonb");

            e.HasIndex(x => new { x.TenantId, x.EventType, x.Language, x.IsActive });
            e.HasIndex(x => new { x.TenantId, x.WhatsAppStatus });
            e.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        builder.Entity<SurveySession>(e =>
        {
            e.ToTable("survey_sessions");
            e.HasKey(x => x.Id);

            e.Property(x => x.RecipientPhone).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.Channel).IsRequired();

            // MVP: one session per event
            e.HasIndex(x => new { x.TenantId, x.EventId }).IsUnique();

            e.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            e.HasIndex(x => new { x.TenantId, x.TemplateId });
            e.HasIndex(x => x.RecipientPhone);
        });

        builder.Entity<SurveyResponse>(e =>
        {
            e.ToTable("survey_responses");
            e.HasKey(x => x.Id);

            e.Property(x => x.QuestionId).IsRequired();
            e.Property(x => x.AnswerJson).HasColumnType("jsonb");

            e.HasIndex(x => new { x.TenantId, x.SessionId });
            e.HasIndex(x => new { x.TenantId, x.QuestionId });
            e.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        builder.Entity<SurveyOutcome>(e =>
        {
            e.ToTable("survey_outcomes");
            e.HasKey(x => x.SessionId);

            e.Property(x => x.ConfirmationStatus).IsRequired();

            e.HasIndex(x => new { x.TenantId, x.ConfirmationStatus });
            e.HasIndex(x => new { x.TenantId, x.ComputedAt });
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


