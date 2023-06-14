
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Stratio.Extensions.Configuration.Vault.Constants;
using Stratio.Extensions.Configuration.Vault.Exceptions;

namespace Stratio.Extensions.Configuration.Vault.Configuration;

/// <summary>The class that contains the Vault's configuration.</summary>
public class VaultConfiguration 
{
    /// <summary>Address of the Vault server.</summary>
    public string VaultAddr { get; set; }
    /// <summary>Vault's mountpoint that should be acessed.</summary>
    public string MountPoint { get; set; }
    /// <summary>Validate or not the Vault's TLS Certificate.</summary>
    public string VaultSkipVerify { get; set; }

    /// <summary>AppRole authentication method name.</summary>
    public string AppRoleAuthName { get; set; }
    /// <summary>AppRole authentication role ID path.</summary>
    public string RoleIdPath { get; set; }
    /// <summary>AppRole authentication secret ID path.</summary>
    public string SecretIdPath { get; set; }

    /// <summary>Kubernetes authentication method name.</summary>
    public string KubernetesAuthName { get; set; }
    /// <summary>Kubernetes authentication role name.</summary>
    public string KubernetesSaRoleName { get; set; }
    /// <summary>Kuberntes authenatication service account token path.</summary>
    public string KubernetesSaTokenPath { get; set; }
    
    /// <summary>Certificate path.</summary>
    public string CertificatePath { get; set; }
    /// <summary>Certificate name.</summary>
    public string CertificateRoleName { get; set; }
    /// <summary>Certicate password.</summary>
    public string CertificatePassword { get; set; }
 
    ///<summary>Initializes the <see cref="VaultConfiguration"/> object.</summary>
    ///<param name="vaultConfig">The section from the appsettings file regarding vault configurations.</param>
    public VaultConfiguration(IConfiguration vaultConfig)
    {
        // Loads all environment variables it can find
        var env_vaultAddr = Environment.GetEnvironmentVariable(EnvironmentVariables.VaultAddress);
        var env_mountPoint = Environment.GetEnvironmentVariable(EnvironmentVariables.VaultMountPoint);
        var env_vaultSkipVerify = Environment.GetEnvironmentVariable(EnvironmentVariables.VaultSkipVerify);

        var env_roleIdPath = Environment.GetEnvironmentVariable(EnvironmentVariables.AppRoleRoleIdPath);
        var env_secretIdPath = Environment.GetEnvironmentVariable(EnvironmentVariables.AppRoleSecretIdPath);
        var env_approleAuthName = Environment.GetEnvironmentVariable(EnvironmentVariables.AppRoleAuthName);
        
        var env_kubernetesSaRoleName = Environment.GetEnvironmentVariable(EnvironmentVariables.KubernetesSaRoleName);
        var env_kubernetesAuthName = Environment.GetEnvironmentVariable(EnvironmentVariables.KubernetesAuthName);
        var env_kubernetesSaTokenPath = Environment.GetEnvironmentVariable(EnvironmentVariables.KubernetesSaTokenPath);

        var env_certificatePath = Environment.GetEnvironmentVariable(EnvironmentVariables.CertificatePath);
        var env_certificatePassword = Environment.GetEnvironmentVariable(EnvironmentVariables.CertificatePassword);
        var env_certificateRoleName = Environment.GetEnvironmentVariable(EnvironmentVariables.CertificateRoleName);

        // Environment variables to access Vault have precedence over appsettings file definitions.
        VaultAddr = string.IsNullOrEmpty(env_vaultAddr) ? vaultConfig.GetValue<string>("vaultAddress") : env_vaultAddr;
        MountPoint = string.IsNullOrEmpty(env_mountPoint) ? vaultConfig.GetValue<string>("mountPoint") : env_mountPoint;
        VaultSkipVerify = string.IsNullOrEmpty(env_vaultSkipVerify) ? 
            vaultConfig.GetValue<string>("vaultSkipVerify") : env_vaultSkipVerify;

        // Environment variables to access Vault via Role have precedence over appsettings file definitions.
        AppRoleAuthName = string.IsNullOrEmpty(env_approleAuthName) ? vaultConfig.GetValue<string>("approleAuthName") : env_approleAuthName;
        RoleIdPath = string.IsNullOrEmpty(env_roleIdPath) ? vaultConfig.GetValue<string>("roleIdPath"): env_roleIdPath;
        SecretIdPath = string.IsNullOrEmpty(env_secretIdPath) ? vaultConfig.GetValue<string>("secretIdPath") : env_secretIdPath;

        // Environment variables to access Vault via Kubernetes have precedence over appsettings file definitions.
        KubernetesSaRoleName = string.IsNullOrEmpty(env_kubernetesSaRoleName) ? 
            vaultConfig.GetValue<string>("kubernetesSaRoleName") : env_kubernetesSaRoleName;
        KubernetesAuthName = string.IsNullOrEmpty(env_kubernetesAuthName) ? 
            vaultConfig.GetValue<string>("kubernetesAuthName") : env_kubernetesAuthName;
        KubernetesSaTokenPath = string.IsNullOrEmpty(env_kubernetesSaTokenPath) ? 
            vaultConfig.GetValue<string>("kubernetesSaTokenPath") : env_kubernetesSaTokenPath;

        // Environment variables to access Vault via Certificate.
        CertificatePath = string.IsNullOrEmpty(env_certificatePath) ? 
            vaultConfig.GetValue<string>("certificatePath") : env_certificatePath;
        CertificatePassword = string.IsNullOrEmpty(env_certificatePassword) ? 
            vaultConfig.GetValue<string>("certificatePassword") : env_certificatePassword;
        CertificateRoleName = string.IsNullOrEmpty(env_certificateRoleName) ? 
            vaultConfig.GetValue<string>("certificateRolesName") : env_certificateRoleName;

        // Validates the RoleId and Secret Id Paths and fixes it if necessary
        ValidateRoleIdAndSecretIdPaths();

        // Runs basic checks on Vault address and mountpoint
        BasicConnectionChecks();
    }

