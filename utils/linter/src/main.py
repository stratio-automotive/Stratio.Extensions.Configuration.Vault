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

import argparse
import os

# File that contains helping methods
from .utils import helper

# Class that stores the Validator report
from .validator.validator_report import ValidatorReport
from .validator.validator import Validator

class SingletonValidatorReport(ValidatorReport):
    """
    A Singleton class that inherits from ValidatorReport to ensure a single instance exists.

    This class uses the Singleton pattern to ensure there's only one instance of the
    ValidatorReport object throughout the script.

    Attributes:
        _instance: The single instance of the class.

    Methods:
        __new__(): Creates and returns the single instance of the class if it doesn't exist.

    Usage:
        validator_report = SingletonValidatorReport()
    """
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

validator_report = SingletonValidatorReport()

def process_base_appsettings_file(appsettings_file):
    """
    Processes the base appsettings.json file.
    This method will validate the syntax of all occurrences of the placeholders supported by the Stratio Vault Library.

    Parameters:
        - appsettings_file (str): The path to the base appsettings.json file.
    """
    validator = Validator(appsettings_file, validator_report)
    validator.validate_base_appsettings_placeholders()
    validator.validate_vault_object()

def process_environment_appsettings_file(appsettings_file):
    """
    Processes the environment appsettings file.
    This method will validate an environment specific appsettings file.

    Parameters:
        - appsettings_file (str): The path to the environment appsettings file.
    """
    validator = Validator(appsettings_file, validator_report)
    validator.validate_environment_appsettings_placeholders()
    validator.validate_vault_object()

def main():
    """
    Main function.
    """

    parser = argparse.ArgumentParser()
    parser.add_argument('--work-dir', required=True,help='The directory where the appsettings files should be located.')

    args = parser.parse_args()

    work_dir = args.work_dir

    print("=== Appsettings Linter ===")
    print("\nThe current script will validate the appsettings files found in: " + work_dir)

    # Before anything lets validate if the work dir exist
    if not os.path.isdir(work_dir):
        print (helper.color_text(f"\nThe provided directory '{work_dir}' does not exist!", "red"))
        exit(1)


    # Look for appsettings.json file first
    base_appsettings_file = os.path.join(work_dir, "appsettings.json")
    if not os.path.exists(base_appsettings_file):
        print(helper.color_text("\nThe base file 'appsettings.json' wasn't found in the provided directory.", "red"))
        exit(1)

    # Process base appsettings file
    process_base_appsettings_file(base_appsettings_file)

    # Load environment files, and process each of the files
    for env_appsettings_file in os.listdir(work_dir):
        if env_appsettings_file.startswith('appsettings.') and \
            env_appsettings_file.endswith('.json') and \
            env_appsettings_file != "appsettings.json":
            process_environment_appsettings_file(os.path.join(work_dir, env_appsettings_file))

    # Print the report for each file
    validator_report.print_report_table()

    # Print the exit summary and find the exit status
    failures = validator_report.print_exit_summary()

    # Exit code is conditioned on the existence of failures
    exit(1 if failures > 0 else 0)

if __name__ == "__main__":
    main()
