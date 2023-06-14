using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Stratio.Extensions.Configuration.Vault.Extensions;
using Stratio.Extensions.Configuration.Vault.Exceptions;
using Stratio.Extensions.Configuration.Vault.Tests.Utils;
using System;

namespace Stratio.Extensions.Configuration.Vault.Tests;

public class NegativeTests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Objective: Raise exception from having a broken syntax in the appsettings
    /// Output: InvalidDataException
    /// Uses: 
    /// - appsettings with broken syntax
    /// - No injector
    /// </summary>
    [Test]
    public void TestABrokenAppSettingsFile()
    {

        // Clearout all vault environment variables
        VaultConfigurationEnvInjector.DisableVaultEnvConfiguration();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.BrokenSyntax.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<InvalidDataException>();

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a non-existing secret field
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with non-existing secret
    /// - injector with approle configuration
    /// </summary>
    [Test]
    public void TestNonExistingSecret()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NonExistingSecret.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to load secret from *");
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a non-existing secret dict
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with non-existing secret
    /// - injector with approle configuration
    /// </summary>
    [Test]
    public void TestNonExistingDict()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NonExistingDict.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to load dictionary from *");
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret where the placeholder was malformed
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with broken secret and vault connection
    /// - injector with approle configuration
    /// </summary>
    [Test]
    public void TestBrokenSecret()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.BrokenSecret.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Could not parse placeholder secret key*");
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret with a non-existing placeholder type
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with non-existing placeholder
    /// - injector with approle configuration
    /// </summary>
    [Test]
    public void TestPlaceholderType()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NonExistingPlaceholderType.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unknown secret type*in placeholder*");
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret with a malformed placeholder
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with malformed placeholder
    /// - injector with approle configuration
    /// </summary>
    [Test]
    public void TestBrokenSecret2()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.BrokenSecret2.json");
                }
            )
            .UseVault();

        // As configuration file is broken should throw an exception
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Could not parse the content of placeholder*");
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret without any information about Vault
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder but no Vault configuration
    /// - No environment variables related to Vault
    /// </summary>
    [Test]
    public void TestSettingsFileWithoutAnythingAboutVaultWithSecrets()
    {

        // Clearout all Vault related environment variables
        VaultConfigurationEnvInjector.DisableVaultEnvConfiguration();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVault.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Placeholders were found but the Vault configuration is not complete.");

    }

}