    /// <summary>Makes a basic connection validation check to see if at least vault address and mountpoint are set.</summary>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    private void BasicConnectionChecks()
    {
        if (string.IsNullOrEmpty(VaultAddr) && !string.IsNullOrEmpty(MountPoint))
        {
            throw new VaultConfigurationSourceException($"Vault address must be set, got '{VaultAddr}'");
        }

        if (!string.IsNullOrEmpty(VaultAddr) && string.IsNullOrEmpty(MountPoint))
        {
            throw new VaultConfigurationSourceException($"Vault mountpoint must be set, got '{MountPoint}'");
        }
    }

    ///<summary>Validates Role ID and Secret ID Paths that are set in the appsettings file.</summary>
    private void ValidateRoleIdAndSecretIdPaths()
    {
        // If one of the below mentioned variables is empty something is missconfigured,
        // but if both are empty it means that probably it's kubernetes authentication
        // nevertheless, validations on connection settings are made after
        if (string.IsNullOrEmpty(RoleIdPath) || string.IsNullOrEmpty(SecretIdPath))
        {
            return;
        }   

        /// The pattern to search is {% user_home %}
        const string needle = "{% " + PlaceholderTypes.UserHome + " %}";

        // If found in RoleIdPath
        if (RoleIdPath.IndexOf(needle, StringComparison.OrdinalIgnoreCase) != -1)
        {
            //Fix folder where RoleIdPath is
            RoleIdPath = FixHomeDirPath(RoleIdPath.Replace(needle, ""));
        }
        
        // If found in SecretIdPath
        if (SecretIdPath.IndexOf(needle, StringComparison.OrdinalIgnoreCase) != -1)
        {
            //Fix folder where RoleIdPath is
            SecretIdPath = FixHomeDirPath(SecretIdPath.Replace(needle, ""));
        }
    }

    ///<summary>Fixes the path to the home folder.</summary>
    ///<param name="filepath">The path to be fixed.</param>
    ///<returns>Returns the fixed path to home folder.</returns>
    private static string FixHomeDirPath(string filepath)
    {
        string home = "";

        // Sets the home folder depending on running Operative System
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            home = Environment.GetEnvironmentVariable("HOME");
        }
        else
        {
            home = Environment.GetEnvironmentVariable("USERPROFILE");
        }

        // Constructs the final home dir path and return it
        if (filepath.StartsWith(Path.DirectorySeparatorChar))
        {
            return home + filepath;
        } 
        else
        {
            return home + Path.DirectorySeparatorChar + filepath;
        }
    }
}
