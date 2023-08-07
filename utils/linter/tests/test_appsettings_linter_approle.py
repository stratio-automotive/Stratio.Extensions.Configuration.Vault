from validator.validator import Validator
from validator.validator_report import ValidatorReport

# Sets the base folder where the test resources are located at
resources_folder = "tests/resources/"

def test_env_approle_appsettings_file_all_good():
    """
    Validates a 100% valid environment specific appsettings file
    """

    appsettings_file = resources_folder + "appsettings.AppRole.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 4
    assert any("{% user_home %}/.vault/secrets/approle.role_id" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])

    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0

def test_env_kubernetes_appsettings_file_broken():
    """
    Validates a Kubernetes environment broken appsettings file
    """

    appsettings_file = resources_folder + "appsettings.AppRoleBroken.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0

    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 4
    assert any("{% u1ser_home %}/.vault/secrets/approle.secret_id" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])

