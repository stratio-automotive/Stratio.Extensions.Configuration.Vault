namespace Stratio.Extensions.Configuration.Vault.Constants;

/// <summary>The class that contains the supported types of placeholders.</summary>
public static class PlaceholderTypes
{
    /// <summary>Placeholders with this type are materialized into a dictionary.</summary>
    public const string VaultDict = "vault_dict";
    /// <summary>Placeholders with this type are materialized into a string.</summary>
    public const string VaultSecret = "vault_secret";
    /// <summary>Placeholder with this type are materialized into the directory of the user</summary>
    public const string UserHome = "user_home";
}
