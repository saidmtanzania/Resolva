namespace Resolva.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = ""; 
    public string DefaultLanguage { get; set; } = "en";
    public string SurveyStyle { get; set; } = "friendly";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
