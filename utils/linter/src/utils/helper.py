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

@staticmethod
def color_text(text, color):
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
