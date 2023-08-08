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

from rich.console import Console
from rich.table import Table
from rich import box

# File that contains helping methods
import utils.helper as helper

class ValidatorReport:
    """
    A class to store the validator report objects like successes, warnings, and failures.

    Attributes:
        files (dict): A dictionary of files with reports for each file.
        files[][successes] (list): A list to store success messages.
        files[][warnings] (list): A list to store warning messages.
        files[][failures] (list): A list to store failure messages.
    """

    def __init__(self):
        """
        Initialize the ValidatorReport object with an empty dictionary of files
        that will later contain successes, warnings, and failures.
        """
        self.__files = {}

    def add_file(self, filename):
        """
        Add a file to the report and sets the default state for successes, warnings and failures.

        Args:
            filename (str): The name of the file to be added to the report.
        """
        if filename in self.__files:
            return

        self.__files[filename] = {
            "successes" : [],
            "warnings": [],
            "failures": []
        }

    def add_success(self, filename, item, message):
        """
        Add a success message to the validator report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The success message to be added.
        """
        self.add_file(filename)
        self.__files[filename]["successes"].append([item, message])

    def add_warning(self, filename, item, message):
        """
        Add a warning message to the validator report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The warning message to be added.
        """
        self.add_file(filename)
        self.__files[filename]["warnings"].append([item, message])

    def add_failure(self, filename, item, message):
        """
        Add a failure message to the validator report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The failure message to be added.
        """
        self.add_file(filename)
        self.__files[filename]["failures"].append([item, message])

    def print_report_table(self):
        """
        Print a pretty table with the validator report results.

        This method prints the collected successes, warnings, and failures for each file
        from the validator report in a well-formatted table using the tabulate library.
        """

        # Print a table for each of the files
        for filename, report_data in self.__files.items():

            table = Table(
                title=f"\nAppsettings file: {filename}",
                expand=True,
                caption=f"Successes: {str(len(report_data['successes']))}, " +
                        f"Warnings: {str(len(report_data['warnings']))}, " +
                        f"Failures: {str(len(report_data['failures']))}",
                box=box.SQUARE_DOUBLE_HEAD,
                show_lines=True)
            table.add_column("Status", justify="center", no_wrap=True)
            table.add_column("#", justify="center", no_wrap=True)
            table.add_column("Item", justify="left")
            table.add_column("Message", justify="left")

            # Add successes to the report table
            if report_data["successes"]:
                table.add_row(
                    "Success",
                    str(len(report_data["successes"])),
                    "\n".join([f"{item}" for item, message in report_data["successes"]]),
                    "\n".join([f"{message}" for item, message in report_data["successes"]]),
                    style="green"
                )
            else:
                table.add_row(
                    "Success",
                    "0",
                    "",
                    "",
                    style="green"
                )

            # Add warnings to the report table
            if report_data["warnings"]:
                table.add_row(
                    "Warning",
                    str(len(report_data["warnings"])),
                    "\n".join([f"{item}" for item, message in report_data["warnings"]]),
                    "\n".join([f"{message}" for item, message in report_data["warnings"]]),
                    style="yellow"
                )
            else:
                table.add_row(
                    "Warning",
                    "0",
                    "",
                    "",
                    style="yellow"
                )

            # Add failures to the report table
            if report_data["failures"]:
                table.add_row(
                    "Failure",
                    str(len(report_data["failures"])),
                    "\n".join([f"{item}" for item, message in report_data["failures"]]),
                    "\n".join([f"{message}" for item, message in report_data["failures"]]),
                    style="red"
                )
            else:
                table.add_row(
                    "Failure",
                    "0",
                    "",
                    "",
                    style="red"
                )

            console = Console()
            console.print(table)

    def print_exit_summary(self):
        """
        Prints the closing summary before the Linter ends.

        Returns:
            - bool: The exit status that should be sent in the end
        """

        # Prints the closing summary showing only if any failures were found
        print ("\n=== SUMMARY ===")

        failures = 0
        for file, report in self.__files.items():

            # Sets the small symbol after the file name indicating the report status
            file_status = helper.color_text("\u2713", "green")

            if len(report["warnings"]) > 0:
                file_status = helper.color_text("\u26A0", "yellow")

            if len(report["failures"]) > 0:
                file_status = helper.color_text("\u2717", "red")

            print("> " + file + " " + file_status)

            # Prints file failures
            if len(report["failures"]) > 0:
                failures = failures + 1
                for item, message in report["failures"]:
                    print(helper.color_text("  - " + item + ": " + message, "red"))

            # Prints file warnings
            if len(report["warnings"]) > 0:
                for item, message in report["warnings"]:
                    print(helper.color_text("  - " + item + ": " + message, "yellow"))

        return failures
