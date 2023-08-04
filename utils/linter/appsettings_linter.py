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
python appsettings_linter.py <path_to_the_appsettings_files_folder>

Authors:
Rafael Couto (rafaelcouto@stratioautomotive.com)
Bernardo Marques (bernardomarques@stratioautomotive.com)

License:
This script is licensed under the same license of the parent project
For more details refer to: https://github.com/stratio-automotive/Stratio.Extensions.Configuration.Vault/blob/main/License.md
"""

import argparse
import json
import os
import re

# Class that stores the Validation report
from report.validation_report import ValidationReport
from validator.validator import Validator

class SingletonValidationReport(ValidationReport):
    """
    A Singleton class that inherits from ValidationReport to ensure a single instance exists.

    This class uses the Singleton pattern to ensure there's only one instance of the
    ValidationReport object throughout the script.

    Attributes:
        _instance: The single instance of the class.

    Methods:
        __new__(): Creates and returns the single instance of the class if it doesn't exist.

    Usage:
        validation_report = SingletonValidationReport()
    """
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

validation_report = SingletonValidationReport()

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

def process_base_appsettings_file(appsettings_file):
    """
    Processes the base appsettings.json file.
    This method will validate the syntax of all occurrences of the placeholders supported by the Stratio Vault Library.

    Parameters:
        - appsettings_file (str): The path to the base appsettings.json file.
    """
    appsettings_data = load_appsettings(appsettings_file)

    validator = Validator(validation_report)
    validator.validate_base_appsettings_placeholders(appsettings_data, appsettings_file)
    validator.validate_vault_object(appsettings_data, appsettings_file)

def process_environment_appsettings_file(appsettings_file):
    """
    Processes the environment appsettings file.
    This method will validate an environment specific appsettings file.

    Parameters:
        - appsettings_file (str): The path to the environment appsettings file.
    """
    appsettings_data = load_appsettings(appsettings_file)

    validator = Validator(validation_report)
    validator.validate_environment_appsettings_placeholders(appsettings_data, appsettings_file)
    validator.validate_vault_object(appsettings_data, appsettings_file)

def main(work_dir):
    """
    Main function.

    Parameters:
        - work_dir (str): The path where the appsettings files are located.
    """

    # Look for appsettings.json file first
    base_appsettings_file = os.path.join(work_dir, "appsettings.json")
    if not os.path.exists(base_appsettings_file):
        validation_report.add_failure("appsettings.json", "File wasn't found in the given folder.")
        validation_report.print_report()

    # Process base appsettings file
    process_base_appsettings_file(base_appsettings_file)

    # Load environment files, and process each of the files
    for env_appsettings_file in os.listdir(work_dir):
        if env_appsettings_file.startswith('appsettings.') and \
            env_appsettings_file.endswith('.json') and \
            env_appsettings_file != "appsettings.json":
            process_environment_appsettings_file(os.path.join(work_dir, env_appsettings_file))

    # Print the report
    validation_report.print_report_table()

    print ("\n=== SUMMARY ===")
    exit_status = 0
    for file, failures in validation_report.get_failures().items():
        print("> " + file + " " +
              (validation_report.colored("\u2717", "red") if len(failures) > 0 else validation_report.colored("\u2713", "green")))
        if len(failures) > 0:
            exit_status = 1
            for item, message in failures:
                print(validation_report.colored("  - " + item + ": " + message, "red"))

    exit(exit_status)

if __name__ == "__main__":

    parser = argparse.ArgumentParser()
    parser.add_argument('--work-dir', required=True,help='The directory where the appsettings files should be located.')

    args = parser.parse_args()

    print("=== Appsettings Linter ===")
    print("\nThe current script will validate the appsettings files found in: " + args.work_dir)

    # Before anything lets validate if the work dir exist
    if not os.path.isdir(args.work_dir):
        print (f"\nThe provided directory '{args.work_dir}' does not exist!")
        exit(1)

    main(args.work_dir)
