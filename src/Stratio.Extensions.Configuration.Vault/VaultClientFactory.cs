using Serilog;
using Stratio.Extensions.Configuration.Vault.Configuration;
using Stratio.Extensions.Configuration.Vault.Exceptions;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.Cert;
using System.Security.Cryptography.X509Certificates;

namespace Stratio.Extensions.Configuration.Vault;

/// <summary>The Vault's Client Factory class.</summary>
internal static class VaultClientFactory
{
    /// <summary>Creates Vault Client either by authentication via K8s or via AppRole.</summary>
    /// <param name="vaultConfig">The Vault Configuration that comes from appsettings.</param>
    /// <returns>The Vault client.</returns>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault.</exception>
    internal static VaultClient CreateVaultClient(VaultConfiguration vaultConfig)
    {
        // Tries to authenticate via Kubernetes
        var kubernetesAuthResult = TryAuthenticateViaKubernetes(vaultConfig, out var vault);
        if (kubernetesAuthResult.Successful && vault != null)
        {
            return vault;
        }

        // Tries to authenticate via AppRole
        var approleAuthResult = TryAuthenticateViaAppRole(vaultConfig, out vault);
        if (approleAuthResult.Successful && vault != null)
        {
            return vault;
        }

        // Tries to authenticate via Certificate
        var certAuthResult = TryAuthenticateViaCertificate(vaultConfig, out vault);
        if (certAuthResult.Successful && vault != null)
        {
            return vault;
        }

        Log.Logger.Error("Could not connect to Vault to retrieve configuration. One of the following reasons applies:" +
                         $"\n{kubernetesAuthResult.FailureReason}" +
                         $"\n{approleAuthResult.FailureReason}");

        throw new VaultConfigurationSourceException("Unable to connect to vault to retrieve configuration.");
    }

    /// <summary>Authenticates against Vault using AppRole method.</summary>
    /// <param name="vaultConfig">The Vault Configuration loaded from the settings file.</summary>
    /// <param name="vaultClient">The Vault Client used to connect to Vault.</param>
    /// <returns>The result of the Vault authentication</returns>
    private static VaultAuthenticationResult TryAuthenticateViaAppRole(VaultConfiguration vaultConfig, out VaultClient? vaultClient)
    {
        vaultClient = null;
        
        // If any of the Vault Config variables is empty or doesn't exist connection to vault will fail
        if (string.IsNullOrEmpty(vaultConfig.VaultAddr) || 
            string.IsNullOrEmpty(vaultConfig.RoleIdPath) || 
            string.IsNullOrEmpty(vaultConfig.SecretIdPath))
        {
            return new VaultAuthenticationResult
            {
                Successful = false,
                FailureReason = $"AppRole Authentication Failed: vars VaultAddress," +
                                $"AppRoleRoleIdPath and AppRoleSecretIdPath must be set either as env vars of in appsettings file."
            };
        }

        // Checks if the files provided for AppRole ID and AppRole Secret ID, indeed exist.
        if (!File.Exists(vaultConfig.RoleIdPath) || !File.Exists(vaultConfig.SecretIdPath))
        {

            return new VaultAuthenticationResult
            {
                Successful = false, FailureReason = "AppRole auth: Either Secret or Role ID file does not exist"
            };
        }

        var roleId = File.ReadAllText(vaultConfig.RoleIdPath).Trim();
        var token = File.ReadAllText(vaultConfig.SecretIdPath).Trim();

        // Load Approle authentication method name
        const string defaultApproleAuthName = "approle";
        var approleAuthName = string.IsNullOrEmpty(vaultConfig.AppRoleAuthName) ?
            defaultApproleAuthName : vaultConfig.AppRoleAuthName;

        // Sets auth method and client settings
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(approleAuthName, roleId, token);
        var vaultClientSettings = new VaultClientSettings(vaultConfig.VaultAddr, authMethod);

        if (vaultConfig.VaultSkipVerify == "true")
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpClientHandler.ServerCertificateCustomValidationCallback = 
                (_, _, _, _) =>
            {
                return true;
            };
            vaultClientSettings.MyHttpClientProviderFunc = handler => new HttpClient(httpClientHandler);
        }

        // Constructs the Vault Client
        vaultClient = new VaultClient(vaultClientSettings);

