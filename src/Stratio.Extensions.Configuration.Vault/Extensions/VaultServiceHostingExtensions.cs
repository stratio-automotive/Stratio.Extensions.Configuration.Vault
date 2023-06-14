using Microsoft.Extensions.Hosting;
using Stratio.Extensions.Configuration.Vault.Configuration;
using Stratio.Extensions.Configuration.Vault.Exceptions;

namespace Stratio.Extensions.Configuration.Vault.Extensions;

/// <summary>
/// Extension of the IHostBuilder to include in its API the ability to load secrets from a vault instance.
/// </summary>
public static class VaultServiceHostingExtensions
{
    /// <summary>
    /// Loads configuration from a vault that is configured via environment variables and local files.
    /// All placeholders in the configuration are replaced by values from vault.
    /// Placeholders look like this: <c>{% &lt;type&gt; &lt;key&gt; %}</c>
    /// </summary>
    /// <param name="builder">The host builder</param>
    /// <returns>The same host builder</returns>
    /// <remarks>
    /// Only settings configured in <c>ConfigureHostConfiguration</c> have their placeholders replaced.
    /// Placeholders in settings loaded during <c>ConfigureAppConfiguration</c> <b>are not replaced</b>.
    /// </remarks>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault.</exception>
    public static IHostBuilder UseVault(this IHostBuilder builder)
    {
        builder
            .ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                // Creates configuration builder build instance
                var localConfiguration = configurationBuilder.Build();
                // Loads Vault Configurations
                var vaultConfig = new VaultConfiguration(localConfiguration.GetSection("Vault"));
                // Creates the vault placeholder checker object
                var vaultCheckPlaceholders = new VaultCheckPlaceholders(localConfiguration);

                // Continue only if 
                // VaultAddr is set AND
                // AppRople is completely set OR
                // Kubernetes is completely set
                bool approleSet = !string.IsNullOrEmpty(vaultConfig.RoleIdPath) && !string.IsNullOrEmpty(vaultConfig.SecretIdPath);
                bool kubernetesSet = !string.IsNullOrEmpty(vaultConfig.KubernetesSaRoleName);
                bool certificateSet = !string.IsNullOrEmpty(vaultConfig.CertificatePath) && 
                                        !string.IsNullOrEmpty(vaultConfig.CertificatePassword) && 
                                        !string.IsNullOrEmpty(vaultConfig.CertificateRoleName);
                if (!string.IsNullOrEmpty(vaultConfig.VaultAddr) && (approleSet || kubernetesSet || certificateSet))
                {
                    // Searches if appsettings file has any placeholders
                    if (vaultCheckPlaceholders.HasPlaceholders())
                    {                    
                        // Creates the Vault Client
                        var vaultClient = VaultClientFactory.CreateVaultClient(vaultConfig);
                        
                        // Creates Vault source
                        var vaultSource = new VaultConfigurationSource(vaultClient, localConfiguration, vaultConfig);
                        configurationBuilder.Sources.Add(vaultSource);
                    }
                // If one, or both, VaultAddr and RoleIDPath are not set
                } else {
                    // Searches if appsettings file has any placeholders
                    if (vaultCheckPlaceholders.HasPlaceholders())
                    {                    
                        throw new VaultConfigurationSourceException("Placeholders were found but the Vault configuration is not complete.");
                    }
                }
            });
        return builder;
    }
}
