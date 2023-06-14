namespace Stratio.Extensions.Configuration.Vault.Exceptions;

/// <summary>The class that manages the Exceptions.</summary>
public class VaultConfigurationSourceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VaultConfigurationSourceException"/> class.
    ///</summary>
    public VaultConfigurationSourceException() : base("Error loading configuration from Vault.")
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultConfigurationSourceException"/> class.
    /// </summary>
    public VaultConfigurationSourceException(string message) : base(message)
    {
        
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultConfigurationSourceException"/> class.
    /// <param name="cause">The Exception that triggered the function</param>
    /// </summary>
    public VaultConfigurationSourceException(string message, Exception cause) : base(message, cause)
    {
        
    }
}
