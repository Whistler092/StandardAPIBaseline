namespace Movies.API;

public static class AuthConstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";

    public const string TruestedMemberPolicyName = "Truested";
    public const string TruestedMemberClaimName = "trusted_member";

    public const string ApiKeyHeaderName = "x-api-key";

    public const string UserIdClaimName = "userid";
}