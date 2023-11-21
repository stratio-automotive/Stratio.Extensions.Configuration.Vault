# .NET Projects appsettings Configuration Linter for Stratio Vault Library

## Description

This Python script is a linter that validates the contents of the appsettings.json file(s)
which are used by the [Stratio Vault Library](https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault).
It ensures that all occurrences of:

- `{% vault_secret path/to/secret:key %}`
- `{% vault_dict path/to/secret %}`
- `{% user_home %}`
- the Vault JSON object

are consistent with the requirements of the [Stratio Vault Library](https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault).

## Usage

    python -m src.main --work-dir <path_to_the_appsettings_files_folder>
    vault-appsettings-linter --work-dir <path_to_the_appsettings_files_folder>

## Available releases

- Docker image: [stratioautomotive/vault-appsettings-linter](https://hub.docker.com/r/stratioautomotive/vault-appsettings-linter)
- PyPi Package: [vault-appsettings-linter](https://pypi.org/project/vault-appsettings-linter/)

## Authors

Rafael Couto [rafaelcouto@stratioautomotive.com](mailto:rafaelcouto@stratioautomotive.com)
Bernardo Marques [bernardomarques@stratioautomotive.com](mailto:bernardomarques@stratioautomotive.com)

## License

This script is licensed under LGPL-v3, the same license of the parent project [Stratio Vault Library](https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault).
