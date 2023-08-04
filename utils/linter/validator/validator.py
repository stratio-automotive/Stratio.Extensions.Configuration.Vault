import json
import re
import validators

class Validator:
    """
    A class to validate the various appsettings files.

    Attributes:
        validation_report (ValidationReport): An instance of the Validation Report.
    """

    def __init__(self, validation_report):
        """
        Initialize the Validator object with an existing validation report.
        """
        self.validation_report = validation_report

    def match_placeholder(self, appsettings_file, string, pattern, message):
        """
        Tries to match the given string with a placeholder.

        Parameters:
            appsettings_file (str): The name of the appsettings file.
            string (str): The string where to find the placeholder.
            pattern (str): The regular expression that should match the string.
            message (str): The specific message to be added to the report if matching fails.
        """
        if not re.match(pattern, string):
            self.validation_report.add_failure(
                appsettings_file,
                "'{% " + string + " %}'",
                message
            )
        else:
            self.validation_report.add_success(
                appsettings_file,
                "'{% " + string + " %}'",
                "Meets the placeholder syntax requirements."
            )

    def match_field(self, appsettings_file, string, pattern, message_on_success, message_on_failure):
        """
        Tries to match the given string with a pattern.

        Parameters:
            appsettings_file (str): The name of the appsettings file.
            string (str): The string to be validated.
            pattern (str): The regular expression that should match the string.
            message_on_success (str): The specific message to be added to the report if matching succeeds.
            message_on_failure (str): The specific message to be added to the report if matching fails.
        """
        if not re.match(pattern, string):
            self.validation_report.add_failure(
                appsettings_file,
                string,
                message_on_failure
            )
        else:
            self.validation_report.add_success(
                appsettings_file,
                string,
                message_on_success
            )

    def validate_base_appsettings_placeholders(self, appsettings_data, appsettings_file):
        """
        Validates the base appsettings.json placeholders and adds the assessments to
        the validation report.

        Parameters:
            appsettings_data (JSON): The JSON object containing the appsettings.json file data.
            appsettings_file (str): The name of the file of the appsettings.json
        """

        # Goes through all the placeholders in the appsettings file
        for match in re.findall(r'{% (.+?) %}', json.dumps(appsettings_data)):

            # Vault Secret Field
            if match.startswith("vault_secret"):
                self.match_placeholder(
                    appsettings_file,
                    match,
                    "^vault_secret\s+([a-zA-Z]+(?:[-\/][a-zA-Z]+)*):([a-zA-Z]+(?:[-\/][a-zA-Z]+)*)$",
                    "Vault secret field placeholders should be similar to: '{% vault_secret path/to/secret:key %}'."
                )

            # Vault Secret Dict
            elif match.startswith("vault_dict"):
                self.match_placeholder(
                    appsettings_file,
                    match,
                    "^vault_dict\s+([a-zA-Z]+(?:[-\/][a-zA-Z]+)*)$",
                    "Vault secret dict placeholders should be similar to: '{% vault_dict path/to/secret %}'."
                )

            # Home
            elif match.startswith("user_home"):
                self.match_placeholder(
                    appsettings_file,
                    match,
                    "^user_home$",
                    "The user home placeholder should be literally only: '{% user_home %}'."
                )

            # Anything else might be some kind of other typo, let's create a warning
            else:
                self.validation_report.add_warning(
                    appsettings_file,
                    "'{% " + match + " %}'",
                    "Are you sure that is correct? You might be using it for something else!"
                )

    def validate_environment_appsettings_placeholders(self, appsettings_data, appsettings_file):
        """
        Validates the environment specific appsettings file placeholders and adds the assessments to
        the validation report.

        Parameters:
            appsettings_data (JSON): The JSON object containing the environment appsettings file data.
            appsettings_file (str): The name of the file of the environment appsettings file.
        """

        # Goes through all the placeholders in the appsettings file
        for match in re.findall(r'{% (.+?) %}', json.dumps(appsettings_data)):

            # Home
            if match.startswith("user_home"):
                self.match_placeholder(
                    appsettings_file,
                    match,
                    "^user_home$",
                    "The user home placeholder should be literally only: '{% user_home %}'."
                )

            # Anything else should be set in the base appsettings_folder, let's create a warning
            else:
                self.validation_report.add_warning(
                    appsettings_file,
                    "'{% " + match + " %}'",
                    "As a best practice you should put all your secret placeholders in the base appsettings.json file!"
                )

    def validate_vault_object(self, appsettings_data, appsettings_file):
        """
        Validates the Vault object to make sure it has a proper configuration.

        Parameters:
            appsettings_data (JSON): The JSON object containing the appsettings.json file data.
            appsettings_file (str): The name of the file of the appsettings.json
        """

        if "Vault" not in appsettings_data:
            self.validation_report.add_warning(
                    appsettings_file,
                    "Vault Section",
                    "You don't have the Vault connection configuration section in this appsettings file!"
                )
            return

        #
        # Vault server configurations
        #

        # Validate the syntax of the Vault address
        if "vaultAddress" in appsettings_data["Vault"]:
            if validators.url(appsettings_data["Vault"]["vaultAddress"]):
                self.validation_report.add_success(
                    appsettings_file,
                    appsettings_data["Vault"]["vaultAddress"],
                    "is a valid Vault address."
                )
            else:
                self.validation_report.add_failure(
                    appsettings_file,
                    appsettings_data["Vault"]["vaultAddress"],
                    "is NOT a valid Vault address."
                )

        # Validate the syntax of the Vault mountpoint
        if "mountPoint" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["mountPoint"],
                "^(?!.*--)(?!.*\/\/)(?!.*-\/)(?!.*\/-)(?!-.*)(?!\/.*)([a-zA-Z0-9-]*\/)*[a-zA-Z0-9]+$",
                "is a valid Vault mountpoint.",
                "is NOT a valid Vault mountpoint."
            )

        #
        # Vault AppRole authentication configurations
        #

        # Validate the syntax of the Approle Authentication method name
        if "approleAuthName" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["approleAuthName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault AppRole authentication method name.",
                "is NOT a valid Vault AppRole authentication method name."
            )

        # Validate the syntax of the Approle ID path
        if "roleIdPath" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["roleIdPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault AppRole ID path.",
                "is NOT a valid Vault AppRole ID path."
            )

        # Validate the syntax of the Approle Secret ID path
        if "secretIdPath" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["secretIdPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault AppRole secret ID path.",
                "is NOT a valid Vault AppRole secret ID path."
            )

        #
        # Vault Kubernetes authentication configurations
        #

        # Validate the syntax of the Kubernetes authentication method name
        if "kubernetesAuthName" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["kubernetesAuthName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault Kubernetes authentication method name.",
                "is NOT a valid Vault Kubernetes authentication method name."
            )

        # Validate the syntax of the Kubernetes Service Account name
        if "kubernetesSaRoleName" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["kubernetesSaRoleName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault Kubernetes Service Account name.",
                "is NOT a valid Vault Kubernetes Service Account name."
            )

        # Validate the syntax of the Kubernetes Service Account token path
        if "kubernetesSaTokenPath" in appsettings_data["Vault"]:
            self.match_field(
                appsettings_file,
                appsettings_data["Vault"]["kubernetesSaTokenPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault Kubernetes Service Account token path.",
                "is NOT a valid Vault Kubernetes Service Account token path."
            )
