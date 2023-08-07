"""
.NET Projects appsettings Configuration Linter

Description:
This Python script is a linter that validates the contents of the appsettings.json file(s)
which are related to the Stratio Vault Library.
It ensures that all occurrences of:
 - `{% vault_secret path/to/secret:key %}`
 - `{% vault_dict path/to/secret %}`
 - `{% user_home %}`
 - the Vault JSON object
are consistent with the requirements of the Stratio Vault Library.

Usage:
python main.py <path_to_the_appsettings_files_folder>

Authors:
Rafael Couto (rafaelcouto@stratioautomotive.com)
Bernardo Marques (bernardomarques@stratioautomotive.com)

License:
This script is licensed under the same license of the parent project
For more details refer to: https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault/blob/main/License.md
"""

import copy
import json
import re
import validators

class Validator:
    """
    A class to validate the various appsettings files.

    Attributes:
        validator_report (ValidatorReport): An instance of the Validation Report.
    """

    def __init__(self, appsettings_file, validator_report):
        """
        Initialize the validator object with an existing validation report.
        """
        self.validator_report = validator_report
        self.appsettings_file = appsettings_file
        self.appsettings_data = self.load_appsettings(appsettings_file)

    @staticmethod
    def load_appsettings(appsettings_file):
        """
        Load an appsettings.json file into a dictionary structure.

        Parameters:
            - appsettings_file (str): Path to the appsettings.json file.

        Returns:
            - dict: The appsettings file as a dictionary.
        """
        with open(appsettings_file, "r") as base:
            settings = json.load(base)

        return settings

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
            self.validator_report.add_failure(
                appsettings_file,
                "'{% " + string + " %}'",
                message
            )
        else:
            self.validator_report.add_success(
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
            self.validator_report.add_failure(
                appsettings_file,
                string,
                message_on_failure
            )
        else:
            self.validator_report.add_success(
                appsettings_file,
                string,
                message_on_success
            )

    def validate_base_appsettings_placeholders(self):
        """
        Validates the base appsettings.json placeholders and adds the assessments to
        the validation report.
        """

        clean_appsettings_data = copy.deepcopy(self.appsettings_data)
        
        # When we're validating the secret fields we don't need to validate the vault connection
        # That's what the validate_vault_object method is for
        if "Vault" in self.appsettings_data:
            del clean_appsettings_data["Vault"]

        # Goes through all the placeholders in the appsettings file
        for match in re.findall(r'{% (.+?) %}', json.dumps(clean_appsettings_data)):

            # Vault Secret Field
            if match.startswith("vault_secret "): 
                self.match_placeholder(
                    self.appsettings_file,
                    match,
                    "^vault_secret\s+([a-zA-Z]+(?:[-\/][a-zA-Z]+)*):([a-zA-Z]+(?:[-\/][a-zA-Z]+)*)$",
                    "Vault secret field placeholders should be similar to: '{% vault_secret path/to/secret:key %}'."
                )

            # Vault Secret Dict
            elif match.startswith("vault_dict "):
                self.match_placeholder(
                    self.appsettings_file,
                    match,
                    "^vault_dict\s+([a-zA-Z]+(?:[-\/][a-zA-Z]+)*)$",
                    "Vault secret dict placeholders should be similar to: '{% vault_dict path/to/secret %}'."
                )

            # Home
            elif match.startswith("user_home"):
                self.match_placeholder(
                    self.appsettings_file,
                    match,
                    "^user_home$",
                    "The user home placeholder should be literally only: '{% user_home %}'."
                )

            # Anything else might be some kind of other typo, let's create a warning
            else:
                self.validator_report.add_warning(
                    self.appsettings_file,
                    "'{% " + match + " %}'",
                    "Are you sure that this is correct? You might be using it for something else!"
                )

    def validate_environment_appsettings_placeholders(self):
        """
        Validates the environment specific appsettings file placeholders and adds the assessments to
        the validation report.
        """

        clean_appsettings_data = copy.deepcopy(self.appsettings_data)
        
        # When we're validating the secret fields we don't need to validate the vault connection
        # That's what the validate_vault_object method is for
        if "Vault" in self.appsettings_data:
            del clean_appsettings_data["Vault"]

        # Goes through all the placeholders in the appsettings file
        for match in re.findall(r'{% (.+?) %}', json.dumps(clean_appsettings_data)):

            # Home
            if match.startswith("user_home"):
                self.match_placeholder(
                    self.appsettings_file,
                    match,
                    "^user_home$",
                    "The user home placeholder should be literally only: '{% user_home %}'."
                )

            # Anything else should be set in the base appsettings_folder, let's create a warning
            else:
                self.validator_report.add_warning(
                    self.appsettings_file,
                    "'{% " + match + " %}'",
                    "As a best practice you should put all your secret placeholders in the base appsettings.json file!"
                )

    def validate_vault_object(self):
        """
        Validates the Vault object to make sure it has a proper configuration.
        """

        if "Vault" not in self.appsettings_data:
            self.validator_report.add_warning(
                    self.appsettings_file,
                    "Vault Section",
                    "You don't have the Vault connection configuration section in this appsettings file!"
                )
            return

        #
        # Vault server configurations
        #

        # Validate the syntax of the Vault address
        if "vaultAddress" in self.appsettings_data["Vault"]:
            if validators.url(self.appsettings_data["Vault"]["vaultAddress"]):
                self.validator_report.add_success(
                    self.appsettings_file,
                    self.appsettings_data["Vault"]["vaultAddress"],
                    "is a valid Vault address."
                )
            else:
                self.validator_report.add_failure(
                    self.appsettings_file,
                    self.appsettings_data["Vault"]["vaultAddress"],
                    "is NOT a valid Vault address."
                )

        # Validate the syntax of the Vault mountpoint
        if "mountPoint" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["mountPoint"],
                "^(?!.*--)(?!.*\/\/)(?!.*-\/)(?!.*\/-)(?!-.*)(?!\/.*)([a-zA-Z0-9-]*\/)*[a-zA-Z0-9-]+$",
                "is a valid Vault mountpoint.",
                "is NOT a valid Vault mountpoint."
            )

        #
        # Vault AppRole authentication configurations
        #

        # Validate the syntax of the Approle Authentication method name
        if "approleAuthName" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["approleAuthName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault AppRole authentication method name.",
                "is NOT a valid Vault AppRole authentication method name."
            )

        # Validate the syntax of the Approle ID path
        if "roleIdPath" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["roleIdPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault AppRole ID path.",
                "is NOT a valid Vault AppRole ID path."
            )

        # Validate the syntax of the Approle Secret ID path
        if "secretIdPath" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["secretIdPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault AppRole secret ID path.",
                "is NOT a valid Vault AppRole secret ID path."
            )

        #
        # Vault Kubernetes authentication configurations
        #

        # Validate the syntax of the Kubernetes authentication method name
        if "kubernetesAuthName" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["kubernetesAuthName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault Kubernetes authentication method name.",
                "is NOT a valid Vault Kubernetes authentication method name."
            )

        # Validate the syntax of the Kubernetes Service Account name
        if "kubernetesSaRoleName" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["kubernetesSaRoleName"],
                "^(?!.*--)(?!-.*)([a-zA-Z0-9-]*)$",
                "is a valid Vault Kubernetes Service Account name.",
                "is NOT a valid Vault Kubernetes Service Account name."
            )

        # Validate the syntax of the Kubernetes Service Account token path
        if "kubernetesSaTokenPath" in self.appsettings_data["Vault"]:
            self.match_field(
                self.appsettings_file,
                self.appsettings_data["Vault"]["kubernetesSaTokenPath"],
                "^({% user_home %}){0,1}([\/]*[a-zA-Z0-9_\-\.]+)+(.[a-zA-Z]+?)$",
                "is a valid Vault Kubernetes Service Account token path.",
                "is NOT a valid Vault Kubernetes Service Account token path."
            )
