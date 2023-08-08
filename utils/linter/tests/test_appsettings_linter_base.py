from src.validator.validator import Validator
from src.validator.validator_report import ValidatorReport

# Sets the base folder where the test resources are located at
resources_folder = "tests/resources/"

def test_base_appsettings_file_all_good():
    """
    Validates a 100% valid base appsettings file
    """

    appsettings_file = resources_folder + "appsettings.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_base_appsettings_placeholders()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 15
    assert any("'{% vault_dict my-tools/events/clients %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("'{% vault_secret my-tools/kafka:brokers %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("'{% user_home %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0

def test_appsettings_no_vault():
    """
    Validates an appsettings file without Vault definition which should trigger a warning
    """

    appsettings_file = resources_folder + "appsettings.NoVault.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 0
    
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 1
    assert any("Vault Section" in item for item in validator_report._ValidatorReport__files[appsettings_file]["warnings"])

    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0

def test_appsettings_broken_secrets():
    """
    Validates a base appsettings file with broken secrets
    """

    appsettings_file = resources_folder + "appsettings.BaseBrokenSecrets.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_base_appsettings_placeholders()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 11

    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 2
    assert any("'{% vault_dicionary my-tools/events/clients %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["warnings"])
    assert any("'{% vault_secret_secret my-tools/kafka:brokers %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["warnings"])

    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 2
    assert any("'{% vault_secret my-tools/kafkatopic %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])
    assert any("'{% vault_dict my-tools/mysql/clients:nonexistent %}'" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])

