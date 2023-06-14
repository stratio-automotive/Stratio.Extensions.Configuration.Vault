# vault-configuration-dotnet <!-- omit in toc -->

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=stratio2_vault-configuration-dotnet&metric=alert_status&token=e71dc8ab542157d1d3fd0c4da2003e3602f2fdd1)](https://sonarcloud.io/summary/new_code?id=stratio2_vault-configuration-dotnet)

## Description

NuGet package to load Vault secrets into your IHostBuilder configuration automagically

Deliverables:

* `Stratio.Extensions.Configuration.Vault.nupkg`

## Table of Contents

- [How it Works](#how-it-works)
- [Caveats](#caveats)
- [Connecting to Vault](#connecting-to-vault)
- [Placeholders](#placeholders)
  - [`vault_secret`](#vault_secret)
  - [`vault_dict`](#vault_dict)
  - [Examples](#examples)
- [Environments](#environments)

## How it Works

Consider the example below to guide the explanation.

The example Host instantiation:

```c#
var host = Host.CreateDefaultBuilder()
    .UseEnvironment("Development")
    .ConfigureHostConfiguration(config =>
    {
        // ... your configurations here ...
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        // ... your configurations here ...
    })
    .ConfigureServices((host, services) =>
    {
        // ... your configurations here ...
    })
    .UseVault()
    .Build();
```

The example appsettings.json:

```json
{
  "ConnectionStrings": {
    "ServerContext": "Data Source={% vault_secret mssql/backoffice:host %},{% vault_secret mssql/backoffice:port %};",
    "ClientContexts": "{% vault_dict mssql/clients %}"
  }
  "Vault": {
    ... (see below)
  }
}
```

In this example, the configuration is loaded by:

1. Adding the `.UseVault()` call to the Host construction
2. Replacing the secrets in appsettings.json with placeholders

There are although some questions left unanswered:

* What are the caveats?
* How does the service connect and authenticate with vault?
* `vault_secret` and `vault_dict`, what's the difference? are there other types?
* Why is the environment relevant?

Refer to the sections below for the answers.

## Caveats

Since the placeholders are being overriden in the `ConfigureAppConfiguration` stage, only configurations loaded in `ConfigureHostConfiguration` calls will actually be properly overriden.

So, in the following example, the configuration in `appsettings.notoverriden.json` will not be overriden!

```c#
var host = Host.CreateDefaultBuilder()
    .UseEnvironment("Development")
    .ConfigureAppConfiguration((context, config) =>
    {
        // This configurations will not be overriden because they're being loaded
        // in the ConfigureAppConfiguration stage.
        config.AddJsonFile("appsettings.notoverriden.json");
    })
    .UseVault()
    .Build();
```

## Connecting to Vault

The NuGet expects the following Vault configurations to be present in the appsettings.json file:

```json
  "Vault": {
    "vaultAddress": "https://VAULT_ADDRESS:8200",
    "mountPoint": "PATH/OF/MOUNTPOINT",
    
    # For authentication via AppRole
    "approleAuthName": "approle",
    "roleIdPath": "PATH/TO/approle.role_id",
    "secretIdPath": "PATH/TO/approle.secret_id",
    
    # For authentication via Kubernetes Role
    "kubernetesAuthName": "kubernetes",
    "kubernetesSaRoleName": "kubernetes-role",
    "kubernetesSaTokenPath": "/var/run/secrets/kubernetes.io/serviceaccount/token"
  },
```

## Placeholders

Placeholders are strings in appsettings.json that look like `{% <type> <args> %}`.  These placeholders are replaced by concrete values when the Host is built.

Refer to the sub sections bellow for a list of supported types.

### vault_secret

Materializes into a field from a secret in vault.

Expected Format: `{% vault_secret <path/to/secret/in/vault>:<secret-field> %}`

### vault_dict

Materializes into an entire dictionary, matching the structure of the correponding secret.

Expected Format: `{% vault_dict <path/to/secret/in/vault> %}`

### Examples

Consider the following secret in vault:

```
  mssql
  |
  | - backoffice
  |     Host = backoffice.localhost
  |     Port = 1433
  | - clients
  |     _default = "Data Source=default.localhost,1433"
  |     other = "Data Source=other.localhost,1433"
```

```
  {% vault_secret mssql/backoffice:Host %}

  would map to:

  backoffice.localhost
```

```
  {% vault_dict mssql/clients %}

  would map to:

  {
    "_default": "Data Source=default.localhost,1433"
    "other": "Data Source=other.localhost,1433"
  }
```

## Environments

Depending on the Hosting environment, different secrets will be loaded.

This distinction is done at the `mount point` level.

So, if the vault your using has credentials for multiple environments, these secrets should be at different mount points within vault.

Bellow is an example mapping between mount points and environment types, but in the end it all depends on your Vault installation/configuration:

| Environment Type | Mount Point |
|---|---|
| Development | /dev/secrets |
| Staging | /uat/secrets |
| Production | /prod/secrets |
