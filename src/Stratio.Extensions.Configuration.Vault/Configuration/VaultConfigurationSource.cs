using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.Commons;
using Stratio.Extensions.Configuration.Vault.Exceptions;
using VaultSharp.Core;

namespace Stratio.Extensions.Configuration.Vault.Configuration;

/// <summary>The class that is responsible for reading secrets from Vault</summary>
internal class VaultConfigurationSource : IConfigurationSource
{
    /// <summary>The Vault Client.</summary>
    private readonly IVaultClient _vault;
    /// <summary>The sections from the configuration where to get secrets.</summary>
    private readonly IConfiguration _config;
    /// <summary>The Vault Configuration.</summary>
    private readonly VaultConfiguration _vaultConfiguration;

    /// <summary>Initializes a new instance of the <see cref="VaultConfigurationSource" />.</summary>
    /// <param name="vault">The Vault Client.</param>
    /// <param name="config">The sections from the configuration where to get secrets.</param
    /// <param name="vaultConfig">The Vault Configuration.</param>
    public VaultConfigurationSource(IVaultClient vault, IConfiguration config, VaultConfiguration vaultConfig)
    {
        _vault = vault ?? throw new ArgumentNullException(nameof(vault));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _vaultConfiguration = vaultConfig ?? throw new ArgumentNullException(nameof(vaultConfig));
    }

    /// <summary>
    /// Builds the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder"/>.</param>
    /// <returns>An <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider"/>.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(this, _config);
    }

    /// <summary>Get's Vault secret as dictionary.</summary>
    /// <param name="path">The path to the secrets to be read from Vault.</param>
    /// <returns>The dictionary with the results of the secrets.</returns>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    public Dictionary<string, string> GetVaultSecretAsDict(string path)
    {
        try 
        {
            // Gets secret dict based on given path
            Secret<SecretData> secret = _vault.V1.Secrets.KeyValue.V2
                                                    .ReadSecretAsync(path, mountPoint: _vaultConfiguration.MountPoint).Result;

            // Converts received secret dict to a dictionary
            var dict = secret.Data.Data.ToDictionary(
                                            entry => entry.Key,
                                            entry => entry.Value.ToString()
                                        );
            return dict;
        } 
        catch (Exception ex) when (ex.InnerException is SocketException || 
                                    ex.InnerException is HttpRequestException)
        {
            throw new HttpRequestException($"Unable to connect to Vault", ex);
        } 
        catch (Exception ex) when (ex.InnerException is VaultApiException && 
                                    ex.InnerException.Message.Contains("permission denied"))
        {
            throw new VaultConfigurationSourceException($"Access to vault was denied, is the mountpoint correctly configured?", ex);
        }
        catch (Exception ex)
        {
            throw new VaultConfigurationSourceException($"Unable to get Secret as dictionary from path {path}", ex);
        }
    }

    /// <summary>Gets Vault secret as value.</summary>
    /// <param name="path">The path to the secret to be read from Vault.</param>
    /// <param name="field">The field from the secret to be read.</param>
    /// <returns>The requested value of the secret.</returns>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    public string GetVaultSecretField(string path, string field)
    {
        try 
        {
            // Gets secret field based on given path and field
            Secret<SecretData> secret = _vault.V1.Secrets.KeyValue.V2
                                                .ReadSecretAsync(path, mountPoint: _vaultConfiguration.MountPoint).Result;

            // Validates the existance of the secret
            if (!secret.Data.Data.ContainsKey(field)) {
                throw new VaultConfigurationSourceException($"Didn't find the required field {field} at vault path {path}");
            }

            return secret.Data.Data[field].ToString();
        } 
        catch (Exception ex) when (ex.InnerException is SocketException || 
                                    ex.InnerException is HttpRequestException)
        {
            throw new HttpRequestException($"Unable to connect to Vault. See inner exception for more details.", ex);
        }
        catch (Exception ex) when (ex.InnerException is VaultApiException && 
                                        ex.InnerException.Message.Contains("permission denied"))
        {
            throw new VaultConfigurationSourceException($"Access to vault was denied, is the mountpoint correctly configured?", ex);
        }
        catch (Exception ex) 
        {
            throw new VaultConfigurationSourceException($"Unable to get Secret field {field} at vault path {path}", ex);
        }
    }
}
