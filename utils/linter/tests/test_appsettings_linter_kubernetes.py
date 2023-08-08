from src.validator.validator import Validator
from src.validator.validator_report import ValidatorReport

# Sets the base folder where the test resources are located at
resources_folder = "tests/resources/"

def test_env_kubernetes_appsettings_file_all_good():
    """
    Validates a 100% valid environment specific appsettings file
    """

    appsettings_file = resources_folder + "appsettings.Kubernetes.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 5
    assert any("https://vault-2022.stratio.local:8200" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("app/uat" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("kubernetes" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("kubernetes-role" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])
    assert any("/var/run/secrets/kubernetes.io/serviceaccount/token" in item for item in validator_report._ValidatorReport__files[appsettings_file]["successes"])

    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 0

def test_env_kubernetes_appsettings_file_broken():
    """
    Validates a Kubernetes environment broken appsettings file
    """

    appsettings_file = resources_folder + "appsettings.KubernetesBroken.json"

    validator_report = ValidatorReport()
    linter = Validator(appsettings_file, validator_report)
    linter.validate_environment_appsettings_placeholders()
    linter.validate_vault_object()

    assert len(validator_report._ValidatorReport__files[appsettings_file]["successes"]) == 0
    assert len(validator_report._ValidatorReport__files[appsettings_file]["warnings"]) == 0

    assert len(validator_report._ValidatorReport__files[appsettings_file]["failures"]) == 5
    assert any("kubernetes!&/($" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])
    assert any("kubernetes--role/%" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])
    assert any("/var\\/run/secrets,kubernetes.io/serviceaccount/token" in item for item in validator_report._ValidatorReport__files[appsettings_file]["failures"])
