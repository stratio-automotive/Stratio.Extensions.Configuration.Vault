using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Stratio.Extensions.Configuration.Vault.Extensions;
using Stratio.Extensions.Configuration.Vault.Tests.Utils;

namespace Stratio.Extensions.Configuration.Vault.Tests;

public class PositiveTests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Objective: Fetch an existing secret using valid Vault configuration
    /// Output: Matching Password
    /// Uses: 
    /// - appsettings with valid Vault address and mountpoint
    /// - injector with AppRole Access
    /// - valid vault_secret field
    /// </summary>
    [Test]
    public void TestExistingSecretField()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault()
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        string mssqlpassword = config["Secrets:mssqlpassword"];
        mssqlpassword.Should().Be("password123", "The password we loaded into vault is password123 and should match the loaded one");

    }

    /// <summary>
    /// Objective: Fetch an existing secret using valid Vault configuration
    /// Output: Matching Password
    /// Uses: 
    /// - appsettings with valid Vault address and mountpoint 
    /// - injector with AppRole Access with disabled certificate validation
    /// - valid vault_secret field
    /// </summary>
    [Test]
    public void TestExistingSecretFieldWithTLSCertificatesDisabled()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);
        VaultConfigurationEnvInjector.InjectVaultSkipVerify();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault()
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        string mssqlpassword = config["Secrets:mssqlpassword"];
        mssqlpassword.Should().Be("password123", "The password we loaded into vault is password123 and should match the loaded one");

    }

    /// <summary>
    /// Objective: Fetch an existing secret using valid Vault configuration
    /// Output: Matching Password
    /// Uses: 
    /// - appsettings without Vault configurations
    /// - injector with Vault configurations and AppRole Acces
    /// - valid vault_secret field
    /// </summary>
    [Test]
    public void TestExistingSecretFieldWithFullEnvVariables()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecretWithoutVault.json");
                }
            )
            .UseVault()
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        string mssqlpassword = config["Secrets:mssqlpassword"];
        mssqlpassword.Should().Be("password123", "The password we loaded into vault is password123 and should match the loaded one");

    }


    /// <summary>
    /// Objective: Fetch an existing dict using valid Vault configuration
    /// Output: Matching Passwords
    /// Uses: 
    /// - appsettings with Vault configurations
    /// - injector with AppRole Acces
    /// - valid vault_dict field
    /// </summary>
    [Test]
    public void TestExistingSecretDict()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingDict.json");
                }
            )
            .UseVault()
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        config["ConnectionStrings:ClientContexts:_default"].Should().Be("Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123", "Vault mssql/clients secret was loaded with a secret of key '_default' containing a string");
        config["ConnectionStrings:ClientContexts:client1"].Should().Be("Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123", "Vault mssql/clients secret was loaded with a secret of key 'client1' containing a string");

    }


    /// <summary>
    /// Objective: Load all types of secrets at once (vault_secret and vault_dict)
    /// Output: Matching Passwords
    /// Uses: 
    /// - appsettings with Vault configurations
    /// - injector with AppRole Acces
    /// - valid secret fields
    /// </summary>
    [Test]
    public void TestAllSecretTypes()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.AllSecretTypes.json");
                }
            )
            .UseVault()
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        config["ConnectionStrings:ServerContext"].Should().BeOfType<String>("Vault contains a series of keys that are valid in the configuration placeholders and are correctly replaced");
        config["ConnectionStrings:ServerContext"].Should().Be("Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123", "Vault contains a series of keys that are valid in the configuration placeholders and are correctly replaced");
        config["ConnectionStrings:ClientContexts:_default"].Should().Be("Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123", "Vault mssql/clients secret was loaded with a secret of key '_default' containing a string");
        config["ConnectionStrings:ClientContexts:client1"].Should().Be("Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123", "Vault mssql/clients secret was loaded with a secret of key 'client1' containing a string");

    }

    /// <summary>
    /// Objective: Load lib without throwing error when there's nothing to get from Vault
    /// Output: All good
    /// Uses: 
    /// - appsettings without Vault configurations
    /// - No injector
    /// - No secrets
    /// </summary>
    [Test]
    public void TestSettingsFileWithoutAnythingAboutVault()
    {

        // Deletes all Vault Configuration environment variables
        VaultConfigurationEnvInjector.DisableVaultEnvConfiguration();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVaultNoPlaceholder.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should not throw anything
        host.Invoking(host => host.Build())
            .Should()
            .NotThrow();

    }
    
    /// <summary>
    /// Objective: Loading lib without throwing error when there's no placeholder but connection to vault
    /// Output: All good
    /// Uses: 
    /// - appsettings without Vault configurations
    /// - Has injector for Vault basic config and AppRole
    /// - No placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithoutPlaceholderUsingAppRole()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVaultNoPlaceholder.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .NotThrow();

    }

    /// <summary>
    /// Objective: Loading lib without throwing error when there's no placeholder but connection to vault
    /// Output: All good
    /// Uses: 
    /// - appsettings without Vault configurations
    /// - Has injector for Vault basic config and Kubernetes config
    /// - No placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithoutPlaceholderUsingKubernetes()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultKubernetesConfiguration(true);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVaultNoPlaceholder.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .NotThrow();

    }

    /// <summary>
    /// Objective: Loading lib without throwing error when there's no placeholder but connection to vault
    /// Output: All good
    /// Uses: 
    /// - appsettings without Vault configurations
    /// - Has injector for Vault basic config and Kubernetes config
    /// - No placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithoutPlaceholderUsingKubernetesWithoutSSL()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultKubernetesConfiguration(true);
        //Disables validation of Certificates
        VaultConfigurationEnvInjector.InjectVaultSkipVerify();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVaultNoPlaceholder.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .NotThrow();

    }

    /// <summary>
    /// Objective: Loading lib without throwing error when using AppRole with a different path
    /// Output: All good
    /// Uses: 
    /// - appsettings full with configurations
    /// - No Injector
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingAppRoleWithDiffPath()
    {

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.FullSettingsAppRole.json");
                }
            )
            .UseVault()
            .Build();


        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        string mssqlpassword = config["Secrets:mssqlpassword"];
        mssqlpassword.Should().Be("password123", "The password we loaded into vault is password123 and should match the loaded one");

    }

    /// <summary>
    /// Objective: Loading lib without throwing error when using AppRole with a different path from Env Var
    /// Output: All good
    /// Uses: 
    /// - appsettings full with configurations
    /// - Has injector for AppRolePath
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingAppRoleWithDiffPathFromEnvironment()
    {

        VaultConfigurationEnvInjector.InjectVaultAppRolePathConfiguration();

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.FullVaultSettings.json");
                }
            )
            .UseVault()
            .Build();


        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Tries to match secret with parsed file
        string mssqlpassword = config["Secrets:mssqlpassword"];
        mssqlpassword.Should().Be("password123", "The password we loaded into vault is password123 and should match the loaded one");

    }

}