        // Returns success for Vault authentication
        return new VaultAuthenticationResult {Successful = true};
    }

    /// <summary> 
    /// Fetches a key-value secret (kv-v2) after authenticating to Vault with a Kubernetes service account.
    /// For a more in-depth setup explanation, please see the relevant readme in the hashicorp/vault-examples repo.
    /// </summary>
    /// <param name="vaultConfig">The Vault Configuration loaded from the settings file.</summary>
    /// <param name="vaultClient">The Vault Client used to connect to Vault.</param>
    /// <returns>The result of the Vault authentication</returns>
    private static VaultAuthenticationResult TryAuthenticateViaKubernetes(VaultConfiguration vaultConfig, out VaultClient? vaultClient)
    {
        vaultClient = null;

        // Will fail if vault addr or k8s role is not set
        if (string.IsNullOrEmpty(vaultConfig.VaultAddr) || string.IsNullOrEmpty(vaultConfig.KubernetesSaRoleName))
        {
            return new VaultAuthenticationResult
            {
                Successful = false,
                FailureReason = $"Kubernetes Authentication Failed: vars VaultAddress and KubernetesSaRoleName" +
                                " must be set as env vars or in appsettings file."
            };
        }

        // Load service account token
        const string defaultTokenPath = "/var/run/secrets/kubernetes.io/serviceaccount/token";
        var pathToToken = string.IsNullOrEmpty(vaultConfig.KubernetesSaTokenPath) ? 
            defaultTokenPath : vaultConfig.KubernetesSaTokenPath;

        // Load K8s authentication method name
        const string defaultK8sAuthName = "kubernetes";
        var k8sAuthName = string.IsNullOrEmpty(vaultConfig.KubernetesAuthName) ?
            defaultK8sAuthName : vaultConfig.KubernetesAuthName;

        // Check if token file really exists
        if (!File.Exists(pathToToken))
        {
            return new VaultAuthenticationResult
            {
                Successful = false, FailureReason = $"Kubernetes auth: Unable to find the Service Account token file at {pathToToken}"
            };
        }

        var jwt = File.ReadAllText(pathToToken);

        // Authenticate against vault
        var authMethod = new KubernetesAuthMethodInfo(k8sAuthName, vaultConfig.KubernetesSaRoleName, jwt);
        var vaultClientSettings = new VaultClientSettings(vaultConfig.VaultAddr, authMethod);

        if (vaultConfig.VaultSkipVerify == "true")
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpClientHandler.ServerCertificateCustomValidationCallback = 
                (_, _, _, _) =>
            {
                return true;
            };
            vaultClientSettings.MyHttpClientProviderFunc = handler => new HttpClient(httpClientHandler);
        }

        vaultClient = new VaultClient(vaultClientSettings);

        //Returns success fo authentication
        return new VaultAuthenticationResult {Successful = true};
    }

    /// <summary> 
    /// For fetching a key-value secret (kv-v2) after authenticating to Vault with a Certificate (TLS).
    /// For a more in-depth setup explanation, please see the relevant readme in the hashicorp/vault-examples repo.
    /// </summary>
    /// <param name="vaultConfig">The Vault Configuration loaded from the settings file.</summary>
    /// <param name="vaultClient">The Vault Client used to connect to Vault.</param>
    /// <returns>The result of the Vault authentication</returns>
    private static VaultAuthenticationResult TryAuthenticateViaCertificate(VaultConfiguration vaultConfig, out VaultClient? vaultClient)
    {
        vaultClient = null;

        // Will fail if vault addr, certificate password or certificate role are not set
        if (
            string.IsNullOrEmpty(vaultConfig.VaultAddr) || 
            string.IsNullOrEmpty(vaultConfig.CertificatePassword) ||
            string.IsNullOrEmpty(vaultConfig.CertificateRoleName))
        {
            return new VaultAuthenticationResult
            {
                Successful = false,
                FailureReason = $"Certificate Authentication Failed: vars VaultAddress, Certificate Password" +
                                " and Certificate role name must be set as env vars or in appsettings file."
            };
        }

        // Check if token file really exists
        if (!File.Exists(vaultConfig.CertificatePath))
        {
            return new VaultAuthenticationResult
            {
                Successful = false, FailureReason = $"Certificate auth: Unable to find the Certificate at {vaultConfig.CertificatePath}"
            };
        }

        var certificateBytes = File.ReadAllBytes(vaultConfig.CertificatePath);
        var certificate = new X509Certificate2(certificateBytes, vaultConfig.CertificatePassword);

        IAuthMethodInfo authMethod = new CertAuthMethodInfo(certificate, vaultConfig.CertificateRoleName);

        var vaultClientSettings = new VaultClientSettings(vaultConfig.VaultAddr, authMethod);

        if (vaultConfig.VaultSkipVerify == "true")
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpClientHandler.ServerCertificateCustomValidationCallback = 
                (_, _, _, _) =>
            {
                return true;
            };
            vaultClientSettings.MyHttpClientProviderFunc = handler => new HttpClient(httpClientHandler);
        }

        vaultClient = new VaultClient(vaultClientSettings);

        //Returns success fo authentication
        return new VaultAuthenticationResult {Successful = true};
    }
}
