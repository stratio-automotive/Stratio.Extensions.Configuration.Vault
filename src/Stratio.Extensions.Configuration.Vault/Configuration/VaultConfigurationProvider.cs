using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Stratio.Extensions.Configuration.Vault.Constants;
using Stratio.Extensions.Configuration.Vault.Exceptions;
using VaultSharp.Core;

namespace Stratio.Extensions.Configuration.Vault.Configuration;

/// <summary>The class that gets and transforms secrets.</summary>
internal class VaultConfigurationProvider : ConfigurationProvider
{
    /// <summary>The Vault's configuration.</summary>
    private readonly VaultConfigurationSource _source;
    /// <summary>The sections from the configuration where to get secrets.</summary>
    private readonly IConfiguration _config;

    /// <summary>Initializes a new instance of the <see cref="VaultConfigurationProvider" />.</summary>
    /// <param name="source">The Vault's configuration.</param>
    /// <param name="config">The sections from the configuration where to get secrets.</param>
    public VaultConfigurationProvider(VaultConfigurationSource source, IConfiguration config)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Loads (or reloads) the data for the provider.</summary>
    public override void Load()
    {
        foreach (var section in _config.GetChildren())
        {
            ReplacePlaceholdersInSection(section);
        }
    }

    /// <summary>Finds the placeholder to materialize in a string.</summary>
    /// <param name="configurationEntry">The configuration section to be scanned.</param>
    /// <returns>List of strings containing the placeholders found in the provided section.</returns>
    private static List<string> GetPlaceholdersInString(string configurationEntry)
    {
                                            // Expected format: "{% <type> <key> %}"
        var placeholderPattern = new Regex(@"{%[^(%})]*%}", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
        return placeholderPattern.Matches(configurationEntry).Select(match => match.Value).ToList();
    }

    /// <summary>Based on a specific key, loads all secrets from given path in Vault and returns a Dictionary.</summary>
    /// <param name="secretKey">The string containing the secret key to be searched for. Format: [PATH_TO_SECRET].</param>
    /// <returns>Dictionary with the secrets fecthed from Vault.</returns>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    private Dictionary<string, string> GetVaultDict(string secretKey)
    {
        try 
        {
            // Gets Vault secret as dictionary
            return _source.GetVaultSecretAsDict(secretKey);
        } 
        catch (Exception ex) when (ex is SocketException || 
                                    ex is HttpRequestException)
        {
            // If vault is not acessible or badly configured will throw this exception
            throw new VaultConfigurationSourceException($"Unable to connect to Vault", ex);
        }
        catch (Exception ex) when (ex.InnerException?.InnerException is VaultApiException vaultException && 
                                    vaultException.Message.Contains("permission denied"))
        {
            throw new VaultConfigurationSourceException($"Access to vault was denied, is the mountpoint correctly configured?", ex);
        }
        catch (Exception ex)
        {
            // If Secret does not exist, will throw following exception
            throw new VaultConfigurationSourceException($"Unable to load dictionary from vault corresponding to key '{secretKey}'", ex);
        }
    }

    /// <summary>Based on given Key will load secret from Vault and returns it.</summary>
    /// <param name="secretKey">The string containing the secret key to be searched for. Format: [PATH_TO_SECRET]:[FIELD].</param>
    /// <returns>String containing the secret fecthed from Vault.</returns>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    private string GetVaultSecret(string secretKey)
    {
        var secretPathAndField = secretKey.Split(":"); // Expected format: "<path>:<field>"

        // If size doesn't match, should thrown an error
        if (secretPathAndField.Length != 2)
        {
            throw new VaultConfigurationSourceException($"Could not parse placeholder secret key '{secretKey}'");
        }

        // Has the path to the secret
        var secretPath = secretPathAndField[0];
        // Has the key of the secret associated with the previous path
        var secretField = secretPathAndField[1];

        try 
        {
            // Tries to get Secret field from Vault
            return _source.GetVaultSecretField(secretPath, secretField);
        } 
        catch (Exception ex) when (ex is SocketException || ex is HttpRequestException)
        {
            // If vault is not acessible or badly configured will throw this exception
            throw new VaultConfigurationSourceException($"Unable to connect to Vault", ex);
        }
        catch (Exception ex) when 
        (ex.InnerException?.InnerException is VaultApiException vaultException && vaultException.Message.Contains("permission denied"))
        {
            throw new VaultConfigurationSourceException($"Access to vault was denied, is the mountpoint correctly configured?", ex);
        }
        catch (Exception ex)
        {
            // If Secret does not exist, will throw following exception
            throw new VaultConfigurationSourceException($"Unable to load secret from vault corresponding to key '{secretKey}'", ex);
        }
    }

    /// <summary>Finds out the type of secret to be loaded (vault_dict, vault_secret) and loads it from Vault.</summary>
    /// <param name="section">Section of the appsettings file where to search for secrets.</param>
    /// <exception cref="VaultConfigurationSourceException">The exception thrown if it can't connect to Vault or find secret.</exception>
    private void MaterializePlaceholdersInSection(IConfigurationSection section)
    {
        // If any section is empty it should just continue working
        if (String.IsNullOrEmpty(section.Value)) {
            return;
        }

        // Extracts the placeholders to an array
        var placeholders = GetPlaceholdersInString(section.Value);

        // Loops through each placeholder present in string
        foreach (var placeholder in placeholders)
        {
            // If the placeholder is UserHome then it's not supposed to go through Vault, skip it
            if (placeholder.IndexOf(PlaceholderTypes.UserHome, StringComparison.OrdinalIgnoreCase) != -1) {
                continue;
            }

            // Tries to find out the placeholder
            if (!TryGetPlaceholderTypeAndKey(placeholder, out var secretType, out var secretKey))
            {
                // If there's something wrong with the placeholder, will through the exception below
                throw new VaultConfigurationSourceException(
                    $"Could not parse the content of placeholder {placeholder} in config section {section.Path}");
            }

            // Checks of the identified placeholder matches any of the available options
            switch (secretType)
            {
                // Case placeholder is 'vault_secret'
                case PlaceholderTypes.VaultSecret:
                {
                    //Gets secret from Vault
                    var secret = GetVaultSecret(secretKey);

                    if (!Data.ContainsKey(section.Path))
                    {
                        Data[section.Path] = section.Value;
                    }

                    // Replaces placeholder by secret retrieved in Vault
                    Data[section.Path] = Data[section.Path].Replace(placeholder, secret);

                    break;
                }

                // Case placeholder is 'vault_dict'
                case PlaceholderTypes.VaultDict:
                {
                    //Gets secret from Vault
                    var secrets = GetVaultDict(secretKey);

                    // Creates array with retrieved secrets
                    foreach (var secret in secrets)
                    {
                        Data[$"{section.Path}:{secret.Key}"] = secret.Value;
                    }

                    break;
                }
                default:
                {
                    // When placeholder is not recognized
                    throw new VaultConfigurationSourceException($"Unknown secret type '{secretType}' in placeholder '{placeholder}'");
                }
            }
        }
    }

    /// <summary>Replaces section placeholders with the secrets.</summary>
    /// <param name="section">The Section where to materialize placeholders.</param>
    private void ReplacePlaceholdersInSection(IConfigurationSection section)
    {
        var children = section.GetChildren().ToList();

        if (!children.Any())
        {
            MaterializePlaceholdersInSection(section);
            return;
        }

        foreach (var subSection in children)
        {
            ReplacePlaceholdersInSection(subSection);
        }
    }

    /// <summary>Tries to get placeholder and the type of placeholder.</summary>
    /// <param name="placeholder">Is the placeholder that was found in the configuration.</param>
    /// <param name="type">Contains the type of the placeholder.</param>
    /// <param name="key">Contains the key to search for in Vault.</param>
    /// <returns>True if a match was found, false otherwise</returns>
    private static bool TryGetPlaceholderTypeAndKey(string placeholder, out string type, out string key)
    {
        var fieldsPattern = new Regex(@"[^\s{%}]+", RegexOptions.None, TimeSpan.FromMilliseconds(1000));

        var matches = fieldsPattern.Matches(placeholder);

        // Placeholder and type must be a set of 2 strings (placeholder<space>secret)
        if (matches.Count != 2)
        {
            type = key = string.Empty;
            return false;
        }

        // Has type of secret (field or dict)
        type = matches[0].Value;
        // Has secret to be fetched
        key = matches[1].Value;

        return true;
    }
}
