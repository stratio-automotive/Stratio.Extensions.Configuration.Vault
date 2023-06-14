using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Stratio.Extensions.Configuration.Vault.Tests.Utils;

internal static class VaultConfigurationEnvInjector
{

    /// <summary>
    /// Injects basic configuration for acessing Vault
    /// </summary>
    public static void InjectBasicVaultConfiguration()
    {
        //Firstly clean existing configuration
        DisableBasicVaultConfiguration();

        Environment.SetEnvironmentVariable("VAULT_ADDR", "http://127.0.0.1:8200");
        Environment.SetEnvironmentVariable("VAULT_MOUNTPOINT", "app/myorg");
    }

    /// <summary>
    /// Disables basic configuration for acessing Vault
    /// </summary>
    public static void DisableBasicVaultConfiguration()
    {
        Environment.SetEnvironmentVariable("VAULT_ADDR", null);
        Environment.SetEnvironmentVariable("VAULT_MOUNTPOINT", null);
    }

    /// <summary> 
    /// Injects Vault AppRole Configuration
    /// <param name="injectBasicVault">Defines if it should also inject basic Vault connection configurations</param>
    /// </summary>
    public static void InjectVaultAppRoleConfiguration(bool injectBasicVault)
    {

        string home = "";
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            home = Environment.GetEnvironmentVariable("HOME");
        }
        else
        {
            home = Environment.GetEnvironmentVariable("USERPROFILE");
        }

        // Read vault configuration from files
        var roleIdPath = home + Path.DirectorySeparatorChar + ".myorg/secrets/approle.role_id";
        var secretIdPath = home + Path.DirectorySeparatorChar + ".myorg/secrets/approle.secret_id";

        //Firstly clean existing configuration
        DisableVaultEnvConfiguration();

        // Inject vault basic configuration
        if (injectBasicVault) {
            InjectBasicVaultConfiguration();
        }

        Environment.SetEnvironmentVariable("APPROLE_ROLE_ID_PATH", roleIdPath);
        Environment.SetEnvironmentVariable("APPROLE_SECRET_ID_PATH", secretIdPath);
    }

    /// <summary> 
    /// Injects Vault AppRole Path Configuration
    /// <param name="injectBasicVault">Defines if it should also inject basic Vault connection configurations</param>
    /// </summary>
    public static void InjectVaultAppRolePathConfiguration()
    {
        Environment.SetEnvironmentVariable("APPROLE_AUTH_NAME", "approle");
    }

    /// <summary> 
    /// Disables Vault AppRole Configuration
    /// </summary>
    public static void DisableVaultAppRoleConfiguration()
    {
        Environment.SetEnvironmentVariable("APPROLE_ROLE_ID_PATH", null);
        Environment.SetEnvironmentVariable("APPROLE_SECRET_ID_PATH", null);
    }

    /// <summary> 
    /// Injects Vault Kubernetes Configuration
    /// <param name="injectBasicVault">Defines if it should also inject basic Vault connection configurations</param>
    /// </summary>
    public static void InjectVaultKubernetesConfiguration(bool injectBasicVault)
    {
        //Firstly clean existing configuration
        DisableVaultEnvConfiguration();

        // Inject vault basic configuration
        if (injectBasicVault) {
            InjectBasicVaultConfiguration();
        }        
        Environment.SetEnvironmentVariable("VAULT_ROLE", "kubernetes-ro");
        // Since we're not testing in a K8s environment we can't use a real token path
        // Can only be used when no placeholder needs to be replaced, otherwise the connection will fail
        Environment.SetEnvironmentVariable("SA_TOKEN_PATH", "My_TOKEN");
    }

    /// <summary> 
    /// Disables Vault Kubernetes Configuration
    /// </summary>
    public static void DisableVaultKubernetesConfiguration()
    {
        Environment.SetEnvironmentVariable("VAULT_ROLE", null);
        Environment.SetEnvironmentVariable("SA_TOKEN_PATH", null);
    }

    /// <summary> 
    /// Injects Vault Certificates Configuration
    /// <param name="injectBasicVault">Defines if it should also inject basic Vault connection configurations</param>
    /// </summary>
    public static void InjectVaultCertificatesConfiguration(bool injectBasicVault)
    {

        //Firstly clean existing configuration
        DisableVaultEnvConfiguration();

        // Inject vault basic configuration
        if (injectBasicVault) {
            InjectBasicVaultConfiguration();
        }        
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT", "Cert.p12");
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT_PASSWORD", "stratio123");
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT_NAME", "test-machine");
    }

    /// <summary> 
    /// Disables Vault Certificate Configuration
    /// </summary>
    public static void DisableVaultCertificatesConfiguration()
    {
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT", null);
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT_PASSWORD", null);
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT_NAME", null);
    }

    /// <summary>
    /// Injects Vault Skip Verify Variable
    /// </summary>
    public static void InjectVaultSkipVerify()
    {
        Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", "true");
    }

    /// <summary>
    /// Disables Vault Skip Verify Variable
    /// </summary>
    public static void DisableVaultSkipVerify()
    {
        Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", null);
    }

    /// <summary> 
    /// Disables all Vault Configurations
    /// </summary>
    public static void DisableVaultEnvConfiguration()
    {
        DisableBasicVaultConfiguration();
        DisableVaultAppRoleConfiguration();
        DisableVaultKubernetesConfiguration();
        DisableVaultCertificatesConfiguration();
        DisableVaultSkipVerify();
    }
}
