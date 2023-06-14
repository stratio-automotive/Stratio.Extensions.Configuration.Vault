namespace Stratio.Extensions.Configuration.Vault;

/// <summary>Stores the result of the Vault Autentication.</summary>
public class VaultAuthenticationResult
{
    /// <summary>Represents the state of authentication.</summary>
    public bool Successful { get; set; }
    /// <summary>Authentication failure reason.</summary>
    public string? FailureReason { get; set; }
}
