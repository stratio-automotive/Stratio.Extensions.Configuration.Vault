namespace Stratio.Extensions.Configuration.Vault.Constants;

/// <summary>The class that contains the constants related with settings.</summary>
public static class EnvironmentVariables
{
    /// <summary>Address of the Vault server.</summary>
    public static readonly string VaultAddress = "VAULT_ADDR";
    /// <summary>Vault's mountpoint that should be acessed.</summary>
    public static readonly string VaultMountPoint = "VAULT_MOUNTPOINT";
    /// <summary>Validate or not the Vault's TLS Certificate.</summary>
    public static readonly string VaultSkipVerify = "VAULT_SKIP_VERIFY";

    /// <summary>AppRole authentication method name.</summary>
    public static readonly string AppRoleAuthName = "APPROLE_AUTH_NAME";
    /// <summary>AppRole authentication role ID path.</summary>
    public static readonly string AppRoleRoleIdPath = "APPROLE_ROLE_ID_PATH";
    /// <summary>AppRole authentication secret ID path.</summary>
    public static readonly string AppRoleSecretIdPath = "APPROLE_SECRET_ID_PATH";

    /// <summary>Kubernetes authentication method name.</summary>
    public static readonly string KubernetesAuthName = "VAULT_K8S_NAME";
    /// <summary>Kubernetes authentication role name.</summary>
    public static readonly string KubernetesSaRoleName = "VAULT_ROLE";
    /// <summary>Kuberntes authenatication service account token path.</summary>
    public static readonly string KubernetesSaTokenPath = "SA_TOKEN_PATH";

    /// <summary>Certificate path.</summary>
    public static readonly string CertificatePath = "VAULT_CLIENT_CERT";
    /// <summary>Certificate name.</summary>
    public static readonly string CertificateRoleName = "VAULT_CLIENT_CERT_NAME";
    /// <summary>Certicate password.</summary>
    public static readonly string CertificatePassword = "VAULT_CLIENT_CERT_PASSWORD";
}
