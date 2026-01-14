namespace Resolva.Core.Enums;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Support = "Support";
    public const string Noc = "NOC";
    public const string Technician = "Technician";

    public static readonly string[] All = { Admin, Manager, Support, Noc, Technician };
}
