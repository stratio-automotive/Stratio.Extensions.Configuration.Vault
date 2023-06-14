using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Stratio.Extensions.Configuration.Vault.Configuration;

/// <summary>The class that is responsible for checking placeholders.</summary>
public class VaultCheckPlaceholders 
{
    /// <summary> The local configuration file.</summary>
    private readonly IConfigurationRoot _localConfiguration;

    /// <summary>Initializes the <see cref="VaultCheckPlaceholders" /> object.</summary>
    /// <param name="localConfiguration">The local configuration file.</param>
    /// <exception cref="ArgumentNullException">The exception thrown if the local configuration variable is null.</exception>
    public VaultCheckPlaceholders(IConfigurationRoot localConfiguration)
    {
        if (localConfiguration is null)
        {
            throw new ArgumentNullException(nameof(localConfiguration) + " variable should be assigned");
        }
        _localConfiguration = localConfiguration;
    }

    /// <summary>Loops through each placeholder section in search of placeholders.</summary>
    /// <returns>True if placeholders were found and False otherwise.</returns>
    public bool HasPlaceholders()
    {
        foreach (var section in _localConfiguration.GetChildren())
        {
            // If the section has a placeholder returns true and stops the execution
            if (SectionHasPlaceholders(section))
            {
                return true;
            }
        }

        // If it reaches here it's because the file didn't had any placeholders
        return false;   
    }

    /// <summary>Searches specific section and its subsections for placeholders. Stops at first found.</summary>
    /// <returns>True if it found placeholders in section and False otherwise.</returns>
    private static bool SectionHasPlaceholders(IConfigurationSection section)
    {
        var children = section.GetChildren().ToList();

        if (!children.Any())
        {
            // If any section is empty it should just continue working
            if (string.IsNullOrEmpty(section.Value)) 
            {
                return false;
            }

            // Extracts the placeholders to an array. Expected format of string: "{% <type> <key> %}"
            var placeholderPattern = new Regex(@"{%[^(%})]*%}", RegexOptions.None, TimeSpan.FromMilliseconds(1000)); 
            return placeholderPattern.Matches(section.Value).Select(match => match.Value).Any();
        }

        // Searches the subsections
        foreach (var subSection in children)
        {
            if (SectionHasPlaceholders(subSection))
            {
                return true;
            }
        }

        return false;
    }
}
