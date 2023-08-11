"""
.NET Projects appsettings Configuration Linter for Stratio Vault Library

Description:
This Python script is a linter that validates the contents of the appsettings.json file(s)
which are used by the Stratio Vault Library.
It ensures that all occurrences of:
 - `{% vault_secret path/to/secret:key %}`
 - `{% vault_dict path/to/secret %}`
 - `{% user_home %}`
 - the Vault JSON object
are consistent with the requirements of the Stratio Vault Library.

Authors:
Rafael Couto (rafaelcouto@stratioautomotive.com)
Bernardo Marques (bernardomarques@stratioautomotive.com)
"""

from src.validator.validator import Validator
from src.validator.validator_report import ValidatorReport

# Sets the base folder where the test resources are located at
resources_folder = "tests/resources/"

def test_env_approle_appsettings_file_all_good():
    """
    Validates a 100% valid environment specific appsettings file.
    """

    appsettings_file = resources_folder + "appsettings.AppRole.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 5
    assert any("{% user_home %}/.vault/secrets/approle.role_id" in item
                for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("approle-secondary-name" in item
                for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])

    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0

def test_env_approle_appsettings_file_broken():
    """
    Validates an Approle environment broken appsettings file.
    """

    appsettings_file = resources_folder + "appsettings.AppRoleBroken.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0

    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 4
    assert any("{% u1ser_home %}/.vault/secrets/approle.secret_id" in item
                for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])
    
def test_env_appsettings_file_with_placeholders():
    """
    Validates an environment appsettings file with placeholders.
    """

    appsettings_file = resources_folder + "appsettings.EnvWithPlaceholders.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 5
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 1
    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0
