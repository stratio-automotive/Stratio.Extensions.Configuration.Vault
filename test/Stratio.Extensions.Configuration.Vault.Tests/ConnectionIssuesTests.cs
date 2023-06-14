using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Stratio.Extensions.Configuration.Vault.Extensions;
using Stratio.Extensions.Configuration.Vault.Exceptions;
using Stratio.Extensions.Configuration.Vault.Tests.Utils;
using System.IO;

namespace Stratio.Extensions.Configuration.Vault.Tests;

public class ConnectionIssuesTests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret field from a non-existing Vault server
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder but no Vault configuration
    /// - Environment variables pointing to wrong Vault server
    /// </summary>
    [Test]
    public void TestGetVaultSecretWhenVaultNotAvailable()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Changes the Address of the Vault server to something that doesn't exist
        Environment.SetEnvironmentVariable("VAULT_ADDR", "http://myorg:8200");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to connect to Vault*");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret field from a non-existing Vault server
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs
    /// </summary>
    [Test]
    public void TestGetVaultSecretWhenVaultNotAvailable2()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecretWrongVault.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to connect to Vault*");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a non-existing Vault server
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs
    /// </summary>
    [Test]
    public void TestGetVaultDictWhenVaultNotAvailable()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Address of the Vault server to something that doesn't exist
        Environment.SetEnvironmentVariable("VAULT_ADDR", "http://myorg:8200");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingDict.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to connect to Vault*");

    }
    
    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// valid credentials (in this case AppRole)
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with broken AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningEmptyRoleIDPath()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Approle id path
        Environment.SetEnvironmentVariable("APPROLE_ROLE_ID_PATH", null);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Placeholders were found but the Vault configuration is not complete.");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// valid credentials (in this case AppRole)
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with unexisting AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningWrongRoleIDPath()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Approle id path
        Environment.SetEnvironmentVariable("APPROLE_ROLE_ID_PATH", "wrongapproleid");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to connect to vault to retrieve configuration.");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// valid credentials (in this case AppRole)
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with unexisting AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningEmptySecretIDPath()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Secret ID path
        Environment.SetEnvironmentVariable("APPROLE_SECRET_ID_PATH", null);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Placeholders were found but the Vault configuration is not complete.");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// valid credentials (in this case AppRole)
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with wrong AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningWrongSecretIDPath()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Secret ID path
        Environment.SetEnvironmentVariable("APPROLE_SECRET_ID_PATH", "thisisawrongsecretid");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.ExistingSecret.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Unable to connect to vault to retrieve configuration.");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// valid Mountpoint
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with empty Mountpoint configs
    /// </summary>
    [Test]
    public void TestDefiningEmptyMountPoint()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Empties the Mountpoint
        Environment.SetEnvironmentVariable("VAULT_MOUNTPOINT", "");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.EmptyMountPoint.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Vault mountpoint must be set*");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server without using 
    /// any Mountpoint
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with null Mountpoint configs
    /// </summary>
    [Test]
    public void TestNotSettingMountPoint()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Deletes the Mountpoint
        Environment.SetEnvironmentVariable("VAULT_MOUNTPOINT", null);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetMountPoint.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Vault mountpoint must be set*");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret field from a Vault server using wrong 
    ///  Mountpoint
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningWrongMountPoint()
    {
        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);
        
        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.WrongMountPoint.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Access to vault was denied, is the mountpoint correctly configured?");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret dict from a Vault server using wrong 
    ///  Mountpoint
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs
    /// </summary>
    [Test]
    public void TestDefiningWrongMountPoint2()
    {
        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(false);
        
        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.WrongMountPoint2.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Access to vault was denied, is the mountpoint correctly configured?");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret without specifying a Vault server
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs but no Vault Address
    /// </summary>
    [Test]
    public void TestNoVaultAddress()
    {
    
        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);

        // Changes the Secret ID path
        Environment.SetEnvironmentVariable("VAULT_ADDR", null);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.NotSetVaultAddressWithPlaceholder.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Vault address must be set*");

    }

    /// <summary>
    /// Objective: Raise exception from trying to get a secret specifying an empty Vault server
    /// Output: VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with placeholder and Vault configuration
    /// - Environment variables with AppRole configs but empty Vault Address
    /// </summary>
    [Test]
    public void TestEmptyVaultAddress()
    {

        // Inject Vault AppRole Configuration with Vault basic config
        VaultConfigurationEnvInjector.InjectVaultAppRoleConfiguration(true);
    
        // Changes the Secret ID path
        Environment.SetEnvironmentVariable("VAULT_ADDR", "");

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.EmptyVaultAddress.json");
                }
            )
            .UseVault();

        // As Vault is not accessible should throw error
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>().WithMessage("Vault address must be set*");

    }

    /// <summary>
    /// Objective: Loading lib throwing an error when there's a try to authenticate using Kubernetes
    /// and no Kubernetes is available
    /// Output:VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with basic Vault configurations
    /// - Has injector for Vault basic config and Kubernetes config and skips TLS
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingKubernetesWithoutSSL()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultKubernetesConfiguration(true);
        //Disables validation of Certificates
        VaultConfigurationEnvInjector.InjectVaultSkipVerify();

        // IMPORTANT NOTICE: THE FILE USED AS JWT TOKEN WAS TAKEN FROM A MINISTACK
        // ENVIRONMENT, THERE'S NO HARM IN HAVING THIS PUBLISHED IN THE REPOSITORY
        string JWTTokenFilePath = Directory.GetCurrentDirectory() + "../../../../Resources/jwt/JWT.token";
        Environment.SetEnvironmentVariable("SA_TOKEN_PATH", JWTTokenFilePath);

        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.AllSecretTypes.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>();

    }

    /// <summary>
    /// Objective: Loading lib throwing an error when there's a try to authenticate using Kubernetes
    /// and no Kubernetes is available
    /// Output:VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with basic Vault configurations
    /// - Has injector for Vault basic config and Kubernetes config
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingKubernetesWithSSL()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultKubernetesConfiguration(true);

        // IMPORTANT NOTICE: THE FILE USED AS JWT TOKEN WAS TAKEN FROM A MINISTACK
        // ENVIRONMENT, THERE'S NO HARM IN HAVING THIS PUBLISHED IN THE REPOSITORY
        string JWTTokenFilePath = Directory.GetCurrentDirectory() + "../../../../Resources/jwt/JWT.token";
        Environment.SetEnvironmentVariable("SA_TOKEN_PATH", JWTTokenFilePath);


        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.AllSecretTypes.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .Throw<VaultConfigurationSourceException>();

    }

    /// <summary>
    /// Objective: Loading lib throwing an error when there's a try to authenticate using Certificates
    /// Output:VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with basic Vault configurations
    /// - Has injector for Vault basic config and Certificates config
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingCertificatesWithSSL()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultCertificatesConfiguration(true);

        // IMPORTANT NOTICE: THE FILE USED AS CERTIFICATE WAS GENERATED FROM SCRATCH
        // AND IS NOT USED ANYWHERE THERE'S NO HARM IN HAVING THIS PUBLISHED IN THE REPOSITORY
        string CertificateFilePath = Directory.GetCurrentDirectory() + "../../../../Resources/ssl/Cert.p12";
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT", CertificateFilePath);


        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.AllSecretTypes.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .Throw<Exception>();

    }


    /// <summary>
    /// Objective: Loading lib throwing an error when there's a try to authenticate using Certificates
    /// Without SSL
    /// Output:VaultConfigurationSourceException
    /// Uses: 
    /// - appsettings with basic Vault configurations
    /// - Has injector for Vault basic config and Certificates config
    /// - Has placeholders
    /// </summary>
    [Test]
    public void TestSettingsFileWithPlaceholderUsingCertificatesWithoutSSL()
    {

        // Inject Vault AppRole Configuration without Vault basic config
        VaultConfigurationEnvInjector.InjectVaultCertificatesConfiguration(true);
        VaultConfigurationEnvInjector.InjectVaultSkipVerify();

        // IMPORTANT NOTICE: THE FILE USED AS CERTIFICATE WAS GENERATED FROM SCRATCH
        // AND IS NOT USED ANYWHERE THERE'S NO HARM IN HAVING THIS PUBLISHED IN THE REPOSITORY
        string CertificateFilePath = Directory.GetCurrentDirectory() + "../../../../Resources/ssl/Cert.p12";
        Environment.SetEnvironmentVariable("VAULT_CLIENT_CERT", CertificateFilePath);


        // Create a new Vault client
        var host = Host.CreateDefaultBuilder().ConfigureHostConfiguration(config =>
                {
                    config.AddJsonFile("Resources/appsettings.AllSecretTypes.json");
                }
            )
            .UseVault();

        // As configuration file has no placeholder to be replaced should throw an error stating that there aren't any placeholders
        host.Invoking(host => host.Build())
            .Should()
            .Throw<Exception>();

    }
}
