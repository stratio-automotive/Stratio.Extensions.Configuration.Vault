from tabulate import tabulate

class ValidationReport:
    """
    A class to store validation report objects like successes, warnings, and failures.

    Attributes:
        files (dict): A dictionary of files with reports for each file.
        files[][successes] (list): A list to store success messages.
        files[][warnings] (list): A list to store warning messages.
        files[][failures] (list): A list to store failure messages.
    """

    def __init__(self):
        """
        Initialize the ValidationReport object with an empty dictionary of files
        that will later contain successes, warnings, and failures.
        """
        self.files = {}

    def add_file(self, filename):
        """
        Add a file to the report and sets the default state for successes, warnings and failures.

        Args:
            filename (str): The name of the file to be added to the report.
        """

        if filename in self.files:
            return

        self.files[filename] = {
            "successes" : [],
            "warnings": [],
            "failures": []
        }

    def add_success(self, filename, item, message):
        """
        Add a success message to the validation report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The success message to be added.
        """
        self.add_file(filename)
        self.files[filename]["successes"].append([item, message])

    def add_warning(self, filename, item, message):
        """
        Add a warning message to the validation report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The warning message to be added.
        """
        self.add_file(filename)
        self.files[filename]["warnings"].append([item, message])

    def add_failure(self, filename, item, message):
        """
        Add a failure message to the validation report.

        Args:
            filename (str): The name of the file the report entry belongs to.
            item (str): The identified placeholder item.
            message (str): The failure message to be added.
        """
        self.add_file(filename)
        self.files[filename]["failures"].append([item, message])

    @staticmethod
    def colored(text, color):
        """
        Wrap the text with ANSI escape codes to apply color.

        Args:
            text (str): The text to be colored.
            color (str): The color code. e.g., 'green', 'yellow', 'red'.

        Returns:
            str: The colored text.
        """
        colors = {
            'green': '\033[92m',   # Green
            'yellow': '\033[93m',  # Yellow
            'red': '\033[91m'      # Red
        }
        end_color = '\033[0m'     # Reset color
        return f"{colors[color]}{text}{end_color}"

    def print_report_table(self):
        """
        Print a pretty table with the validation report results.

        This method prints the collected successes, warnings, and failures for each file
        from the validation report in a well-formatted table using the tabulate library.
        """

        # Print a table for each of the files
        for filename, report_data in self.files.items():

            table = []

            # Add successes to the report table
            if report_data["successes"]:
                success_messages = "\n".join([f"{item}: {message}" for item, message in report_data["successes"]])
            else:
                success_messages = ""
            table.append([
                self.colored("Successes (" + str(len(report_data["successes"])) + ")", 'green'),
                self.colored(success_messages, 'green')]
            )

            # Add warnings to the report table
            if report_data["warnings"]:
                warning_messages = "\n".join([f"{item}: {message}" for item, message in report_data["warnings"]])
            else:
                warning_messages = ""
            table.append([
                self.colored("Warnings (" + str(len(report_data["warnings"])) + ")", 'yellow'),
                self.colored(warning_messages, 'yellow')]
            )

            # Add failures to the report table
            if report_data["failures"]:
                failure_messages = "\n".join([f"{item}: {message}" for item, message in report_data["failures"]])
            else:
                failure_messages = ""
            table.append([
                self.colored("Failures (" + str(len(report_data["failures"])) + ")", 'red'),
                self.colored(failure_messages, 'red')]
            )

            # Print table
            headers = ["Report", "Item"]
            print(f"\n=== {filename} ===")
            print(tabulate(table, headers=headers, tablefmt="fancy_grid"))

    def get_failures(self):
        """
            Returns a summary of the failures that were found

            Returns:
                list: A list with the failures organized by file
        """

        return_list = {}
        for filename, report_data in self.files.items():
            return_list[filename] = report_data["failures"]

        return return_list