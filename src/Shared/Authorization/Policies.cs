namespace Shared.Authorization;

public static class Policies
{
    public const string Admin = "AdminPolicy";
    public const string Manager = "ManagerPolicy";
    public const string User = "UserPolicy";
    public const string AdminOrManager = "AdminOrManagerPolicy";
}